using System;

namespace Exchange.Email.Notifications.Models
{
    public class AttachmentModel
    {
        public String Id { get; set; }
        public String Filename { get; set; }
        public byte[] Content { get; set; }
    }
}
