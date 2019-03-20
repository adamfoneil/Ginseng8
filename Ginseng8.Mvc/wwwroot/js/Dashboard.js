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

var htmlEditLinks = document.querySelectorAll('.editHtml');
htmlEditLinks.forEach(function (e) {
    e.addEventListener("click", function (e) {
        var id = e.target.getAttribute("data-id");
        var idPrefix = e.target.getAttribute('data-id-prefix');
        $("#" + idPrefix + '-view-' + id).hide();
        $("#" + idPrefix + '-edit-' + id).show();
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

var addCommentSubmitButtons = document.querySelectorAll('.add-comment-submit');
addCommentSubmitButtons.forEach(function (ele) {
    ele.addEventListener('click', function (ev) {
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
            $('#comments-' + number + '-output').html(html);
        });
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
    
});

function InitWorkItemSortable() {
    $('.sortable').sortable({
        placeholder: "ui-state-highlight",
        cancel: ':input, button, [contenteditable="true"]',
        start: function (event, ui) {
            // This event is triggered when sorting starts.
            $('.ui-state-highlight').outerHeight($(ui.item).outerHeight()); // update height of placeholder to current item height
        },

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

    var milestoneId = list.parents('[data-milestone-id]').data('milestone-id'); // what about dropping on another milestone?
    var userId = list.data('user-id'); // assumed to be the user id containing the dropped item, but what about dropping on another user?
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