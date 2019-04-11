var notifyToggles = document.querySelectorAll('.toggleNotification');
notifyToggles.forEach(function (ele) {
    ele.addEventListener('click', function (ev) {
        var data = {
            id: ev.target.getAttribute('data-id'),
            propertyName: ev.target.getAttribute('data-property-name'),
            tableName: ev.target.getAttribute('data-table-name')
        };

        var formData = getFormData(data);
        fetch('/Update/Notification', {
            method: 'post',
            body: formData
        }).then(function (response) {
            return response.json();
        }).then(function (result) {
            $(loading).hide();
            $('#btnRefresh').show();
        });

    });
});