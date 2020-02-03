var serviceWorkerNotification = (function () {

    var pubkey = document.getElementById('push-notification-publickey').value;
    var notifyBtn = document.getElementById('notifybtn');
    var btnIcon = document.getElementById('notifyicon');

    var serviceWorkerObject;

    if ('serviceWorker' in navigator && 'PushManager' in window) {

        if (Notification.permission !== 'denied') {
            notifyBtn.disabled = false;
        }

        navigator.serviceWorker.ready.then(function (sw) {
            serviceWorkerObject = sw;

            sw.pushManager.getSubscription()
                .then(function (s) {
                    var isSubscribed = (s !== null);
                    btnIcon.innerHTML = isSubscribed ? '&#xf0f3' : '&#xf1f6';
                });
        });
    }

    notifyBtn.addEventListener('click', function (event) {
            
        serviceWorkerObject.pushManager.getSubscription().then(function (s) {
                
            if (s !== null && s !== undefined) {
                    
                s.unsubscribe().then(function (s) {
                            
                    fetch('WebPush/RemoveSubscription',
                    {
                        headers: { 'Content-Type': 'application/json' },
                        method: 'POST',
                        credentials: 'same-origin',
                        body: JSON.stringify(s)
                    })
                    .then(function(response) {
                            return response.json();
                    })
                    .then(function (data) {

                        if (data.Success) {
                            notificationMessage('success', 'Your Subscription Removed', false, 'OK');
                            btnIcon.innerHTML = '&#xf1f6';
                        } else {
                            notificationMessage('error', 'Something going wrong', true, 'Try Again?');
                            btnIcon.innerHTML = '&#xf0f3';
                        }
                    });
                }).catch(function (err) {
                    console.log(err);
                });

            } else {

                serviceWorkerObject.pushManager.subscribe({
                    userVisibleOnly: true,
                    applicationServerKey: urlB64ToUint8Array(pubkey)

                }).then(function (s) {
                        
                    fetch('WebPush/CreateOrUpdateSubscription',
                    {
                        headers: { 'Content-Type': 'application/json' },
                        method: 'POST',
                        credentials: 'same-origin',
                        body: JSON.stringify(s)
                    })
                    .then(function(response) {
                        return response.json();
                    })
                    .then(function (data) {
                        if (data.Success) {
                            notificationMessage('success', 'Your Subscription Activate', false, 'OK');
                            btnIcon.innerHTML = '&#xf0f3';
                        } else {
                            notificationMessage('error', 'Something going wrong', true, 'Try Again?');
                            btnIcon.innerHTML = '&#xf1f6';
                        }
                    });
                }).catch(function (err) {
                    console.log(err);
                });
            }
        });
    });


    function notificationMessage(type, message,showCancelButton, comfirmButtonText) {
        swal({
            title: 'NopCommerce Notifications',
            text: message,
            type: type,
            showCancelButton: showCancelButton,
            showConfirmButton: true,
            confirmButtonColor: '#286893',
            cancelButtonColor: '#d33',
            confirmButtonText: comfirmButtonText
        })
        .then(function(result) {
            if(type === 'error' && result.value) {
                notifyBtn.click();
            }
        }).catch(swal.noop);
    }


    function urlB64ToUint8Array(base64String) {
        const padding = '='.repeat((4 - base64String.length % 4) % 4);
        const base64 = (base64String + padding)
            .replace(/\-/g, '+')
            .replace(/_/g, '/');

        const rawData = window.atob(base64);
        const outputArray = new Uint8Array(rawData.length);

        for (let i = 0; i < rawData.length; ++i) {
            outputArray[i] = rawData.charCodeAt(i);
        }
        return outputArray;
    }
})();
