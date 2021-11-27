using Exchange.Email.Notifications.Models;
using System;

namespace Exchange.Email.Notifications.Services
{
    public interface IEmailNotificationService
    {
        event EventHandler<NewEmailMessageModel> OnNewEmailReceived;
    }
}
