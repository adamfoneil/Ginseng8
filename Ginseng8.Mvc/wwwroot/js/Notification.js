var notifyToggles = document.querySelectorAll('.toggleNotification');
notifyToggles.forEach(function (ele) {
    ele.addEventListener('click', function (ev) {
        var anchor = $(ev.target).parent('a')[0];
        var id = anchor.getAttribute('data-id');

        var data = {
            id: id,
            propertyName: anchor.getAttribute('data-property-name'),
            tableName: anchor.getAttribute('data-table-name')
        };

        var formData = getFormData(data);
        fetch('/Update/ToggleNotification', {
            method: 'post',
            body: formData
        }).then(function (response) {
            return response.text();
        }).then(function (result) {
            $('#notifyOptions-' + id).html(result);
        });
    });
});