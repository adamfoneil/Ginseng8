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

function sortableStartUpdateHeightAndWidth(event, ui) {
    // This event is triggered when sorting starts.
    $('.ui-sortable-placeholder').outerWidth($(ui.item).outerWidth()); // update width of placeholder to current item height
    sortableStart(event, ui);
}

function sortableStop() {
    setTimeout(function () {
        $('body').removeClass('dragging');
    });
}

function initDraggableItems(params) {
    var options = params && params.options ? params.options : {};
    var selector = params && params.selector ? ', ' + params.selector : '';

    $('.js-item-draggable' + selector)
    .draggable(Object.assign({
        revert: 'invalid',
        start: sortableStart,
        stop: sortableStop
    }, options));
}

function InitDroppable(params, dropCallback) {
    var options = params && params.options ? params.options : {};

    $(params.selector).droppable(Object.assign({
        classes: {
            'ui-droppable-active': 'ui-state-active',
            'ui-droppable-hover': 'ui-state-highlight'
        },
        drop: function(event, ui) {
            var draggableElement = $(ui.draggable).clone();
            draggableElement.css({
                position: 'static',
                top: 'auto',
                left: 'auto'
            });
            $(ui.draggable).remove();
            $('.ui-state-active').removeClass('ui-state-active');
            $('.ui-draggable-dragging').removeClass('ui-draggable-dragging');

            dropCallback && dropCallback(draggableElement, $(this));

            initDraggableItems();
        }
    }, options));
}
