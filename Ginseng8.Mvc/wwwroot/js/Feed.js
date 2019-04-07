var eventCheckboxes = document.querySelectorAll('.update-event-subscription');
eventCheckboxes.forEach(function (ele) {
    ele.addEventListener('change', function (ev) {
        var loading = $(ev.target).find('img');
        $(loading).show();

        var data = {
            appId: ev.target.getAttribute('data-appId'),
            eventId: ev.target.getAttribute('data-id'),
            selected: $(ev.target).is(':checked')
        };

        var formData = getFormData(data);
        fetch('/Update/EventSubscription', {
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