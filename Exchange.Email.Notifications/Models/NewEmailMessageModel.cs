using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.Email.Notifications.Models
{
    public class NewEmailMessageModel
    {
        public string Subject { get; set; }
        public string Folder { get; set; }

        public IEnumerable<Attachment> Attachments { get; set; }
    }
}
