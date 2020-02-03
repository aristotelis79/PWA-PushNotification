using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Progressive.Web.App.Settings;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Progressive.Web.App.Components
{
    [ViewComponent(Name = "ProgressiveWebAppHeaderTags")]
    public class ProgressiveWebAppHeaderTagsViewComponent : NopViewComponent
    {
        private readonly ProgressiveWebAppSettings _progressiveWebAppSettings;

        public ProgressiveWebAppHeaderTagsViewComponent(ProgressiveWebAppSettings progressiveWebAppSettings)
        {
            _progressiveWebAppSettings = progressiveWebAppSettings;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            //_httpContext.Request.Headers.Add("Service-Worker-Allowed", "/"); //add header to web.config
            return View("~/Plugins/Progressive.Web.App/Views/PublicInfo.cshtml", _progressiveWebAppSettings.ProgressiveWebAppHeaderTags);
        }
    }
}