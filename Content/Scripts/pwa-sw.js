importScripts('.\\node_modules\\sw-toolbox\\sw-toolbox.js');
importScripts('.\\node_modules\\\localforage\\dist\\localforage.min.js');
importScripts('.\\pwa-site.js');

var cacheVersion = {
    static: 'nop-site-static-v2', //TODO setting?
    dynamic: 'nop-site-dynamic-v2'  //TODO setting? 
}

var cacheOptions = {
    staticOptions: {
        cache: {
            name: cacheVersion.static,
            maxAgeSecond: 60 * 60 * 24 //1 day //TODO setting
        }
    },
    dynamicOptions: {
        networkTimeoutSecond: 5, //after 5 seconds without response  //TODO setting
        cache: {
            name: cacheVersion.dynamic,
            maxEntries: 50 //TODO setting
        }
    }
    //staticPattern: new RegExp("^("+self.location.origin.replace("/","\/")+").*\.(js|css|gif|jpg|jpeg|tiff|png|svg|woff2)$","i")
};

self.addEventListener('install', function (event) {

        event.waitUntil(
            caches.open(cacheVersion.static).then(function (cache) {
                return cache.addAll([
                    '.\\..\\..\\Views\\Offline.html',
                    '.\\..\\..\\Content\\Images\\NoConnection.jpg',
                    '.\\..\\..\\Content\\Icons\\safari-pinned-tab-blue.svg',
                    //'.\\..\\..\\Content\\Scripts\\node_modules\\sw-toolbox\\sw-toolbox.js',
                    //'.\\..\\..\\Content\\Scripts\\node_modules\\\localforage\\dist\\localforage.min.js'
                    //'/css/imgs/sprites-v6.png',
                    //'/css/fonts/whatever-v8.woff',
                    //'/js/all-min-v4.js'
                    // etc
                ]);
            })
        );
        //self.skipWaiting();
    });

self.addEventListener('activate', function (event) {
    event.waitUntil(
        caches.keys().then(function (cacheNames) {
            return Promise.all(cacheNames.filter(function (cacheName) {
                    // Return true if you want to remove this cache,
                    // but remember that caches are shared across
                    // the whole origin
                    return !Object.values(cacheVersion).includes(cacheName);
                }).map(function (cacheName) {
                    return caches.delete(cacheName);
                })
            );
        })
    );
});

self.addEventListener('notificationclick', function (event) {
    event.notification.close();
    var payload = event.notification.data;

    switch (event.action) {
    case 'addToWishlist':
        fetch(`/addproducttocart/catalog/${payload.offer.Id}/2/1`,
                {
                    headers: { 'Content-Type': 'application/json' },
                    method: 'POST',
                    credentials: 'same-origin',
                    cache: 'no-cache'
                })
            .then(function (response) {
                if (response.ok) {
                    return response.json();
                }
            })
            .then(function (data) {
            })
            .catch(function (err) {
                console.log(err);
            });
        break;
    case 'viewOffer':
        self.clients.openWindow(`${event.target.location.origin}/${payload.offer.SeName}`);
        break;
    case 'goToCart':
        self.clients.openWindow('/cart');
        break;
    case 'later':
        event.notification.close();
        break;
    }
});

self.addEventListener('push', function(event) {
    
    var title = "";
    var options = {};
    var payload = event.data.json();

    if(typeof payload.notificationType == "undefined") 
        return false;
    
    switch (payload.notificationType) {
    case 'Offer':

        var body = payload.offer.Name;
        if (payload.offer.Price) {
            body = payload.offer.Name + ' for ' + payload.offer.Price;
        }

        title = 'New Super Offer';
        options = {
            body: body,
            icon: '.\\..\\..\\Content\\Icons\\android-chrome-192x192.png',
            badge: '.\\..\\..\\Content\\Icons\\android-chrome-192x192.png',
            image: payload.offer.ImageUrl,
            data: payload,
            actions: [
                { action: 'viewOffer', title: 'See the Offer', icon: '' },
                { action: 'addToWishlist', title: 'Add to wishlist', icon: '' }
            ]
        };
        break;
    case 'Cart':
        title = 'You are online, continue your shopping';
        options = {
            body: 'Your cart was updated, You may proceed to purchase',
            icon: '.\\..\\..\\Content\\Icons\\android-chrome-192x192.png',
            badge: '.\\..\\..\\Content\\Icons\\android-chrome-192x192.png',
            //image: ,
            //data: payload,
            actions: [
                { action: 'goToCart', title: 'Go to Cart', icon: '' },
                { action: 'later', title: 'Dismiss for later', icon: '' }
            ]
        };
        break;
    }

    event.waitUntil(self.registration.showNotification(title, options));
});

self.addEventListener('sync', function (event) {
    if (event.tag === 'sync-add-to-cart') {
        
        event.waitUntil( syncProducts() );
    }
});              
                
function syncProducts() {
    AddToCartProductsDb.ready().then(() => {
        AddToCartProductsDb.getAllkeys()
            .then((allkeys) => {

                return Promise.all(allkeys.map(function(key) {

                    return AddToCartProductsDb.get(key);
                }));
            })
            .then((addToCartProducts) => {

                return Promise.all(addToCartProducts.map(function(product) {

                    return processAddToCart(product);
                }));
            })
            .then((synchronized) => {

                var hasASyncProduct = false;

                synchronized.forEach((sync) => {

                    if (sync.success) {
                        hasASyncProduct = true;
                    }
                });

                if (hasASyncProduct) {

                    fetch('/WebPush/AddToCartNotification',
                            {
                                method: "GET",
                                credentials: 'include',
                                cache: 'no-cache'
                            })
                        .then(function(response) {
                            return response.json();
                        })
                        .then(function(data) {
                        });
                }
            })
            .catch(error => {
                console.log(error);
            });
    })
    .catch(error => {
        console.log(error);
    });            
}

async function processAddToCart(product){
    var response;
    
    if (product.isFrom === 'ProductPage') {

        var body = new FormData();
        for (prAttr in product) {
            body.append(prAttr, product[prAttr]);
        }

        response = await fetch(`/addproducttocart/details/${product.productId}/1`,
        {
            method: "POST",
            body: body,
            credentials: 'include',
            cache: 'no-cache'
        });

    } else {
       
        response = await fetch(`/addproducttocart/catalog/${product.productId}/1/${product[`addtocart_${product.productId}.EnteredQuantity`]}`,
        {
            headers: { 'Content-Type': 'application/json' },
            method: 'POST',
            credentials: 'include',
            cache: 'no-cache'
        });
    }

    if (response.ok){

        var cloneResponse = response.clone();

        var data = await cloneResponse.json();

        if (data && (data.success || data.redirect)) {

           AddToCartProductsDb.remove(product.productId);

           return {
               productId: product.productId,
               success: data.success,
               redirect: data.redirect
            }
        }
    }
}


//self.addEventListener('fetch', function (e) {
//    if (!navigator.onLine) {
//        e.respondWith(new Response(' <h1>Offline</h1>', { headers : { 'Content-Type': 'text/html'} } ));
//    } else {
//        console.log(e.request.url);
//        e.respondWith(fetch(e.request));   
//    }
//}),


//Tsw-Toolbox for Caching
//toolbox.options.debug = true,

//Admin Area
toolbox.router.get("/admin/*", toolbox.networkOnly),

//Contents
toolbox.router.get('/Content/*', toolbox.cacheFirst, cacheOptions.staticOptions),
toolbox.router.get('/content/*', toolbox.cacheFirst, cacheOptions.staticOptions),
toolbox.router.get('/Scripts/*', toolbox.cacheFirst, cacheOptions.staticOptions),
toolbox.router.get('/Plugins/Progressive.Web.App/Content/*', toolbox.cacheFirst, cacheOptions.staticOptions),
toolbox.router.get('/Themes/DefaultClean/Content/*', toolbox.cacheFirst, cacheOptions.staticOptions),

//Manual call to network first
toolbox.router.get('/*', function (request, values, options) {
    return toolbox.networkFirst(request, values, options)
        .catch(function (error) {
            return caches.match(new Request('.\\..\\..\\Views\\Offline.html')); //TODO Custom offline page
        });
}, cacheOptions.dynamicOptions);
