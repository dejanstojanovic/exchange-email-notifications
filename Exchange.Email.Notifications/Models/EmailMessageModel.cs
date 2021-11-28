using System;
using System.Collections.Generic;

namespace Exchange.Email.Notifications.Models
{
    public class EmailMessageModel
    {
        public string Id { get; set; }
        public string Subject { get; set; }
        public string Folder { get; set; }
        public String From { get; set; }
        public DateTime DateTimeSent { get; set; }
        public string Body { get; set; }
        public IEnumerable<EmailMessageAttachmentModel> Attachments { get; set; }
    }
}
