getNotification();

let cn = new signalR.HubConnection("/notify");

cn.on('displayNofification', () => {
    getNotification();
});

cn.start();