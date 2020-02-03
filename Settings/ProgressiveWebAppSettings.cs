using Nop.Core.Configuration;

namespace Nop.Plugin.Progressive.Web.App.Settings
{
    public class ProgressiveWebAppSettings : ISettings
    {
        public string ProgressiveWebAppCode { get; set; }

        public string ProgressiveWebAppHeaderTags { get; set; }

        public string PushNotificationHtml { get; set; }

        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
    }
}