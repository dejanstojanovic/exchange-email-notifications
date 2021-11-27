using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.Email.Notifications.Options
{
    public class ExchangeConfiguration
    {
        public Uri Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
