using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Progressive.Web.App.Settings;
using Nop.Web.Framework.Components;


namespace Nop.Plugin.Progressive.Web.App.Components
{
    [ViewComponent(Name = "ProgressiveWebAppCode")]
    public class ProgressiveWebAppCodeViewComponent : NopViewComponent
    {
        private readonly ProgressiveWebAppSettings _progressiveWebAppSettings;

        public ProgressiveWebAppCodeViewComponent(ProgressiveWebAppSettings progressiveWebAppSettings)
        {
            _progressiveWebAppSettings = progressiveWebAppSettings;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            return View("~/Plugins/Progressive.Web.App/Views/PublicInfo.cshtml", _progressiveWebAppSettings.ProgressiveWebAppCode);
        }
    }
}