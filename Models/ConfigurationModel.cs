using Nop.Web.Framework.Mvc.ModelBinding;


namespace Nop.Plugin.Progressive.Web.App.Models
{
    public class ConfigurationModel
    {
        [NopResourceDisplayName("Admin.Plugins.ProgressiveWebApp.Web.App.Code")]
        //[AllowHtml] 
        public string ProgressiveWebAppCode { get; set; }

        [NopResourceDisplayName("Admin.Plugins.ProgressiveWebApp.Web.HeaderTags")]
        //[AllowHtml]
        public string ProgressiveWebAppHeaderTags { get; set; }

        [NopResourceDisplayName("Admin.Plugins.ProgressiveWebApp.Web.Push.Notification.Html")]
        //[AllowHtml]
        public string PushNotificationHtml { get; set; }

        [NopResourceDisplayName("Admin.Plugins.ProgressiveWebApp.Web.Push.PublicKey")]
        public string PublicKey { get; set; }

        [NopResourceDisplayName("Admin.Plugins.ProgressiveWebApp.Web.Push.PrivateKey")]
        public string PrivateKey { get; set; }
    }
}