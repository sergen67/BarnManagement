using BarnManagement.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Windows.Forms;


namespace BarnManagement
{
    internal static class Program
    {
        public static ServiceProvider Services;
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            Directory.CreateDirectory(logDir);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .WriteTo.File(Path.Combine(logDir, "log-.txt"), rollingInterval: RollingInterval.Day, shared: true, retainedFileCountLimit: 14,outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] {Level:u4} - {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            var services = new ServiceCollection();
                services.AddLogging(builder => { builder.ClearProviders();
                    builder.AddSerilog(dispose: true);
                });
            services.AddTransient<RegisterForm>();
            services.AddTransient<LoginForm>();
            services.AddTransient<MainForm>();

            Services = services.BuildServiceProvider();
            // DOĞRU
            var loggerFactory = Services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(nameof(Program)); // kategori: "Program"
            logger.LogInformation("App starting (RegisterForm-only).");

            logger.LogInformation("Application starting...");

            Application.Run(Services.GetRequiredService<LoginForm>());


        }
    }
}
