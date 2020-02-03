using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Data;
using Nop.Plugin.Progressive.Web.App.Domain;

namespace Nop.Plugin.Progressive.Web.App.Services
{
    public class ProgressiveWebPushService : IProgressiveWebPushService
    {
        private readonly IRepository<SubscriptionRecord> _subscritionRepository;

        public ProgressiveWebPushService(IRepository<SubscriptionRecord> subscritionRepository)
        {
            _subscritionRepository = subscritionRepository;
        }

        public SubscriptionRecord GetSubscriptionByCustomerId(int customerId)
        {
            if (customerId <= 0)
                throw new Exception("Invalid customerId");
            return _subscritionRepository.Table.FirstOrDefault(x => x.CustomerId == customerId);
        }

        public List<SubscriptionRecord> GetSubscriptionByCustomerIds(int[] customerIds)
        {
            if (customerIds == null)
                throw new NullReferenceException("Null customerIds");
            return _subscritionRepository.TableNoTracking.Where(x => customerIds.Contains(x.CustomerId)).ToList();
        }

        public List<int> GetSubscriptionsCustomerIds()
        {
            return _subscritionRepository.TableNoTracking.Select(x => x.CustomerId).ToList();
        }

        public void CreateSubscription(SubscriptionRecord subscriptionRecord)
        {
            if (subscriptionRecord == null)
                throw new ArgumentNullException(nameof(subscriptionRecord));

            _subscritionRepository.Insert(subscriptionRecord);
        }

        public void RemoveSubscription(SubscriptionRecord subscriptionRecord)
        {
            if (subscriptionRecord == null)
                throw new ArgumentNullException(nameof(subscriptionRecord));

            _subscritionRepository.Delete(subscriptionRecord);
        }

        public void UpdateSuscription(SubscriptionRecord subscriptionRecord)
        {
            if (subscriptionRecord == null)
                throw new ArgumentNullException(nameof(subscriptionRecord));

            _subscritionRepository.Update(subscriptionRecord);
        }

        public void RemoveSubscriptionByCustomerId(int customerId)
        {
            if (customerId <= 0)
                throw new Exception("Invalid customerId");

            RemoveSubscription(GetSubscriptionByCustomerId(customerId));
        }
    }
}