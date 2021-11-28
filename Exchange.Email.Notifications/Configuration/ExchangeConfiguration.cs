using System;
using System.Collections.Generic;
using System.Linq;

namespace Exchange.Email.Notifications.Options
{
    public class ExchangeConfiguration
    {
        public readonly string EXCHANGE_SERVICE_URL = "https://outlook.office365.com/EWS/Exchange.asmx";
        public Uri Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public IEnumerable<String> Folders { get; set; }


    }
}
