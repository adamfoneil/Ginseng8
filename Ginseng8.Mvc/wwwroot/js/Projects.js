var viewProjectDetailLinks = document.querySelectorAll('.viewProjectBody');
viewProjectDetailLinks.forEach(function (e) {
    e.addEventListener('click', function (e) {
        var projectId = e.target.getAttribute('data-project-id');
        var hasBody = (e.target.getAttribute('data-project-has-body') == 'true');
        var showDiv = (hasBody) ?
            document.getElementById('project-view-' + projectId) :
            document.getElementById('project-edit-' + projectId);

        $(showDiv).slideToggle();
    });
});

$(document).ready(function () {
    InitProjectSortable();

    InitDashboardWorkItemDraggable();
    InitDashboardProjectCardLinkButtonsDroppable();
});

function InitProjectSortable() {
    $('.app-container').each(function(index, appContainer) {
        var appContainerProjectsSortable = $(appContainer).find('.project-sortable');

        appContainerProjectsSortable.sortable({
            placeholder: 'ui-state-highlight',
            connectWith: appContainerProjectsSortable,
            cancel: 'button',
            start: sortableStartUpdateHeightAndWidth,
            stop: sortableStop,

            update: function (event, ui) {
                // This event is triggered when the user stopped sorting and the DOM position has changed.

                if (ui.sender == null) {
                    // issue here is we're not getting all the project cards on the page via .parents.... It's only
                    // including what's in the priority tier that was dropped on
                    updateAppProjectsPriorities($(ui.item).parents('.project-sortable'));
                } else {
                    // when an item from a connected sortable list has been dropped into another list
                    updateAppProjectsPriorities($(ui.sender));
                }
            }
        });
    });
}

function updateAppProjectsPriorities(list) {
    if (list.length === 0) {
        return;
    }

    var projectArray = [];

    var appContainer = list.parents('.app-container');
    var appProjectsCards = appContainer.find('.project-card');

    list.find('.project-card').each(function (index, element) {
        var priority = appProjectsCards.index(element);

        $(element).find('sup').html(priority + 1);

        projectArray.push({
            number: element.getAttribute('data-project-id'),
            index: priority
        });
    });

    ProjectReorder({
        items: projectArray
    });
}

function ProjectReorder(data) {
    console.log('ProjectReorder data object', data, '\n\n');

    fetch('/Update/ProjectPriorities', {
        method: 'post',
        headers: {
            "Content-Type": "application/json",
            "RequestVerificationToken": getAntiForgeryToken()
        },
        body: JSON.stringify(data)
    }).then(function (response) {
        // success fail info?
        return response.json();
    });
}

$('.project-work-items').tooltip({
    items: 'a',
    content: function() {
        var projectId = $(this).data('project-id');
        return '<div class="fa fa-spinner fa-spin" data-project-id="' + projectId + '"></div>';
    },
    show: null,
    open: function(event, ui) {
        if (typeof(event.originalEvent) === 'undefined') {
            return false;
        }

        // close any lingering tooltips
        var $id = $(ui.tooltip).attr('id');
        $('div.ui-tooltip').not('#' + $id).remove();
        
        var projectId = $(ui.tooltip).find('[data-project-id]').data('project-id');
        var url = '/WorkItem/ListInProject/' + projectId;

        var year = $(this).data('year');
        var month = $(this).data('month');
        if (year !== undefined & month !== undefined) url += '?year=' + year + '&month=' + month;

        fetch(url, {
            method: 'get'
        }).then(function (response) {
            return response.text();
        }).then(function (content) {
            $(ui.tooltip).find('.ui-tooltip-content').html(content);
        });
    },
    close: function(event, ui) {
        ui.tooltip.hover(function() {
            $(this).stop(true).fadeTo(400, 1);
        }, function() {
            $(this).fadeOut('400', function() {
                $(this).remove();
            });
        });
    }
});

function InitDashboardWorkItemDraggable() {
    initDraggableItems({
        selector: '.js-dashboard-work-item-draggable',
        options: {
            helper: function() {
                return $("<div class='card card-header bg-light z-index-fixed flex-row align-items-center p-2' data-number=" + $(this).data('number') + "></div>")
                        .append($(this).find('.work-item-number').clone().addClass('mr-1'))
                        .append($(this).find('.js-title').clone());
            }
        }
    })
}

function InitDashboardProjectCardLinkButtonsDroppable() {
    InitDroppable({
        selector: '.js-droppable-project-card-link-button-container'
    }, onProjectCardLinkButtonsDrop);
}

function onProjectCardLinkButtonsDrop(draggableElement, droppableElement) {
    var data = {
        projectId: droppableElement.data('project-id'),
        id: draggableElement.data('number'),
    };

    console.group('update');
    console.log(data);
    console.groupEnd();

    fetch('/WorkItem/SetProject', {
        method: 'post',
        body: getFormData(data)
    }).then(function (response) {
        return response.text();
    }).then(function (html) {
        $('#projectInfo-' + data.projectId).html(html);
    });
}
