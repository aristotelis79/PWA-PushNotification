
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Progressive.Web.App.Models;
using Nop.Plugin.Progressive.Web.App.Security;
using Nop.Plugin.Progressive.Web.App.Settings;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Extensions;
using Nop.Web.Areas.Admin.Helpers;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc.Filters;
using ICustomerServiceExtend = Nop.Plugin.Progressive.Web.App.Services.ICustomerServiceExtend;

namespace Nop.Plugin.Progressive.Web.App.Controllers
{
    [Area(AreaNames.Admin)]
    public class AdminWebPushController : BasePluginController
    {
        #region fields
        private readonly ProgressiveWebAppSettings _progressiveWebAppSettings;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ICategoryService _categoryService;
        private readonly ICacheManager _cacheManager;
        private readonly IManufacturerService _manufacturerService;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerServiceExtend _customerServiceExtend;
        private readonly CustomerSettings _customerSettings;
        private readonly IDateTimeHelper _dateTimeHelper;
        #endregion

        #region ctor
        public AdminWebPushController(ProgressiveWebAppSettings progressiveWebAppSettings, ISettingService settingService, ILocalizationService localizationService, IPermissionService permissionService, IWorkContext workContext, VendorSettings vendorSettings, ICategoryService categoryService, ICacheManager cacheManager, IStoreService storeService, IShippingService shippingService, IVendorService vendorService, IManufacturerService manufacturerService, IProductService productService, IPictureService pictureService, ICustomerServiceExtend customerServiceExtend, ICustomerService customerService, CustomerSettings customerSettings, IDateTimeHelper dateTimeHelper)
        {
            _progressiveWebAppSettings = progressiveWebAppSettings;
            _settingService = settingService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _categoryService = categoryService;
            _cacheManager = cacheManager;
            _storeService = storeService;
            _vendorService = vendorService;
            _manufacturerService = manufacturerService;
            _productService = productService;
            _customerService = customerService;
            _customerServiceExtend = customerServiceExtend;
            _customerSettings = customerSettings;
            _dateTimeHelper = dateTimeHelper;
        }
        #endregion

        #region configuration
        [AuthorizeAdmin]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel
            {
                ProgressiveWebAppCode = _progressiveWebAppSettings.ProgressiveWebAppCode,
                ProgressiveWebAppHeaderTags = _progressiveWebAppSettings.ProgressiveWebAppHeaderTags,
                PushNotificationHtml = _progressiveWebAppSettings.PushNotificationHtml,
                PublicKey = _progressiveWebAppSettings.PublicKey,
                PrivateKey = _progressiveWebAppSettings.PrivateKey
            };
            return View("~/Plugins/Progressive.Web.App/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        public ActionResult Configure(ConfigurationModel model)
        {
            _progressiveWebAppSettings.ProgressiveWebAppCode = model.ProgressiveWebAppCode;
            _progressiveWebAppSettings.ProgressiveWebAppHeaderTags = model.ProgressiveWebAppHeaderTags;
            _progressiveWebAppSettings.PushNotificationHtml = model.PushNotificationHtml;
            _progressiveWebAppSettings.PrivateKey = model.PrivateKey;
            _progressiveWebAppSettings.PublicKey = model.PublicKey;
            _settingService.SaveSetting(_progressiveWebAppSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.ProgressiveWebApp.Save.Config.Success"));
            return View("~/Plugins/Progressive.Web.App/Views/Configure.cshtml", model);
        }
        #endregion
        
        #region OfferType
        public IActionResult ProductAddPopupList()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts)
            && !_permissionService.Authorize(ProgressivePermissionProvider.ProgressivePermissionRecord))
                return AccessDeniedView();

            var model = new AddOfferTypeModel();
            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var categories = SelectListHelper.GetCategoryList(_categoryService, _cacheManager, true);
            foreach (var c in categories)
                model.AvailableCategories.Add(c);

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var manufacturers = SelectListHelper.GetManufacturerList(_manufacturerService, _cacheManager, true);
            foreach (var m in manufacturers)
                model.AvailableManufacturers.Add(m);

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var vendors = SelectListHelper.GetVendorList(_vendorService, _cacheManager, true);
            foreach (var v in vendors)
                model.AvailableVendors.Add(v);

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            return View("~/Plugins/Progressive.Web.App/Views/ProductAddPopup.cshtml",model);
        }

        [HttpPost]
        public virtual IActionResult ProductAddPopupList(DataSourceRequest command, AddOfferTypeModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts)
                && !_permissionService.Authorize(ProgressivePermissionProvider.ProgressivePermissionRecord))
                return AccessDeniedKendoGridJson();

            var gridModel = new DataSourceResult();
            var products = _productService.SearchProducts(
                categoryIds: new List<int> { model.SearchCategoryId },
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true
                );
            gridModel.Data = products.Select(x => x.ToModel());
            gridModel.Total = products.TotalCount;

            return Json(gridModel);
        }

        public virtual IActionResult ProductAddPopup(int selectedOfferId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts)
                && !_permissionService.Authorize(ProgressivePermissionProvider.ProgressivePermissionRecord))
                return AccessDeniedView();

            var model = new AddOfferTypeModel();
            if (selectedOfferId > 0)
            {
                var product = _productService.GetProductById(selectedOfferId);
                if (product != null)
                {
                    model.SelectedOfferId = product.Id;
                    model.OfferType = OfferType.Product;
                    model.OfferName = product.Name;
                }
            }

            model.SelectOfferType = true;

            return View("~/Plugins/Progressive.Web.App/Views/ProductAddPopup.cshtml",model);
        }

        public IActionResult CategoryAddPopupList()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories)
                && !_permissionService.Authorize(ProgressivePermissionProvider.ProgressivePermissionRecord))
                return AccessDeniedView();

            var model = new AddOfferTypeModel();
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            return View("~/Plugins/Progressive.Web.App/Views/CategoryAddPopup.cshtml",model);
        }

        [HttpPost]
        public virtual IActionResult CategoryAddPopupList(DataSourceRequest command, AddOfferTypeModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories)
                && !_permissionService.Authorize(ProgressivePermissionProvider.ProgressivePermissionRecord))
                return AccessDeniedKendoGridJson();

            var categories = _categoryService.GetAllCategories(model.SearchCategoryName,
                model.SearchStoreId, command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = categories.Select(x =>
                {
                    var categoryModel = x.ToModel();
                    categoryModel.Breadcrumb = x.GetFormattedBreadCrumb(_categoryService);
                    return categoryModel;
                }),
                Total = categories.TotalCount
            };
            return Json(gridModel);
        }

        public virtual IActionResult CategoryAddPopup(int selectedOfferId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories)
                && !_permissionService.Authorize(ProgressivePermissionProvider.ProgressivePermissionRecord))
                return AccessDeniedView();
            
            var model = new AddOfferTypeModel();
            if (selectedOfferId > 0)
            {
                var category = _categoryService.GetCategoryById(selectedOfferId);
                if (category != null)
                {
                    model.SelectedOfferId = category.Id;
                    model.OfferType = OfferType.Category;
                    model.OfferName = category.Name;
                }
            }

            model.SelectOfferType = true;
            return View("~/Plugins/Progressive.Web.App/Views/CategoryAddPopup.cshtml",model);
        }
        #endregion

        #region Customers
        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers)
                && !_permissionService.Authorize(ProgressivePermissionProvider.ProgressivePermissionRecord))
                return AccessDeniedView();

            //load registered customers by default
            var defaultRoleIds = new List<int> { _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered).Id };
            var model = new OfferTypeModel
            {
                UsernamesEnabled = _customerSettings.UsernamesEnabled,
                DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled,
                SearchCustomerRoleIds = defaultRoleIds,
            };
            var allRoles = _customerService.GetAllCustomerRoles(true);
            foreach (var role in allRoles)
            {
                model.AvailableCustomerRoles.Add(new SelectListItem
                {
                    Text = role.Name,
                    Value = role.Id.ToString(),
                    Selected = defaultRoleIds.Any(x => x == role.Id)
                });
            }

            return View("~/Plugins/Progressive.Web.App/Views/SentOffer.cshtml", model);
        }

        [HttpPost]
        public virtual IActionResult CustomerList(DataSourceRequest command, OfferTypeModel model, int[] searchCustomerRoleIds)
        {
            //we use own own binder for searchCustomerRoleIds property 
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers)
                && !_permissionService.Authorize(ProgressivePermissionProvider.ProgressivePermissionRecord))
                return AccessDeniedKendoGridJson();

            var searchDayOfBirth = 0;
            var searchMonthOfBirth = 0;
            if (!string.IsNullOrWhiteSpace(model.SearchDayOfBirth))
                searchDayOfBirth = Convert.ToInt32(model.SearchDayOfBirth);
            if (!string.IsNullOrWhiteSpace(model.SearchMonthOfBirth))
                searchMonthOfBirth = Convert.ToInt32(model.SearchMonthOfBirth);

            var customers = _customerServiceExtend.GetAllCustomersExtend(
                customerRoleIds: searchCustomerRoleIds,
                email: model.SearchEmail,
                username: model.SearchUsername,
                dayOfBirth: searchDayOfBirth,
                monthOfBirth: searchMonthOfBirth,
                ipAddress: model.SearchIpAddress,
                loadOnlyWithShoppingCart: model.HasShoppingCart || model.HasWishList,
                sct:    (model.HasShoppingCart && model.HasWishList) 
                    ||  (!model.HasShoppingCart && !model.HasWishList) 
                                                                    ? (ShoppingCartType?) null 
                                                                    : model.HasShoppingCart 
                                                                            ? ShoppingCartType.ShoppingCart 
                                                                            : ShoppingCartType.Wishlist,
                hasOfferInShoppingCartOrWishlist: model.HasOfferInShoppingCartOrWishlist,
                offerType: model.OfferType,
                offerId:model.OfferId,
                hasSubscription: model.HasSubscription,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = customers.Select(PrepareCustomerModelForList),
                Total = customers.TotalCount
            };

            return Json(gridModel);
        }

        protected virtual CustomerModel PrepareCustomerModelForList(Customer customer)
        {
            return new CustomerModel
            {
                Id = customer.Id,
                Email = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest"),
                Username = customer.Username,
                FullName = customer.GetFullName(),
                CustomerRoleNames = GetCustomerRolesNames(customer.CustomerRoles.ToList()),
                Active = customer.Active,
                CreatedOn = _dateTimeHelper.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc),
                LastActivityDate = _dateTimeHelper.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc),
            };
        }

        protected virtual string GetCustomerRolesNames(IList<CustomerRole> customerRoles, string separator = ",")
        {
            var sb = new StringBuilder();
            for (var i = 0; i < customerRoles.Count; i++)
            {
                sb.Append(customerRoles[i].Name);
                if (i != customerRoles.Count - 1)
                {
                    sb.Append(separator);
                    sb.Append(" ");
                }
            }
            return sb.ToString();
        }
        #endregion
    }
}