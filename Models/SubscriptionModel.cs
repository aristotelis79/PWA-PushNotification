using System;

namespace Nop.Plugin.Progressive.Web.App.Models
{
    public class SubscriptionModel
    {
        public int CustomerId { get; set; }

        public string Endpoint { get; set; }

        public DateTime? ExpirationTime { get; set; }

        public SubscriptionKeys Keys { get; set; }
    }

    public class SubscriptionKeys
    {
        public string P256dh { get; set; }

        public string Auth { get; set; }
    }
}