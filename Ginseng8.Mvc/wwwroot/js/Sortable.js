$(".item-sortable").sortable({
    placeholder: "ui-state-highlight",
    start: function (event, ui) {
        // This event is triggered when sorting starts.
        $('.ui-state-highlight').outerHeight($(ui.item).outerHeight()); // update height of placeholder to current item height
    }
});