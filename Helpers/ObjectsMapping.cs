using Nop.Plugin.Progressive.Web.App.Domain;
using Nop.Plugin.Progressive.Web.App.Models;

namespace Nop.Plugin.Progressive.Web.App.Helpers
{
    public static class ObjectsMapping
    {
        public static SubscriptionModel ToPushSubscription(SubscriptionRecord record)
        {
            return new SubscriptionModel
            {
                CustomerId = record.CustomerId,
                Endpoint = record.Endpoint,
                ExpirationTime = record.ExpirationTime,
                Keys = new SubscriptionKeys
                {
                    P256dh = record.P256DHKey,
                    Auth = record.AuthKey
                }
            };
        }

        public static SubscriptionRecord ToSubscriptionRecord(this SubscriptionModel subscriptionModel, int customerId)
        {
            return new SubscriptionRecord
            {
                CustomerId = customerId,
                Endpoint = subscriptionModel.Endpoint,
                ExpirationTime = subscriptionModel.ExpirationTime,
                P256DHKey = subscriptionModel.Keys.P256dh,
                AuthKey = subscriptionModel.Keys.Auth
            };
        }
    }
}