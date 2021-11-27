using Exchange.Email.Notifications.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using Threading = System.Threading.Tasks;

namespace Exchange.Email.Notifications
{
    public class Worker : BackgroundService
    {
        readonly ILogger<Worker> _logger;
        readonly IEmailNotificationService _emailNotificationService;

        public Worker(
            ILogger<Worker> logger, 
            IEmailNotificationService emailNotificationService)
        {
            _logger = logger;
            _emailNotificationService = emailNotificationService;

            _emailNotificationService.OnNewEmailReceived += OnNewEmailReceived;
        }

        private void OnNewEmailReceived(object sender, Models.NewEmailMessageModel e)
        {
            
        }
       

        protected override async Threading.Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Threading.Task.CompletedTask;
        }
    }
}
