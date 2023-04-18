// See https://aka.ms/new-console-template for more information
using Base10.SparkplugB.ApplicationDemo;
using Base10.SparkplugB.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

Host.CreateDefaultBuilder()
	.ConfigureAppConfiguration((hostingContext, config) =>
	{
		config.AddJsonFile("appsettings.json", optional: true);
		config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true);
		config.AddJsonFile($"appsettings.secrets.json", optional: true);
		config.AddEnvironmentVariables();
	})
	.ConfigureLogging((hostingContext, logging) =>
	{
		Log.Logger = new LoggerConfiguration()
			.ReadFrom.Configuration(hostingContext.Configuration)
			.CreateLogger();
	})
	.ConfigureServices((hostContext, services) =>
	{
		services.Configure<SparkplugServiceOptions>(hostContext.Configuration.GetSection("SparkplugServiceOptions"));
		services.AddHostedService<SparkplugExampleService>();
		services.AddTransient<SparkplugListener>();
	})
	.UseSerilog()
	.Build()
	.Run();
Console.WriteLine("Shutting down...");
