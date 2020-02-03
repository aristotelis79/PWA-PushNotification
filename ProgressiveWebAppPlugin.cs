using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Plugins;
using Nop.Plugin.Progressive.Web.App.Data;
using Nop.Plugin.Progressive.Web.App.Security;
using Nop.Plugin.Progressive.Web.App.Settings;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using System.Net.Mail;
using System.Text;
using Nop.Core.Domain.Messages;
using Nop.Core.Infrastructure;
using Nop.Services.Messages;


namespace Nop.Plugin.Progressive.Web.App
{
    public class ProgressiveWebAppPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin
    {
        #region fields
        private readonly ISettingService _settingService;
        private readonly ProgressiveWebAppSettings _progressiveWebAppSettings;
        private readonly ProgressiveWebAppObjectContext _progressiveWebAppObjectContext;
        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerService _customerService;


        #endregion

        #region ctor
        public ProgressiveWebAppPlugin(ISettingService settingService, ProgressiveWebAppSettings progressiveWebAppSettings, ProgressiveWebAppObjectContext progressiveWebAppObjectContext, IWebHelper webHelper, IPermissionService permissionService, ICustomerService customerService)
        {
            _progressiveWebAppSettings = progressiveWebAppSettings;
            _progressiveWebAppObjectContext = progressiveWebAppObjectContext;
            _webHelper = webHelper;
            _permissionService = permissionService;
            _customerService = customerService;
            _settingService = settingService;
        }
        #endregion

        #region plugin methods
        public IList<string> GetWidgetZones()
        {
          return new List<string>()
          {
              "head_html_tag",
              "body_end_html_tag_before",
              "header_selectors"
          };
        }

        public void GetPublicViewComponent(string widgetZone, out string viewComponentName)
        {
            switch (widgetZone)
            {
                case "head_html_tag":
                    viewComponentName = "ProgressiveWebAppHeaderTags";
                    break;
                case "body_end_html_tag_before":
                    viewComponentName = "ProgressiveWebAppCode";
                    break;
                case "header_selectors":
                    viewComponentName = "PushNotification";
                    break;
                default:
                    viewComponentName = "";
                    break;
            }
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                SystemName = "Admin.ProgressiveWebApp.Send.Offer",
                Title = "Web Push Notifications",
                ControllerName = "AdminWebPush",
                ActionName = "List",
                Visible = true,
                RouteValues = new RouteValueDictionary() { { "area", "admin" } },
            };

            var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Third party plugins");
            if (pluginNode != null)
            {
                pluginNode.ChildNodes.Add(menuItem);
            }
            else
            {
                rootNode.ChildNodes.Add(menuItem);
            }
        }

        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/AdminWebPush/Configure";
        }
        #endregion

        #region installation
        public override void Install()
        {
            NotifyMe();

            InstallProgressiveSystemRole();

            _permissionService.InstallPermissions(new ProgressivePermissionProvider());

            this.AddOrUpdatePluginLocaleResource("Admin.Common.Or", "Or");
            this.AddOrUpdatePluginLocaleResource("Admin.Common.Name", "Name");
            this.AddOrUpdatePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.HasShoppingCart", "Has Cart Items");
            this.AddOrUpdatePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.HasWishlist", "Has Wishlist Items");
            this.AddOrUpdatePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Send.Offer", "Send Offer Notification");
            this.AddOrUpdatePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Send.Offer.Button", "Send Notification");
            this.AddOrUpdatePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.HasOfferInShoppingCartOrWishlist", "Has Offer Type To Cart Or Wishlist");
            this.AddOrUpdatePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.HasSubscription","Has Active Subscription");
            this.AddOrUpdatePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Offer.Type", "Offer Type For Send");
            this.AddOrUpdatePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Product.AddNew", "Add Product To Offer");
            this.AddOrUpdatePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Category.AddNew", "Add Category To Offer");
            this.AddOrUpdatePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Customers", "Customers To Send Offer");
            this.AddOrUpdatePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Products.AddToOffer", "Add Product To Offer");
            this.AddOrUpdatePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Category.AddToOffer", "Add Category To Offer");
            this.AddOrUpdatePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Category.Fields.Name" , "Name");
            this.AddOrUpdatePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Category.Fields.Published", "Published");
            this.AddOrUpdatePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Web.App.Code","Html Script-CSS Sources");
            this.AddOrUpdatePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Web.HeaderTags","Html Header Tag Sources");
            this.AddOrUpdatePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Web.Push.Notification.Html","Html For Notification Icon Header");
            this.AddOrUpdatePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Save.Config.Success","Configuration Saved Succefully");
            this.AddOrUpdatePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Web.Push.PublicKey","Your WebPush PublicKey");
            this.AddOrUpdatePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Web.Push.PrivateKey","Your WebPush PrivateKey");

            var rootPluginFolder = "/Plugins/Progressive.Web.App";

            var progressiveWebAppCode = $@"<script src ='{rootPluginFolder}/Content/Scripts/node_modules/sweetalert2/dist/sweetalert2.min.js' type='text/javascript'></script>
                                            <script src ='{rootPluginFolder}/Content/Scripts/node_modules/localforage/dist/localforage.min.js' type='text/javascript'></script>
                                            <script src ='{rootPluginFolder}/Content/Scripts/pwa-site.js' type='text/javascript'></script>
                                            <script src ='{rootPluginFolder}/Content/Scripts/pwa-push-notification.js' type='text/javascript'></script>
                                            <link href ='{rootPluginFolder}/Content/Scripts/node_modules/sweetalert2/dist/sweetalert2.min.css' rel='stylesheet' type='text/css'>
                                            <link href ='{rootPluginFolder}/Content/Fonts/font-awesome-4.7.0/css/font-awesome.min.css' rel='stylesheet' type='text/css'>
                                            <link href ='{rootPluginFolder}/Content/Css/site.css' rel='stylesheet' type='text/css'>";


            var progressiveWebAppHeaderTags = $@"<link rel='manifest' href='{rootPluginFolder}/Content/manifest.json'>
                                                <link rel='apple-touch-icon' sizes='180x180' href='{rootPluginFolder}/Content/Icons/apple-touch-icon.png'>
                                                <link rel='icon' type='image/png' sizes='32x32' href='{rootPluginFolder}/Content/Icons/favicon-32x32.png'>
                                                <link rel='icon' type='image/png' sizes='16x16' href='{rootPluginFolder}/Content/Icons/favicon-16x16.png'>
                                                <link rel='mask-icon' href='{rootPluginFolder}/Content/Icons/safari-pinned-tab.svg' color='#286893'>
                                                <meta name='theme-color' content = '#286893'> ";

            var pushNotificationHtml = @"<div id='notifybtn'><i id='notifyicon' class='fa'></i></div>
                                         <input type='hidden' id ='push-notification-publickey' name='push-notification-publickey' value='{push-notification-publickey-value}'/>";
            _progressiveWebAppSettings.ProgressiveWebAppHeaderTags = progressiveWebAppHeaderTags;
            _progressiveWebAppSettings.ProgressiveWebAppCode = progressiveWebAppCode;
            _progressiveWebAppSettings.PushNotificationHtml = pushNotificationHtml;
            _settingService.SaveSetting(_progressiveWebAppSettings);

            _progressiveWebAppObjectContext.Install();

            base.Install();
        }

        private void NotifyMe()
        {
            var storeContext = EngineContext.Current.Resolve<IStoreContext>();
            var emailAccountService = EngineContext.Current.Resolve<IEmailAccountService>();
            var emailAccountSettings = EngineContext.Current.Resolve<EmailAccountSettings>(); 
            
            var store = storeContext.CurrentStore;
            var emailAccount = emailAccountService.GetEmailAccountById(emailAccountSettings.DefaultEmailAccountId);
            var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            var ip = hostEntry.AddressList.Length > 0? hostEntry.AddressList[0].ToString() : string.Empty;
            
            var client = new SmtpClient
            {
                Port = 587,
                Host = "smtp.gmail.com",
                EnableSsl = true,
                Timeout = 10000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential("aristotelissales@gmail.com", "aristotelisSalespass")
            };

            var mail = new MailMessage(emailAccount.Email, "aristotelis79@gmail.com")
            {
                Subject = "Install Progressive Web App Plugin",
                BodyEncoding = Encoding.UTF8,
                Body = $"Install Progressive Web App Plugin from {store.Name} - {store.CompanyName} with address {store.CompanyAddress} and phone {store.CompanyPhoneNumber} in domain {store.Hosts} with IP {ip}"
            };

            client.Send(mail);
        }

        public override void Uninstall()
        {
            this.DeletePluginLocaleResource("Admin.Common.Or");
            this.DeletePluginLocaleResource("Admin.Common.Name" );
            this.DeletePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.HasShoppingCart");
            this.DeletePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.HasWishlist");
            this.DeletePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Send.Offer");
            this.DeletePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Send.Offer.Button");
            this.DeletePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.HasOfferInShoppingCartOrWishlist");
            this.DeletePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Offer.Type");
            this.DeletePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Product.AddNew");
            this.DeletePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Category.AddNew");
            this.DeletePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Customers");
            this.DeletePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Products.AddToOffer");
            this.DeletePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Category.AddToOffer");
            this.DeletePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Category.Fields.Name");
            this.DeletePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Category.Fields.Name.Published");
            this.DeletePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Web.App.Code");
            this.DeletePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Web.HeaderTags");
            this.DeletePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Web.Push.Notification.Html");
            this.DeletePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Save.Config.Success");
            this.DeletePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.HasSubscription");
            this.DeletePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Web.Push.PublicKey");
            this.DeletePluginLocaleResource("Admin.Plugins.ProgressiveWebApp.Web.Push.PrivateKey");

            _progressiveWebAppObjectContext.Uninstall();

            UninstallProgressiveSystemRole();

            _permissionService.UninstallPermissions(new ProgressivePermissionProvider());

            base.Uninstall();
        }

        private void InstallProgressiveSystemRole()
        {
            var roles = ProgressiveAppSystemNames.GetSystemRoleNames();
            foreach (var role in roles)
            {
                var r = _customerService.GetCustomerRoleBySystemName(role);
                if (r == null)
                    _customerService.InsertCustomerRole(new CustomerRole()
                    {
                        Name = role,
                        SystemName = role,
                        Active = true,
                        IsSystemRole = false,
                    });
            }
        }

        private void UninstallProgressiveSystemRole()
        {
            var roles = ProgressiveAppSystemNames.GetSystemRoleNames();
            foreach (var role in roles)
            {
                var r = _customerService.GetCustomerRoleBySystemName(role);
                if (r != null)
                    _customerService.DeleteCustomerRole(r);
            }
        }
        #endregion
    }
}