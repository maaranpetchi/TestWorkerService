using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestWorkerService.Data;
using TestWorkerService.Models;

namespace TestWorkerService
{
    internal class TimeTrigger : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly ILogger<TimeTrigger> _logger;
        private Timer? _timer = null;
     
        private bool _isTaskExecutedSuccessfully = false;
        private readonly IConfiguration _configuration;
        private DateTime _resetTime = DateTime.Now.AddMinutes(3);
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Dictionary<Guid, CancellationTokenSource> _taskCancellationTokens = new Dictionary<Guid, CancellationTokenSource>();


        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public TimeTrigger(ILogger<TimeTrigger> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogWarning("Timed Hosted Service running.");

            _timer = new Timer(async (state) => await DoWork(state, stoppingToken), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

            return Task.CompletedTask;
        }

        private async Task DoWork(object? state, CancellationToken cancellationToken1)
        {
            var taskId= Guid.NewGuid();
            var cancellationTokenSource = new CancellationTokenSource();
            _taskCancellationTokens[taskId] = cancellationTokenSource;

            var cancellationToken = cancellationTokenSource.Token;
            if (await _semaphore.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken))
            {
                try
            {
              
                    if (cancellationToken1.IsCancellationRequested)
                        return;
                    var now = DateTime.Now;
                    if (!_isTaskExecutedSuccessfully && !cancellationToken1.IsCancellationRequested)
                    {
                        var test = _configuration.GetSection("Report").GetValue<string>("Time").Split(":");
                        var resetInterval = _configuration.GetSection("Report").GetValue<int>("Reset");


                        if (now.Hour == Convert.ToInt32(test[0]) && now.Minute == Convert.ToInt32(test[1]))
                        {
                            _logger.LogWarning("Flow Started");
                            var stopwatch = new Stopwatch();
                            stopwatch.Start();
                          
                            using (var _ = new ExcelUpload(_logger, _configuration))
                            {
                                _logger.LogWarning("Excel import Starts");
                                var data = await _.ExcelImport();
                                _logger.LogWarning("Excel import Ends");

                                if (data>0)
                                {
                                    using (var _db = new DevTestingContext())
                                    {
                                        _logger.LogWarning("Run Employee Update Starts");
                                        var storedProcData = await _db.StoredProc_Result.FromSqlRaw("exec sp_RunUpdate").ToListAsync();
                                        _logger.LogWarning("Run Employee Update Ends");

                                        if(storedProcData.Any() && storedProcData.First().RowAffected > 0)
                                        {
                                            if (_configuration.GetSection("Report").GetValue<bool>("MailNotification"))
                                            {
                                                stopwatch.Stop();
                                                TimeSpan executionTime = stopwatch.Elapsed;
                                                _logger.LogWarning("Preparing to send mail...");
                                                using (var _service = new EmailService(_configuration, _logger))
                                                {
                                                    await _service.SendEmailAsync(data: storedProcData.First(), IsSuccess: true, exectime: executionTime);
                                                }
                                                _logger.LogWarning("Mail Sent...");
                                            }
                                            else
                                            {
                                                _logger.LogWarning("Mail notification not enabled!");
                                            }
                                            _isTaskExecutedSuccessfully = true;
                                            _resetTime = DateTime.Now.AddMinutes(resetInterval);
                                        }
                                        else
                                        {
                                            throw new Exception("An Error occured  While Updating the employee table  ");
                                        }
                                    }
                                }
                                else
                                {
                                    throw new Exception("An Error occured  While Reading the Excel");
                                }
                                
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Awaited for the time: [{0}:{1}]", test[0], test[1]);
                        }
                    }
                    if (now >= _resetTime)
                    {
                        _isTaskExecutedSuccessfully = false; // Reset the flag
                        _resetTime = _resetTime.AddHours(1); // Reset the reset time for the next day
                        _logger.LogWarning("Timer rescheduled");
                    }
                    else if (_isTaskExecutedSuccessfully)
                    {
                        _logger.LogWarning("Waiting For Reschedueled {0} sec Remaining", _resetTime - DateTime.Now);
                    }
                  //  throw new Exception("Manual Exception");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the task.");
                using (EmailService _service = new EmailService(_configuration, _logger))
                {
                    await _service.SendEmailAsync(IsSuccess: false, ExMessage: ex.Message);
                }
            }
            finally
            {
                _semaphore.Release();

                }
            }
            else
            {
                try
                {
                  //  _logger.LogWarning("Timeout occurred while waiting for the semaphore.{0}", Task.CurrentId);
                    CancelSpecificTask(taskId);
                }
                // Handle timeout (e.g., log, throw exception, etc.)
               catch(OperationCanceledException exc)
                {
                    _logger.LogWarning("Timeout occurred while waiting for the semaphore.{0}", exc.Message);
                }
            }
        }

        public void CancelSpecificTask(Guid taskId)
        {
            if (_taskCancellationTokens.TryGetValue(taskId, out var taskCancellationTokenSource))
            {
                taskCancellationTokenSource.Cancel();
                _taskCancellationTokens.Remove(taskId);
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogWarning("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
