using System;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace ScheduleMail
{
    public class Program
    {
        public static IConfiguration Configuration;

        public static void Main(string[] args)
        {
            Configuration = new ConfigurationBuilder().AddJsonFile(Environment.CurrentDirectory + "/appsettings.json").Build();
            var host = CreateWebHostBuilder(args).Build();

            GlobalConfiguration.Configuration.UseMemoryStorage();
            RecurringJob.AddOrUpdate<RecurringMailHelper>("recurringMail", x => x.NewSession(), Cron.Minutely);

            using (var server = new BackgroundJobServer())
            {
                Console.WriteLine("Hangfire Server started. Press any key to exit...");
                host.Run();
            }
        }
        public class RecurringMailHelper
        {
            public static string LastId { get; set; } = "";
            public void NewSession()
            {
                if(LastId != "")
                {
                    BackgroundJob.Delete(LastId);
                }

                BackgroundJob.Enqueue<MailSchedule>(x => x.StartCheckingQueue(null));
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args).UseStartup<Startup>();

    }
}
