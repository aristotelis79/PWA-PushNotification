using System;

namespace Nop.Plugin.Progressive.Web.App.Models
{
    public class SentNotificationModel
    {
        public string SelectedIds { get; set; }

        public int OfferId { get; set; }

        public OfferType OfferType { get; set; }

        public NotificationType NotificationType { get; set; }
        
        public string OfferName { get; set; }

        public string OfferSeName { get; set; }

        public string Price { get; set; }

        public string ImageUrl { get; set; }
    }
}