using Exchange.Email.Notifications.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using Threading = System.Threading.Tasks;

namespace Exchange.Email.Notifications
{
    public class Worker : BackgroundService
    {
        readonly ILogger<Worker> _logger;
        readonly IEmailService _emailService;

        public Worker(
            ILogger<Worker> logger, 
            IEmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;

            _emailService.OnNewEmailReceived += OnNewEmailReceived;
        }

        private void OnNewEmailReceived(object sender, Models.NewEmailMessageModel e)
        {
            _logger.LogInformation("Message received");
            var fullMessage = _emailService.GetEmailMessage(e.Id);

            if (e.Attachments != null && e.Attachments.Any()) {
                foreach (var messageAttachment in e.Attachments)
                {
                    var attachment = _emailService.GetAttachment(messageAttachment.Id);
                }
            }

            _emailService.MarkAsRead(e.Id);
        }
       

        protected override async Threading.Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Threading.Task.CompletedTask;
        }
    }
}
