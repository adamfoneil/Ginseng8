var reloadProjectDropdowns = document.querySelectorAll('.fillProjects');
reloadProjectDropdowns.forEach(function (ele) {
    ele.addEventListener('change', function (ev) {        
        var number = ev.target.form.Number.value;
        fetch('/WorkItem/GetAppProjects?appId=' + $(ev.target).val(), {
            method: 'get'            
        }).then(function (response) {
            return response.json();
        }).then(function (data) {
            var select = $('#ProjectId-' + number);
            select.children().remove();
            $("<option>").val("").text("- project -").appendTo(select);
            $(data).each(function () {
                $("<option>").val(this.value).text(this.text).appendTo(select);
            });
        });
    });
});

// jQuery UI tooltips
$('.tooltips').tooltip({
    items: 'span',
    content: function () {
        var number = $(this).data('number');
        return $('#work-item-title-' + number).html();
    }
});

var updateFields = document.querySelectorAll('.itemUpdate');
updateFields.forEach(function (e) {
    e.addEventListener("change", function (e) {
        var frm = e.target.form;
        var number = frm['Number'].value;
        var loadingImg = document.getElementById('loading-' + number);
        $(loadingImg).show();

        var failImg = document.getElementById('update-failed-' + number);

        try {
            let formData = new FormData(frm);
            fetch('/Update/WorkItem', {
                method: 'post',
                body: new URLSearchParams(formData)
            }).then(function (response) {
                return response.json();
            }).then(function (result) {                
                if (result.success) {
                    var successImg = document.getElementById('update-success-' + number);
                    $(successImg).show();
                    $(successImg).fadeOut();
                } else {
                    $(failImg).show();
                    failImg.setAttribute('title', result.message);
                }
            });
        } catch (e) {            
            $(failImg).show();
            failImg.setAttribute('title', e.message);
        } finally {
            $(loadingImg).hide();
        }
    });
});

var projectUpdateFields = document.querySelectorAll('.projectUpdate');
projectUpdateFields.forEach(function (ele) {
    ele.addEventListener("change", function (ev) {
        var frm = ev.target.form;
        var projectId = frm.Id.value;
        var loadingImg = document.getElementById('loading-project-' + projectId);
        $(loadingImg).show();
        var failImg = document.getElementById('update-failed-project-' + projectId);
        try {
            let formData = new FormData(frm);
            fetch('/Update/Project', {
                method: 'post',
                body: new URLSearchParams(formData)
            }).then(function (response) {
                return response.json();
            }).then(function (result) {
                if (result.success) {
                    var successImg = document.getElementById('update-success-project-' + projectId);
                    $(successImg).show();
                    $(successImg).fadeOut();
                } else {
                    $(failImg).show();
                    failImg.setAttribute('title', result.message);
                }
            });
        } catch (e) {
            $(failImg).show();
            failImg.setAttribute('title', e.message);
        } finally {
            $(loadingImg).hide();
        }
    });
});

var htmlEditLinks = document.querySelectorAll('.editHtml');
htmlEditLinks.forEach(function (e) {
    e.addEventListener("click", function (e) {
        var id = e.target.getAttribute("data-id");
        var idPrefix = e.target.getAttribute('data-id-prefix');
        $("#" + idPrefix + '-view-' + id).hide();
        $("#" + idPrefix + '-edit-' + id).show();
        $("#" + idPrefix + '-content-' + id).focus(); // doesn't work because this textarea is hidden by the editor
        $(e.target).hide();
    });
});

var htmlCancelEditLinks = document.querySelectorAll('.cancelHtmlEdit');
htmlCancelEditLinks.forEach(function (e) {
    e.addEventListener('click', function (e) {
        var id = e.target.getAttribute('data-id');
        var idPrefix = e.target.getAttribute('data-id-prefix');
        $('#' + idPrefix + '-view-' + id).show();
        $('#' + idPrefix + '-edit-' + id).hide();
        $('#editHtmlLink-' + idPrefix + '-' + id).show();
    });
});

var htmlSaveButtons = document.querySelectorAll('.saveHtmlEdit');
htmlSaveButtons.forEach(function (e) {
    e.addEventListener('click', function (e) {
        var frm = e.target.form;
        var id = frm['Id'].value;
        var failImg = document.getElementById('update-failed-' + id);
        var idPrefix = e.target.getAttribute('data-id-prefix');
        var postUrl = e.target.getAttribute('data-post-url');

        try {
            let formData = new FormData(frm);
            fetch(postUrl, {
                method: 'post',
                body: new URLSearchParams(formData)
            }).then(function (response) {
                return response.json();
            }).then(function (result) {
                if (result.success) {
                    var updatedContent = $('#' + idPrefix + '-content-' + id).val();
                    $('#' + idPrefix + '-view-' + id).html(updatedContent);
                    $('#' + idPrefix + '-view-' + id).show();
                    $('#' + idPrefix + '-edit-' + id).hide();
                    $('#editHtmlLink-' + idPrefix + '-' + id).show();
                } else {
                    $(failImg).show();
                    failImg.setAttribute('title', result.message);
                }
            });
        } catch (e) {
            $(failImg).show();
            failImg.setAttribute('title', e.message);
        }
    });
});

var labelCheckboxes = document.querySelectorAll('.labelCheckbox');
labelCheckboxes.forEach(function (e) {    
    e.addEventListener('click', function (e) {
        e.stopPropagation();      
        var frm = e.target.form;
        var number = frm['WorkItemNumber'].value;
        let formData = new FormData(frm);
        fetch('/Update/WorkItemLabel', {
            method: 'post',
            body: new URLSearchParams(formData)
        }).then(function (response) {
            return response.text();
        }).then(function (html) {
            $('#labels-' + number).html(html);
        });
    });
});

function LabelCheckboxLinkOnClick(ev) {
    ev.stopPropagation();
    var checkbox = null;
    if (ev.target.tagName == 'A') {
        checkbox = ev.target.getElementsByTagName('input')[0];
    } else if (ev.target.tagName == 'SPAN') {
        checkbox = ev.target.parentElement.getElementsByTagName('input')[0];
    }
    checkbox.checked = !checkbox.checked;
    var event = new Event('click');
    checkbox.dispatchEvent(event);
}

var labelCheckboxes = document.querySelectorAll('.labelCheckboxLink');
labelCheckboxes.forEach(function (e) {
    e.addEventListener('click', LabelCheckboxLinkOnClick);
});

var itemDetailButtons = document.querySelectorAll('.btn-item-detail');
itemDetailButtons.forEach(function (ele) {
    ele.addEventListener('click', function (ev) {
        var divId = ev.target.getAttribute('data-target');
        var div = document.getElementById(divId);
        $(div).slideToggle();
    });
});

var selfStartLinks = document.querySelectorAll('.self-start-activity');
selfStartLinks.forEach(function (ele) {
    ele.addEventListener('click', function (ev) {
        var element = ev.target;
        if (element.tagName == 'I') element = element.parentElement;

        var data = {
            id: element.getAttribute('data-number'),
            activityId: element.getAttribute('data-activity-id')
        };

        var formData = getFormData(data);
        
        fetch('/WorkItem/SelfStartActivity', {
            method: 'post',
            body: formData
        }).then(function (response) {
            // show success/fail or something?
            return response.json();
        });
    });
});

var resumeWorkLinks = document.querySelectorAll('.resume-work-item');
resumeWorkLinks.forEach(function (ele) {
    ele.addEventListener('click', function (ev) {
        var data = {
            id: ev.target.getAttribute('data-number')
        };

        var formData = getFormData(data);

        fetch('/WorkItem/ResumeActivity', {
            method: 'post',
            body: formData
        }).then(function (response) {
            // show success/fail or something?
            return response.json();
        });
    });
});

var unassignWorkLinks = document.querySelectorAll('.unassign-work-item');
unassignWorkLinks.forEach(function (ele) {
    ele.addEventListener('click', function (ev) {
        var data = {
            id: ev.target.getAttribute('data-number')
        };
        var formData = getFormData(data);
        fetch('/WorkItem/UnassignMe', {
            method: 'post',
            body: formData
        }).then(function (response) {
            // show success/fail or something?
            return response.json();
        });
    });
});

var addCommentButtons = document.querySelectorAll('.addComment');
addCommentButtons.forEach(function (ele) {
    ele.addEventListener('click', function (ev) {
        var button = $(ev.target).closest('.addComment')[0];
        var target = button.getAttribute('data-target');
        var div = document.getElementById(target);
        $(div).slideToggle('fast', function () {
            if ($(div).is(':visible')) {
                var field = document.getElementById(target + '-HtmlBody');
                field.focus();
            }
        });
    });
});

$(document)
.on('click', '.add-comment-submit', function(ev) {
    ev.preventDefault();
    var frm = ev.target.form;
    let formData = new FormData(frm);

    fetch('/WorkItem/SaveComment', {
        method: 'post',
        body: formData
    }).then(function (response) {
        return response.text();
    }).then(function (html) {
        var number = frm.Number.value;
        $('#comments-' + number + '-output').first().html(html);
    });
});

var noPropagateItems = document.querySelectorAll('.no-propagation');
noPropagateItems.forEach(function (ele) {
    ele.addEventListener('click', function (ev) {
        ev.stopPropagation();
    });
});

$(document).ready(function () {
    $('.nav-tabs li:first-child a').tab('show');

    InitWorkItemSortable();
    initDraggableItems();
    InitProjectCrosstabWorkItemDroppable();
    InitTableBodySortable();
});

function sortableStart(event, ui) {
    // This event is triggered when sorting starts.
    $('.ui-sortable-placeholder').outerHeight($(ui.item).outerHeight()); // update height of placeholder to current item height
    $('body').addClass('dragging');
}

function sortableStop() {
    setTimeout(function() {
        $('body').removeClass('dragging');
    });
}

function InitTableBodySortable() {
    $('.js-table-body-sortable').sortable({
        items: '.js-table-row-sortable',
        placeholder: 'ui-state-highlight',
        cancel: ':input, button, a',
        start: sortableStart,
        stop: sortableStop,
        update: function(event, ui) {
            var sortableTable = $(ui.item).parents('.js-table-body-sortable')
            var sortableRows = sortableTable.find('.js-table-row-sortable');
            var rowsArray = [];

            sortableRows.each(function(index, row) {
                rowsArray.push({
                    id: Number($(row).attr('id').replace('row-', '')),
                    index: index
                });
            });

            tableBodySortableRowsReorder(rowsArray);
        },
    });
}

function tableBodySortableRowsReorder(rows) {
    console.log('rows data json', JSON.stringify(rows));

    fetch('/Update/PropertyOrder', {
        method: 'post',
        headers: {
            "Content-Type": "application/json",
            "RequestVerificationToken": getAntiForgeryToken()
        },
        body: JSON.stringify(rows)
    }).then(function (response) {
        // success fail info?
        return response.json();
    });
}

function InitWorkItemSortable() {
    $('.sortable').sortable({
        placeholder: "ui-state-highlight",
        connectWith: '.milestone-items .sortable, #assignedUserTab .nav-link:not(.active)',
        cancel: ':input, button, [contenteditable="true"]',
        start: sortableStart,
        stop: sortableStop,

        update: function (event, ui) {
            // This event is triggered when the user stopped sorting and the DOM position has changed.

            if (ui.sender == null) {
                updateSortableList($(ui.item).parents('.sortable'));
            } else {
                // when an item from a connected sortable list has been dropped into another list
                updateSortableList($(ui.sender));
            }
        }
    });
}

function updateSortableList(list, taskObject) {
    var task = taskObject || list.prevObject;

    if (list.length === 0 || task == null) {
        return;
    }

    if (list.parents('#assignedUserTab').length) {
        console.group('work item assign to another user');
        console.log('user id:', list.data('user-group-id'));
        console.log('task number', task.data('number'));
        console.groupEnd();

        var data = {
            userId: list.data('user-group-id'),
            number: task.data('number')
        };

        fetch('/WorkItem/SetUser', {
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

        return;
    }

    var milestoneId = list.parents('[data-milestone-id]').data('milestone-id'); 
    var userId = list.data('user-id');
    var number = task.data('number');
    var tasksArray = [];

    list.find('.work-item-card').each(function (taskIndex, workItemElement) {
        tasksArray.push({
            number: workItemElement.getAttribute('data-number'),
            index: taskIndex
        });
    });

    TaskReorder({        
        milestoneId: milestoneId,        
        userId: userId,
        number: number,
        items: tasksArray,
    })
}

function TaskReorder(data) {
    console.log('TaskReorder data json', JSON.stringify(data));
    console.log('TaskReorder data object', data);

    fetch('/WorkItem/SetPriorities', {
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

function initDraggableItems() {
    $('.js-item-draggable').draggable({
        revert: 'invalid',
        start: sortableStart,
        stop: sortableStop
    });
}

function InitProjectCrosstabWorkItemDroppable() {
    $('.milestone-work-items-droppable').droppable({
        accept: function(draggable) {
            var draggableMilestone = $(draggable).parents('.milestone-work-items-droppable')[0];
            if ($(draggableMilestone).data('project-id') == $(this).data('project-id')) {
                return draggableMilestone != this
            }
        },
        classes: {
            'ui-droppable-active': 'ui-state-active',
            "ui-droppable-hover": "ui-state-highlight"
        },
        drop: function( event, ui ) {
            var element = $(ui.draggable).clone();
            element.css({
                position: 'static',
                top: 'auto',
                left: 'auto'
            });
            $(ui.draggable).remove();
            $(this).append(element);
            $('.ui-state-active').removeClass('ui-state-active');
            initDraggableItems();

            projectCrosstabWorkItemUpdate({
                number: element.find('.work-item-number').data('number'),
                milestoneDate: $(this).data('milestone-date')
            });
        }
    });
}

function projectCrosstabWorkItemUpdate(data) {
    console.log('work item update:', JSON.stringify(data));
}

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

