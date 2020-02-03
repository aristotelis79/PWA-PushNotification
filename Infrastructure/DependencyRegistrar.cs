using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Progressive.Web.App.Data;
using Nop.Plugin.Progressive.Web.App.Domain;
using Nop.Plugin.Progressive.Web.App.Services;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Progressive.Web.App.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<ProgressiveWebPushService>().As<IProgressiveWebPushService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerServiceExtend>().As<ICustomerServiceExtend>().InstancePerLifetimeScope();

            //data context
            this.RegisterPluginDataContext<ProgressiveWebAppObjectContext>(builder, "nop_object_context_progressive_web_push");

            //override required repository with our custom context
            builder.RegisterType<EfRepository<SubscriptionRecord>>()
                .As<IRepository<SubscriptionRecord>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_progressive_web_push"))
                .InstancePerLifetimeScope();
        }

        public int Order => 10;
    }
}