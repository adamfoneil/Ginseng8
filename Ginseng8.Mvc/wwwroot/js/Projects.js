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
        cancel: ':input, button, [contenteditable="true"]',
        start: sortableStart,
        stop: sortableStop,

        update: function (event, ui) {
            // This event is triggered when the user stopped sorting and the DOM position has changed.

            if (ui.sender == null) {
                updateProjectPriorities($(ui.item).parents('.project-sortable'));
            } else {
                // when an item from a connected sortable list has been dropped into another list
                updateProjectPriorities($(ui.sender));
            }
        }
    });
}

function updateProjectPriorities(list, taskObject) {
    var task = taskObject || list.prevObject;

    if (list.length === 0 || task == null) {
        return;
    }
   
    var projectArray = [];

    list.find('.project-card').each(function (projectIndex, projectElement) {
        projectArray.push({
            number: projectElement.getAttribute('data-project-id'),
            index: projectIndex
        });
    });

    ProjectReorder({
        items: projectArray
    });
}

function ProjectReorder(data) {
    console.log('ProjectReorder data json', JSON.stringify(data));
    console.log('ProjectReorder data object', data);

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
