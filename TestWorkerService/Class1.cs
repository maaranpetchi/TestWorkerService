//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics.Eventing.Reader;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using TestWorkerService.Data;
//using TestWorkerService.Models;

//namespace TestWorkerService
//{
//    public class TimedHostedService : IHostedService, IDisposable
//    {
//        private int executionCount = 0;
//        private readonly ILogger<TimedHostedService> _logger;
//        private Timer? _timer = null;
//        private readonly object _lock = new object();
//        private bool isCompleted = false;
//        private readonly IConfiguration _configuration;
//        private DateTime Reset=DateTime.Now;
//        public TimedHostedService(ILogger<TimedHostedService> logger,IConfiguration configuration)
//        {
//            _configuration = configuration;
//            _logger = logger;
//        }

//        public Task StartAsync(CancellationToken stoppingToken)
//        {
//            _logger.LogWarning("Timed Hosted Service running.");

//            _timer = new Timer(async (state) => await DoWork(state, stoppingToken), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

//            return Task.CompletedTask;
//        }

//        private async Task DoWork(object? state, CancellationToken cancellationToken)
//        {
//           // _logger.LogWarning("Timed Hosted Service running. {0}",DateTime.Now);
//            var test = _configuration.GetSection("Report").GetValue<string>("Time").Split(":");
//            var ResetInterval = _configuration.GetSection("Report").GetValue<int>("Reset");
          
//            lock (_lock){
//                if (!isCompleted) {

//                try
//                {
//                    var now = DateTime.Now;
//                    if (now.Hour == Convert.ToInt32(test[0]) && now.Minute == Convert.ToInt32(test[1]))
//                    {
//                          Reset = now.AddMinutes(ResetInterval);

//                        if (cancellationToken.IsCancellationRequested)
//                        {
//                            return;
//                        }
//                            _logger.LogWarning("Flow Started");
//                            Task.Run(async () =>
//                            {
                          
//                                using (var _ = new ExcelUpload(_logger,_configuration))
//                                {

//                                    _logger.LogWarning("Excel import Starts");
//                                    var data = await _.ExcelImport();
//                                    _logger.LogWarning("Excel import Ends");
//                                    return data;
//                                }

//                            }).ContinueWith (async(d) => {
//                                if (d.Exception == null )
//                                  {
//                                d.Result.ToString();
//                                    using (var _db = new DevTestingContext())
//                                    {

//                                        _logger.LogWarning("Run Employee Update Starts");
//                                        var data = await _db.StoredProc_Result.FromSqlRaw("exec sp_RunUpdate").ToListAsync();
//                                        _logger.LogWarning("Run Employeee Update Ends");
//                                        return data;
//                                    }
//                              }
//                                else
//                                {
//                                   return new List<StoredProc_Result>();
//                                }
                            
                       
//                        }).ContinueWith (async (t) => {

//                            if (_configuration.GetSection("Report").GetValue<bool>("MailNotification"))
//                            {
//                                _logger.LogWarning("Preparing to send mail...");
//                                if (t.Exception == null )

//                            {
//                                using (EmailService _service = new EmailService(_configuration,_logger))
//                                {
//                                    await _service.SendEmailAsync(data: t?.Result?.Result?.First(), IsSuccess: true, exectime: (float)1.45);
//                                }
//                            }
//                            else
//                            {
//                                using (EmailService _service = new EmailService(_configuration, _logger))
//                                {
//                                    await _service.SendEmailAsync(data: null, IsSuccess: false, ExMessage: t.Exception.Message);
//                                }
//                            }
//                                _logger.LogWarning("Mail Sended...");
//                            }
//                            else
//                            {
//                                _logger.LogWarning("Mail notification not enabled!");
//                            }
                          
//                        }).GetAwaiter ().GetResult();
//                            _logger.LogWarning("Ends");
                       
//                    }
//                        else
//                        {
//                            _logger.LogWarning("Awaited for the time : [{0}:{1}]", test[0], test[1]);
//                        }
//                }
//                catch (Exception ex)
//                {

//                }
//                finally {
//                    isCompleted = true;
//                }
//                }
//                else
//                {
//                    if(Reset.Hour==DateTime.Now.Hour && Reset.Minute == DateTime.Now.Minute)
//                    {
//                        isCompleted = false;
//                        _logger.LogWarning("Timer Reseted Successfully {0}", DateTime.Now);
//                    }

//                }
//            }
            

//        }


//        public Task StopAsync(CancellationToken stoppingToken)
//        {
//            _logger.LogWarning("Timed Hosted Service is stopping.");

//            _timer?.Change(Timeout.Infinite, 0);
          
//            return Task.CompletedTask;
//        }

//        public void Dispose()
//        {
//            _timer?.Dispose();
//        }
//    }
//}
