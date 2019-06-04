$('.milestone-work-items').tooltip({
    items: 'a',
    content: function () {
        var milestoneId = $(this).data('milestone-id');
        return '<div class="fa fa-spinner fa-spin" data-milestone-id="' + milestoneId + '"></div>';
    },
    show: null,
    open: function (event, ui) {
        if (typeof (event.originalEvent) === 'undefined') {
            return false;
        }

        // close any lingering tooltips
        var $id = $(ui.tooltip).attr('id');
        $('div.ui-tooltip').not('#' + $id).remove();

        var milestoneId = $(ui.tooltip).find('[data-milestone-id]').data('milestone-id');

        fetch('/WorkItem/ListInMilestone/' + milestoneId, {
            method: 'get'
        }).then(function (response) {
            return response.text();
        }).then(function (content) {
            $(ui.tooltip).find('.ui-tooltip-content').html(content);
        });
    },
    close: function (event, ui) {
        ui.tooltip.hover(function () {
            $(this).stop(true).fadeTo(400, 1);
        }, function () {
            $(this).fadeOut('400', function () {
                $(this).remove();
            });
        });
    }
});
