using BarnManagement.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using System.Windows.Forms;


namespace BarnManagement
{
   
    internal class Program
    {
        public static IServiceProvider Services;
      
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var services = new ServiceCollection();
            var logs = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            Directory.CreateDirectory(logs);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(Path.Combine(logs, "log-.txt"),
                              rollingInterval: RollingInterval.Day,
                              retainedFileCountLimit: 14,
                              shared: true,
                              outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] {Level:u3} {SourceContext} - {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            services.AddLogging(b => { b.ClearProviders(); b.AddSerilog(dispose: true); });
            services.AddTransient<RegisterForm>();
            services.AddTransient<LoginForm>();
            services.AddTransient<MainForm>();


            var provider = services.BuildServiceProvider();
            Services = provider;

            try
            {
                var logger = provider.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Uygulama başlıyor.....");

                Application.Run(provider.GetRequiredService<LoginForm>());

            }
            finally
            {
                (provider as IDisposable).Dispose();
                Log.CloseAndFlush();
            }
            
            
        




        }
    }
}
