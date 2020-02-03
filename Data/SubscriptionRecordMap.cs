using Nop.Data.Mapping;
using Nop.Plugin.Progressive.Web.App.Domain;

namespace Nop.Plugin.Progressive.Web.App.Data
{
    public class SubscriptionRecordMap : NopEntityTypeConfiguration<SubscriptionRecord>
    {
        public SubscriptionRecordMap()
        {
            this.ToTable("WebPushSubscriptions");
            this.HasKey(x => x.Id);
        }
    }
}