namespace TestWorkerService
{
   

    public sealed class WindowsBackgroundService : BackgroundService
    {
        private readonly JokeService jokeService;
        private readonly ILogger<WindowsBackgroundService> logger;
        public WindowsBackgroundService(
        JokeService jokeService,
        ILogger<WindowsBackgroundService> logger)
        {
            this.jokeService = jokeService;
            this.logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    string joke = jokeService.GetJoke();
                    logger.LogWarning("{Joke}", joke);
                    using (StreamWriter writer = new StreamWriter("debug.txt", append: true))
                    {
                        // Write the log message along with a timestamp
                        writer.WriteLine($"{DateTime.Now.ToString()} - {joke}");
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // When the stopping token is canceled, for example, a call made from services.msc,
                // we shouldn't exit with a non-zero exit code. In other words, this is expected...
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{Message}", ex.Message);
                using (StreamWriter writer = new StreamWriter("debug.txt", append: true))
                {
                    // Write the log message along with a timestamp
                    writer.WriteLine($"{DateTime.Now.ToString()} - {ex.Message}");
                }
                // Terminates this process and returns an exit code to the operating system.
                // This is required to avoid the 'BackgroundServiceExceptionBehavior', which
                // performs one of two scenarios:
                // 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
                // 2. When set to "StopHost": will cleanly stop the host, and log errors.
                //
                // In order for the Windows Service Management system to leverage configured
                // recovery options, we need to terminate the process with a non-zero exit code.
                Environment.Exit(1);
            }
        }
    }
}