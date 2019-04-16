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
});

function InitProjectSortable() {
    $('.project-sortable').sortable({
        placeholder: "ui-state-highlight",                
        connectWith: '.project-sortable',
        cancel: 'button',
        start: sortableStartUpdateHeightAndWidth,
        stop: sortableStop,

        update: function (event, ui) {
            // This event is triggered when the user stopped sorting and the DOM position has changed.

            if (ui.sender == null) {
                // issue here is we're not getting all the project cards on the page via .parents.... It's only
                // including what's in the priority tier that was dropped on
                updateProjectPriorities($(ui.item).parents('.project-sortable'));
            } else {
                // when an item from a connected sortable list has been dropped into another list
                updateProjectPriorities($(ui.sender));
            }
        }
    });
}

function updateProjectPriorities(list) {
    if (list.length === 0) {
        return;
    }

    var projectArray = [];

    list.find('.project-card').each(function (index, element) {
        var priority = $('.project-card').index(element);

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
