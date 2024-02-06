using Microsoft.Extensions.DependencyInjection.Extensions;
using TestWorkerService;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {


        services.AddSingleton<JokeService>();
      //  builder.Services.AddHostedService<WindowsBackgroundService>();
      //  services.AddHostedService<WindowsBackgroundService>();
      //  services.AddHostedService<TimedHostedService>();
        services.AddHostedService<TimeTrigger>();

    })
   
    //.ConfigureLogging((hostingContext, logging) =>
    //{
    //    logging.ClearProviders(); // Clear any existing providers

   
    //  //  logging.AddConsole("logfile.txt"); // Add a file logger provider
    //})
    .UseWindowsService(x =>
    {
        x.ServiceName = ".NET Joke Service";
      
    })
    .Build();

await host.RunAsync();
