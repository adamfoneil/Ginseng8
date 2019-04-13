function getFormData(object) {
    var formData = new FormData();
    for (var key in object) formData.append(key, object[key]);
    formData.append("__RequestVerificationToken", getAntiForgeryToken());
    return formData;
}

function getAntiForgeryToken() {
    var div = document.getElementById('antiForgeryToken');
    var input = div.getElementsByTagName('input')[0];
    return input.value;
}

function sortableStart(event, ui) {
    // This event is triggered when sorting starts.
    $('.ui-sortable-placeholder').outerHeight($(ui.item).outerHeight()); // update height of placeholder to current item height
    $('body').addClass('dragging');
}

function sortableStop() {
    setTimeout(function () {
        $('body').removeClass('dragging');
    });
}

