using System;
using System.Collections.Generic;

namespace Exchange.Email.Notifications.Options
{
    public class ExchangeConfiguration
    {
        public Uri Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public IEnumerable<String> Folders { get; set; }
    }
}
