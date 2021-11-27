using Exchange.Email.Notifications.Models;
using Exchange.Email.Notifications.Options;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Exchange.Email.Notifications.Services
{

    public class EmailNotificationService : IEmailNotificationService
    {
        public event EventHandler<NewEmailMessageModel> OnNewEmailReceived;

        readonly ILogger<Worker> _logger;
        readonly ExchangeConfiguration _exchangeConfiguration;
        readonly ExchangeService _exchangeService;

        public EmailNotificationService(
            ILogger<Worker> logger,
            IOptions<ExchangeConfiguration> exchangeConfiguration
            )
        {
            _logger = logger;
            _exchangeConfiguration = exchangeConfiguration.Value;

            _exchangeService = new ExchangeService
            {
                Credentials = new WebCredentials(
                                username: _exchangeConfiguration.Username,
                                password: _exchangeConfiguration.Password),
                Url = _exchangeConfiguration.Url
            };
            AddNewMailSunbscription();
        }


        void AddNewMailSunbscription()
        {
            Folder rootfolder = Folder.Bind(_exchangeService, WellKnownFolderName.MsgFolderRoot);
            var folders = rootfolder.FindFolders(new FolderView(400)).AsEnumerable()
                .Where(f => f.DisplayName.Equals("NOR", StringComparison.InvariantCultureIgnoreCase))
                .ToArray();


            var StreamingSubscription = _exchangeService.SubscribeToStreamingNotifications(
                //folderIds: new FolderId[] { WellKnownFolderName.Inbox },
                folderIds: folders.Select(f => f.Id),
                EventType.NewMail
                );
            StreamingSubscriptionConnection connection = new StreamingSubscriptionConnection(_exchangeService, 30);
            connection.AddSubscription(StreamingSubscription);
            connection.OnNotificationEvent += OnNotificationEvent;
            connection.OnDisconnect += OnDisconnect;
            connection.Open();

        }

        private void OnDisconnect(object sender, SubscriptionErrorEventArgs args)
        {
            AddNewMailSunbscription();
        }

        private void OnNotificationEvent(object sender, NotificationEventArgs args)
        {
            foreach (ItemEvent @event in args.Events.Where(e => e.EventType == EventType.NewMail))
            {
                EmailMessage message = EmailMessage.Bind(_exchangeService, @event.ItemId);

                if (message.HasAttachments)
                {
                    Folder folder = Folder.Bind(_exchangeService, message.ParentFolderId);
                    var eventModel = new NewEmailMessageModel()
                    {
                        Subject = message.Subject,
                        Folder = folder.DisplayName
                    };

                    //var recepients = message.ToRecipients.AsEnumerable()
                    //                    .Where(r => r.Address.EndsWith(_exchangeConfiguration.Username, StringComparison.InvariantCultureIgnoreCase))
                    //                    .ToArray();

                    //if (!recepients.Any())
                    //    continue;

                    //TODO: Calculate the TLA

                    var fileAttachemnts = message.Attachments.Where(a => a is FileAttachment);
                    foreach (FileAttachment fileAttachemnt in fileAttachemnts)
                    {
                        using (var contentStream = new MemoryStream())
                        {
                            fileAttachemnt.Load(contentStream);
                            File.WriteAllBytes(Path.Combine(@"C:\Temp\attachemnts\", fileAttachemnt.Name), contentStream.ToArray());
                        }
                    }

                    if (this.OnNewEmailReceived != null)
                        OnNewEmailReceived(this, new NewEmailMessageModel() { Subject = "Test" });


                }
            }
        }
    }
}
