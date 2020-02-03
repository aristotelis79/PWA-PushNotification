using System;
using Nop.Core;

namespace Nop.Plugin.Progressive.Web.App.Domain
{
    public class SubscriptionRecord : BaseEntity
    {

        public int CustomerId { get; set; }

        public string Endpoint { get; set; }

        public DateTime? ExpirationTime { get; set; }

        public string P256DHKey { get; set; }

        public string AuthKey { get; set; }
    }
}