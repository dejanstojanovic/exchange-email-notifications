using Exchange.Email.Notifications.Options;
using Exchange.Email.Notifications.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Exchange.Email.Notifications.Extensions
{
    public static class DependecyInjection
    {
        public static void AddExchangeEmailNotifications(this IServiceCollection services, IConfiguration configuration) 
        {
            services.Configure<ExchangeConfiguration>(configuration.GetSection("Exchange"));
            services.AddSingleton<IEmailNotificationService, EmailNotificationService>();
        }
    }
}
