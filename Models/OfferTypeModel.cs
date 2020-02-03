using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Progressive.Web.App.Models
{
    public class OfferTypeModel
    {
        public OfferTypeModel()
        {
            SearchCustomerRoleIds = new List<int>();
            AvailableCustomerRoles = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Customers.Customers.List.CustomerRoles")]
        public IList<int> SearchCustomerRoleIds { get; set; }
        public IList<SelectListItem> AvailableCustomerRoles { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchEmail")]
        public string SearchEmail { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchUsername")]
        public string SearchUsername { get; set; }
        public bool UsernamesEnabled { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchDateOfBirth")]
        public string SearchDayOfBirth { get; set; }
        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchDateOfBirth")]
        public string SearchMonthOfBirth { get; set; }
        public bool DateOfBirthEnabled { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchIpAddress")]
        public string SearchIpAddress { get; set; }

        [NopResourceDisplayName("Admin.Plugins.ProgressiveWebApp.HasShoppingCart")]
        public bool HasShoppingCart { get; set; } = false;

        [NopResourceDisplayName("Admin.Plugins.ProgressiveWebApp.HasWishList")]
        public bool HasWishList { get; set; }  = false;

        [NopResourceDisplayName("Admin.Plugins.ProgressiveWebApp.HasOfferInShoppingCartOrWishlist")]
        public bool HasOfferInShoppingCartOrWishlist { get; set; }  = false;

        [NopResourceDisplayName("Admin.Plugins.ProgressiveWebApp.OfferType")]
        public OfferType OfferType { get; set; }

        [NopResourceDisplayName("Admin.Plugins.ProgressiveWebApp.OfferId")]
        public int OfferId { get; set; }

        [NopResourceDisplayName("Admin.Plugins.ProgressiveWebApp.HasSubscription")]
        public bool HasSubscription { get; set; } = true;
    }
}