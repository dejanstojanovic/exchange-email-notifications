using Exchange.Email.Notifications.Exceptions;
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
        readonly IDictionary<String, Folder> _folders;

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
            var folders = new Dictionary<String, Folder>();

            if (_exchangeConfiguration.Folders == null && _exchangeConfiguration.Folders.Any())
                folders.Add(Enum.GetName(typeof(WellKnownFolderName), WellKnownFolderName.Inbox), Folder.Bind(_exchangeService, WellKnownFolderName.Inbox));
            else
            {
                foreach (var folder in _exchangeConfiguration.Folders)
                {
                    Folder currentFolder = Folder.Bind(_exchangeService, WellKnownFolderName.MsgFolderRoot);
                    var path = folder.Split('/');
                    foreach (var segment in path)
                    {
                        currentFolder = currentFolder.FindFolders(new SearchFilter.IsEqualTo(FolderSchema.DisplayName, segment), new FolderView(1)).AsEnumerable().SingleOrDefault();
                        if (currentFolder == null)
                            throw new FolderNotFoundException();
                    }

                    folders.Add(folder, currentFolder);
                }

                _folders = folders;
            }

            AddNewMailSunbscription();
        }


        void AddNewMailSunbscription()
        {
            var StreamingSubscription = _exchangeService.SubscribeToStreamingNotifications(
                folderIds: _folders.Select(f => f.Value.Id),
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
                        Id = message.Id.UniqueId,
                        Subject = message.Subject,
                        From = message.From.Address,
                        DateTimeSent = message.DateTimeSent,
                        Attachments = message.Attachments.Where(a => a is FileAttachment).Select(a => a.Name).ToArray()
                    };

                    if (this.OnNewEmailReceived != null)
                        OnNewEmailReceived(this, eventModel);


                    //var fileAttachemnts = message.Attachments.Where(a => a is FileAttachment);
                    //foreach (FileAttachment fileAttachemnt in fileAttachemnts)
                    //{
                    //    using (var contentStream = new MemoryStream())
                    //    {
                    //        fileAttachemnt.Load(contentStream);
                    //        File.WriteAllBytes(Path.Combine(@"C:\Temp\attachemnts\", fileAttachemnt.Name), contentStream.ToArray());
                    //    }
                    //}

                }
            }
        }
    }
}
