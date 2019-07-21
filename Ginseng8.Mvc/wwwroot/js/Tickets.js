$('.fd-ticket-tooltip').tooltip({
    items: 'a',
    content: function () {
        var ticketId = $(this).data('ticket-id');
        return '<div class="fa fa-spinner fa-spin" data-ticket-id="' + ticketId + '"></div>';
    },
    show: null,
    open: function (event, ui) {
        if (typeof (event.originalEvent) === 'undefined') {
            return false;
        }

        // close any lingering tooltips
        var $id = $(ui.tooltip).attr('id');
        $('div.ui-tooltip').not('#' + $id).remove();

        var ticketId = $(ui.tooltip).find('[data-ticket-id]').data('ticket-id');

        fetch('/Freshdesk/TicketBody/' + ticketId, {
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