$(document).on('click', '.toggleNotification', function () {         
    var id = $(this).attr('data-id');

    var data = {
        id: id,
        propertyName: $(this).attr('data-property-name'),
        tableName: $(this).attr('data-table-name')
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
