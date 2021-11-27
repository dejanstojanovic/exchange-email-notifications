using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.Email.Notifications.Models
{
    public class Attachment
    {
        public string Id { get; set; }
        public string Filename { get; set; }
        public int Size { get; set; }
    }
}
