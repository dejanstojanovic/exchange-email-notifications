using AutoMapper;
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

    public class EmailService : IEmailService
    {
        public event EventHandler<NewEmailMessageModel> OnNewEmailReceived;

        readonly ILogger<Worker> _logger;
        readonly ExchangeConfiguration _exchangeConfiguration;
        readonly ExchangeService _exchangeService;
        readonly IDictionary<String, Folder> _folders;
        readonly IMapper _mapper;

        public EmailService(
            ILogger<Worker> logger,
            IOptions<ExchangeConfiguration> exchangeConfiguration,
            IMapper mapper
            )
        {
            _mapper = mapper;
            _logger = logger;
            _exchangeConfiguration = exchangeConfiguration.Value;

            _exchangeService = new ExchangeService
            {
                Credentials = new WebCredentials(
                                username: _exchangeConfiguration.Username,
                                password: _exchangeConfiguration.Password),
                                Url = _exchangeConfiguration.Url != null ? _exchangeConfiguration.Url : new Uri(_exchangeConfiguration.EXCHANGE_SERVICE_URL)
            };
            var folders = new Dictionary<String, Folder>();

            if (_exchangeConfiguration.Folders == null || !_exchangeConfiguration.Folders.Any())
                _exchangeConfiguration.Folders = new string[] { "Inbox" };

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
                EmailMessage message = EmailMessage.Bind(
                    _exchangeService,
                    @event.ItemId,
                    new PropertySet(
                        ItemSchema.Id,
                        ItemSchema.Subject,
                        ItemSchema.DateTimeSent,
                        ItemSchema.Attachments,
                        ItemSchema.HasAttachments,
                        ItemSchema.DisplayTo,
                        ItemSchema.ParentFolderId,
                        EmailMessageSchema.From));

                Folder folder = Folder.Bind(_exchangeService, message.ParentFolderId);
                var eventModel = _mapper.Map<NewEmailMessageModel>(message);
                eventModel.Folder = CalculateFolderPath(message.ParentFolderId);

                if (this.OnNewEmailReceived != null)
                    OnNewEmailReceived(this, eventModel);

            }
        }

        public AttachmentModel GetAttachment(string id)
        {
            var attachemnt = _exchangeService.GetAttachments(new String[] { id }, BodyType.HTML, new PropertySet(ItemSchema.Attachments))
                                        .AsEnumerable()
                                        .Where(a => a.Attachment is FileAttachment)
                                        .Select(a => a.Attachment as FileAttachment)
                                        .SingleOrDefault();
            if (attachemnt == null)
                return null;

            using (var contentStream = new MemoryStream())
            {
                attachemnt.Load(contentStream);
                return new AttachmentModel()
                {
                    Content = contentStream.ToArray(),
                    Filename = attachemnt.Name,
                    Id = attachemnt.Id
                };
            }

        }

        public EmailMessageModel GetEmailMessage(string id)
        {
            var item = _exchangeService.BindToItems(
                new ItemId[] { new ItemId(id) },
                new PropertySet(ItemSchema.Id))
                .AsEnumerable()
                .Select(i => i.Item)
                .SingleOrDefault();

            if (item == null)
                return null;

            var email = EmailMessage.Bind(_exchangeService, item.Id,
                new PropertySet(
                        ItemSchema.Id,
                        ItemSchema.Subject,
                        ItemSchema.DateTimeSent,
                        ItemSchema.Attachments,
                        ItemSchema.HasAttachments,
                        ItemSchema.ParentFolderId,
                        EmailMessageSchema.Body,
                        EmailMessageSchema.From));

            var messageModel = _mapper.Map<EmailMessageModel>(email);
            messageModel.Folder = CalculateFolderPath(email.ParentFolderId);
            
            return messageModel;
        }

        string CalculateFolderPath(FolderId folderId)
        {
            var propertySet = new PropertySet(FolderSchema.Id,
                                FolderSchema.ParentFolderId,
                                FolderSchema.DisplayName);

            var root = Folder.Bind(_exchangeService, WellKnownFolderName.MsgFolderRoot, propertySet);

            IList<Folder> folders = new List<Folder>();
            var folder = Folder.Bind(_exchangeService, folderId, propertySet);

            folders.Add(folder);

            if (folder.ParentFolderId != null)
                addParent(folder.ParentFolderId);

            void addParent(FolderId id)
            {
                var parentFolder = Folder.Bind(_exchangeService, id, propertySet);

                if (!parentFolder.Id.UniqueId.Equals(root.Id.UniqueId))
                {
                    folders.Insert(0, parentFolder);
                    if (parentFolder.ParentFolderId != null && !folders.Any(f => f.Id.UniqueId == parentFolder.ParentFolderId.UniqueId))
                    {
                        addParent(parentFolder.ParentFolderId);
                    }
                }
            }

            return string.Join("/", folders.Select(f => f.DisplayName));
        }

        public void MarkAsRead(string id)
        {
            var message = EmailMessage.Bind(_exchangeService, new ItemId(id), new PropertySet()
            {
                EmailMessageSchema.Id,
                EmailMessageSchema.ParentFolderId,
                EmailMessageSchema.IsRead,
            });
            message.IsRead = true;

            _exchangeService.UpdateItems(new Item[] { message }, message.ParentFolderId, ConflictResolutionMode.AutoResolve, MessageDisposition.SaveOnly, null);

        }
    }
}
