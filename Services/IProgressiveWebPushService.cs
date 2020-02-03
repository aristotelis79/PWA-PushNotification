using System.Collections.Generic;
using Nop.Plugin.Progressive.Web.App.Domain;

namespace Nop.Plugin.Progressive.Web.App.Services
{
    public interface IProgressiveWebPushService
    {
        SubscriptionRecord GetSubscriptionByCustomerId(int customerId);
        void CreateSubscription(SubscriptionRecord subscriptionRecord);
        void RemoveSubscriptionByCustomerId(int customerId);
        void RemoveSubscription(SubscriptionRecord subscriptionRecord);
        void UpdateSuscription(SubscriptionRecord sub);
        List<SubscriptionRecord> GetSubscriptionByCustomerIds(int[] customerIds);
        List<int> GetSubscriptionsCustomerIds();
    }
}