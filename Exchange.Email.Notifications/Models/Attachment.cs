using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.Email.Notifications.Models
{
    public class Attachment
    {
        public string Filename { get; set; }
        public byte[] Content { get; set; }
    }
}
