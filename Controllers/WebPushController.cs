using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Messages;
using Nop.Plugin.Progressive.Web.App.Helpers;
using Nop.Plugin.Progressive.Web.App.Models;
using Nop.Plugin.Progressive.Web.App.Services;
using Nop.Plugin.Progressive.Web.App.Settings;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Web.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Models.Catalog;
using WebPush;

namespace Nop.Plugin.Progressive.Web.App.Controllers
{
    public class WebPushController : BasePluginController

    {
        #region fields
        private readonly IProgressiveWebPushService _progressiveWebPushService;
        private readonly IWorkContext _workContext;
        private readonly IEmailAccountService _emailAccountService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly ILogger _logger;
        private readonly ICategoryService _categoryService;
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly ProgressiveWebAppSettings _progressiveWebAppSettings;

        #endregion

        #region ctor
        public WebPushController(IProgressiveWebPushService progressiveWebPushService, 
        IWorkContext workContext, 
        IEmailAccountService emailAccountService, 
        EmailAccountSettings emailAccountSettings,
        IProductService productService,
        IProductModelFactory productModelFactory, 
        ILogger logger, 
        ICategoryService categoryService,
        ICatalogModelFactory catalogModelFactory, ProgressiveWebAppSettings progressiveWebAppSettings)
        {
            _progressiveWebPushService = progressiveWebPushService;
            _workContext = workContext;
            _emailAccountService = emailAccountService;
            _emailAccountSettings = emailAccountSettings;
            _productService = productService;
            _productModelFactory = productModelFactory;
            _logger = logger;
            _categoryService = categoryService;
            _catalogModelFactory = catalogModelFactory;
            _progressiveWebAppSettings = progressiveWebAppSettings;
        }
        #endregion

        #region subscription 
        
        [HttpPost]
        public ActionResult CreateOrUpdateSubscription([FromBody] SubscriptionModel subscriptionModel)
        {
            try
            {
                var subscriptionRecord = _progressiveWebPushService.GetSubscriptionByCustomerId(_workContext.CurrentCustomer.Id);
                if (subscriptionRecord == null)
                    _progressiveWebPushService.CreateSubscription(subscriptionModel.ToSubscriptionRecord(_workContext.CurrentCustomer.Id));
                else
                    _progressiveWebPushService.UpdateSuscription(subscriptionRecord);
            }
            catch (Exception e)
            {
                return Json(new { Success = false, e.Message });
            }
            return Json(new { Success = true });
        }

        [HttpPost]
        public IActionResult RemoveSubscription()
        {
            try
            {
                _progressiveWebPushService.RemoveSubscriptionByCustomerId(_workContext.CurrentCustomer.Id);
            }
            catch (Exception e)
            {
                return Json(new { Success = false, e.Message });
            }
            return Json(new { Success = true });
        }
        
        #endregion subscription

        #region notifications
        
        public IActionResult AddToCartNotification()
        {
            var payload = JsonConvert.SerializeObject(new { notificationType = NotificationType.Cart.ToString() });
            
            var customerIds = new int[]{_workContext.CurrentCustomer.Id};
            
            return Json(SentNotification(customerIds,payload)); 
        }

        [HttpPost]
        public IActionResult SendNotification(int[] customerIds, string payload)
        {
            return Json(SentNotification(customerIds,payload));
        }

        [HttpPost]
        public IActionResult SendNotificationOffer(SentNotificationModel model)
        {
            if (model.SelectedIds == null)                
                return Json(new ResultMessageModel{ Success = false, Message = "No Customers select" });

            
            var customerIds = model.SelectedIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x))
                .ToArray();

            string payload;
            
            switch (model.OfferType)
            {
                case OfferType.Product:
                    var products = new List<Product>{_productService.GetProductById(model.OfferId)};
 
                    var productModel = _productModelFactory.PrepareProductOverviewModels(products).FirstOrDefault();
                    if (productModel == null)
                        return Json(new ResultMessageModel{ Success = false, Message = "No product found" });
                    
                    payload = JsonConvert.SerializeObject(new {
                                                                offer = new {
                                                                    productModel.Id,
                                                                    productModel.Name,
                                                                    productModel.SeName,
                                                                    productModel.ProductPrice.Price,
                                                                    productModel.DefaultPictureModel.ImageUrl
                                                                },
                                                                notificationType = NotificationType.Offer.ToString()});     
                    break;
                
                case OfferType.Category:
                    var category = _categoryService.GetCategoryById(model.OfferId);
                    var categoryModel = _catalogModelFactory.PrepareCategoryModel(category ,new CatalogPagingFilteringModel());
                    if (categoryModel == null)
                        return Json(new ResultMessageModel{ Success = false, Message = "No category found" });
              
                    payload = JsonConvert.SerializeObject(new{
                                                                offer = new{
                                                                    categoryModel.Id,
                                                                    categoryModel.Name,
                                                                    categoryModel.SeName,
                                                                    categoryModel.PictureModel.ImageUrl
                                                                },
                                                                notificationType = NotificationType.Offer.ToString()
                                                            });        
                    break;
                default:
                    return Json(new ResultMessageModel{ Success = false, Message = "No offer type selected" });
            }

            return Json(SentNotification(customerIds,payload)); 
        }

        private ResultMessageModel SentNotification(int[] customerIds, string payload)
        {
            var defaultEmail = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
            
            if(!customerIds.Any())
                return new ResultMessageModel{ Success = false, Message = "No customerIds" };

            var vadipDetails = new VapidDetails($@"mailto:{defaultEmail.Email}",
                _progressiveWebAppSettings.PublicKey,//"BPm7brrdyZSRGdrdToRv6SQ4jc4zItgIpO4lcRxWFxLoItq_5voFmSqAdMvuF2yb6vvPUDVwN1-6z3UpXkHapKM",
                _progressiveWebAppSettings.PrivateKey); //"EKXB3GIqxhHWBfEg1sRl1HJ3E8U9EXWP5TTuSfeKDqk"


            var subscriptions = _progressiveWebPushService.GetSubscriptionByCustomerIds(customerIds);
            if(subscriptions == null || !subscriptions.Any())
                return new ResultMessageModel{ Success = false, Message = "No Subcriptions" };

            var sendNotificationNumber = 0; 
            
            foreach (var subscription in subscriptions)
            {
                try
                {
                    var webPushClient = new WebPushClient();
                    webPushClient.SendNotification(new PushSubscription(subscription.Endpoint, subscription.P256DHKey, subscription.AuthKey), payload, vadipDetails);
                    sendNotificationNumber++;
                }
                catch (WebPushException e)
                {
                    _logger.Warning(e.Message,e.InnerException);
                }
            }
            return new ResultMessageModel{ Success = true, Message = $"Send {sendNotificationNumber} from {subscriptions.Count} Notifications" };
        }
               
        #endregion

        public IActionResult GetOffer(int customerId)
        {
            var products = new List<Product>{_productService.GetProductById(18)};
            var offer = _productModelFactory.PrepareProductOverviewModels(products).FirstOrDefault();

            if (offer == null) return Json("No Offer");

            var payload = JsonConvert.SerializeObject(new {
                offer = new {
                    offer.Id,
                    offer.Name,
                    offer.SeName,
                    offer.ProductPrice.Price,
                    offer.DefaultPictureModel.ImageUrl
                }, notificationType = NotificationType.Offer.ToString() });

            var customerIds = new[]{customerId};
            return Json(SentNotification(customerIds,payload)); 
        }
    }
}