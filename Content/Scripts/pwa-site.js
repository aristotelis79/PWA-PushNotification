var OfflineMode = (function () {
    
    var isProductPage;
    var hasProductBox;
    
    function init() {

        isProductPage = document.querySelectorAll(".page.product-details-page").length > 0;
        hasProductBox = document.querySelectorAll(".item-box .product-item").length > 0;
        
        addOfflineBar();
        
        if (isProductPage || hasProductBox) {
            changeAddToCartProductEvents();
        }
    };
    
    function addOfflineBar() {

        var parent = document.querySelector('.master-wrapper-page');
        var offlineBar = "<div class='offline-mode'>Offline Mode</div><div class='offline-mode-offset'></div>";
        parent.insertAdjacentHTML('afterbegin', offlineBar);
    };
    
    function changeAddToCartProductEvents() {

        document.querySelectorAll("input[id^='add-to-cart-button-'], input.product-box-add-to-cart-button").forEach((p) => {
                p.setAttribute("onClick", "OfflineMode.storeProductToDb();");
            });
    };
    
    function storeProductToDb() {
    
        var inputAttributes = {};
        var productId;
    
        if (isProductPage) {
    
            //checkboxes-radiobuttons
            var attributesWwithCheck = document.querySelectorAll("input[name^='product_attribute_']");
            
            attributesWwithCheck.forEach((attrCheck) => {
                if (attrCheck.checked === true) {
                    inputAttributes[attrCheck.name] = attrCheck.value;
                }
            });
    
            //dropdowns - textboxes
            var attributesDropAndText = document.querySelectorAll("select[name^='product_attribute_'], input[name^='product_attribute_'][type='text'] ,input[name^='addtocart_']");
                
            attributesDropAndText.forEach((attrDropAndText) => {
                    inputAttributes[attrDropAndText.name] = attrDropAndText.value;
            });
    
            productId = document.querySelector("input[id^='add-to-cart-button-']").dataset.productid;
            inputAttributes['isFrom'] = 'ProductPage';

        } else {

            productId = event.currentTarget.closest(".product-item").dataset.productid;
            var qnty = 1;// document.querySelector("").value;
            inputAttributes[`addtocart_${productId}.EnteredQuantity`] = qnty;
            inputAttributes['isFrom'] = 'ProductBox';                
        }

        inputAttributes['productId'] = productId;
        AddToCartProductsDb.add(productId, inputAttributes);
    }
    
    return {
        init: init,
        storeProductToDb: storeProductToDb
    }
})();
    
    
var ServiceWorkerSite = (function () {
        
    var isMobileDevice = navigator.userAgent.search(new RegExp('[Aa]ndroid|i[Pp]ad|i[Pp]hone')) > 0;
    var addToHomeEvent;
    var userhasChoiced;
    
    //Service Worker RegistrationS
    if ('serviceWorker' in navigator) {

        navigator.serviceWorker.register('/Plugins/Progressive.Web.App/Content/Scripts/pwa-sw.js', { scope: "/" })
            .then(function (r) {
                beforeInstallPrompt();
            })
            .catch(console.error());
    }
    
    //Offline mode
    if (!navigator.onLine) {
        OfflineMode.init();
    }
    
    function beforeInstallPrompt() {
        window.addEventListener('beforeinstallprompt', function (event) {
                        
            event.preventDefault(); //stop add to home popup
        
            addToHomeEvent = event; //store add to home event
            
            userhasChoiced = getCookie('wantMyProgressiveApp'); 

            if (isMobileDevice && !userhasChoiced) {
                promptAddToHome();
            }
            return false;
        });
    }
        
    function promptAddToHome(){
        swal({
            title: 'NopCommerce Home Screen Shortcut',
            text: "If you like an App experience confirm add in the upcoming alert",
            type: 'info',
            showCancelButton: true,
            confirmButtonColor: '#286893',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, I will add it'
        }).then(function() {
            if (addToHomeEvent != undefined) {
                setCookie('wantMyProgressiveApp', 'accepted', 30);
                addToHome();
            } else {
                setCookie('wantMyProgressiveApp', 'notaccepted', 30);
            }
        }).catch(function(result) {
            setCookie('wantMyProgressiveApp', 'notaccepted', 30);
            swal.noop;
        });
    }
        
    function addToHome() {    
            addToHomeEvent.prompt();
                    
            addToHomeEvent.userChoice.then(function (choice) {
                userhasChoiced = choice.outcome;
                if (choice.outcome === 'accepted') {
                    //appInstalled();
                }
                addToHomeEvent = null; //We no longer need the prompt.  Clear it up. 
            });
    };
        
    function appInstalled() {
                
        window.addEventListener('appinstalled', function (event) {
            console.log("app installed");
        });
    }

    function registerAddToCartSync(qnty) {

        if('serviceWorker' in navigator && 'SyncManager' in window){
        
            navigator.serviceWorker.ready.then(sw => {
            
                return sw.sync.register('sync-add-to-cart')
                        .then(AjaxCart.success_process(addToCartSuccessMessage(qnty)));
        });
    }
}

    function addToCartSuccessMessage(qnty)  {
            return{
                success : true,
                message : "The product will be add to your shopping cart when you online",
                updatetopcartsectionhtml : `${qnty}`
        }
    }

    function setCookie(name, value, days) {
        var d = new Date;
        d.setTime(d.getTime() + 24*60*60*1000*days);
        document.cookie = name + "=" + value + ";path=/;expires=" + d.toGMTString();
    }

    function getCookie(name) {
        var v = document.cookie.match('(^|;) ?' + name + '=([^;]*)(;|$)');
        return v ? v[2] : null;
    }

    function deleteCookie(name) { setCookie(name, '', -1); }

    return{
        registerAddToCartSync : registerAddToCartSync
    }
})();


var AddToCartProductsDb = (function() {
        
    localforage.config({
        name: 'nop-offline-add-to-cart'
    });
        
    function ready() {
        return localforage.ready();
    }

    function get(key) {
        return localforage.getItem(key);
    }
        
    function getAllkeys(){
        return localforage.keys();
    }
    
    function add(key, value) {

        localforage.setItem(key, value)
            .then(function (value) {
                ServiceWorkerSite.registerAddToCartSync(value[`addtocart_${key}.EnteredQuantity`]);
            })
            .catch(function (err) {
                console.log(err);
            });  
    }
        
    function remove(key) {

        localforage.removeItem(key)
            .catch(function (err) {
                console.log(err);
            }); 
    }
        
    return {
        get: get,
        add: add,
        remove:remove,
        ready:ready,
        getAllkeys: getAllkeys
    }
})();