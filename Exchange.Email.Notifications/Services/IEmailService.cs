using Exchange.Email.Notifications.Models;
using System;

namespace Exchange.Email.Notifications.Services
{
    public interface IEmailService
    {
        event EventHandler<NewEmailMessageModel> OnNewEmailReceived;
        AttachmentModel GetAttachment(string id);
        EmailMessageModel GetEmailMessage(string id);
        void MarkAsRead(string id);
    }
}
