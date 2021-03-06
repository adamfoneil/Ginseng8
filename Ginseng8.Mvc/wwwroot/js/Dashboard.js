﻿var pins = document.querySelectorAll('.toggle-pin');
pins.forEach(function (ele) {
    ele.addEventListener('click', function (ev) {
        var data = {
            id: $(ev.target).data('number')
        };
        fetch('/WorkItem/TogglePin', {
            method: 'post',
            body: getFormData(data)
        }).then(function (response) {
            return response.json();
        }).then(function (data) {
            $(ev.target).switchClass(data.removeClass, data.addClass);
        });
    });
});

var reloadAppDropdowns = document.querySelectorAll('.fillApps');
reloadAppDropdowns.forEach(function (ele) {
    ele.addEventListener('change', function (ev) {
        FillDropdown(ev, '/WorkItem/GetApps?teamId=', '#AppId-', '- application -');
    });
});

var showHideApps = document.querySelectorAll('.showHideApps');
showHideApps.forEach(function (ele) {
    ele.addEventListener('change', function (ev) {
        var teamId = $(ev.target).val();
        var useApps = teamUseApps[teamId];
        var number = ev.target.form.Number.value;        
        if (useApps) {
            $('#AppId-' + number).show();
        } else {
            $('#AppId-' + number).hide();
        }
    });
});

var reloadProjectDropdowns = document.querySelectorAll('.fillProjects');
reloadProjectDropdowns.forEach(function (ele) {
    ele.addEventListener('change', function (ev) {
        var number = ev.target.form.Number.value;
        var teamId = $('#TeamId-' + number).val();
        var appId = $('#AppId-' + number).val();
        var useApps = teamUseApps[teamId];
        if (useApps & (appId != '')) {            
            FillDropdown(ev, '/WorkItem/GetAppProjects?appId=', '#ProjectId-', '- project -');
        } else {
            if (appId == '') {
                FillDropdown(ev, '/WorkItem/GetTeamProjects?appId=0&teamId=', '#ProjectId-', '- project -', teamId);
            } else {
                FillDropdown(ev, '/WorkItem/GetTeamProjects?teamId=', '#ProjectId-', '- project -', teamId);
            }            
        }        
    });
});

var reloadMilestoneDropdowns = document.querySelectorAll('.fillMilestones');
reloadMilestoneDropdowns.forEach(function (ele) {
    ele.addEventListener('change', function (ev) {
        FillDropdown(ev, '/WorkItem/GetAppMilestones?appId=', '#MilestoneId-', '- milestone -');
    });
});

function FillDropdown(ev, url, selectIdPrefix, blankOption, paramValue) {
    var number = ev.target.form.Number.value;
    var id = (paramValue === undefined) ? $(ev.target).val() : paramValue;
    fetch(url + id, {
        method: 'get'
    }).then(function (response) {
        return response.json();
    }).then(function (data) {
        var select = $(selectIdPrefix + number);
        FillDropdownItems(data, select, blankOption);
    });
}

function FillDropdownItems(data, select, blankOption) {
    select.children().remove();
    $("<option>").val("").text(blankOption).appendTo(select);
    $(data).each(function () {
        $("<option>").val(this.value).text(this.text).appendTo(select);
    });
}

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
                    var numberBadge = $('#work-item-number-' + number);
                    numberBadge.css('background-color', result.backgroundColor);
                    numberBadge.removeClass('badge-secondary');
                    if (result.className) numberBadge.addClass(result.className);
                    var missingEstimateModifier = $('#' + result.missingEstimateModifierId);
                    if (result.showMissingEstimateModifier) {
                        missingEstimateModifier.show();
                    } else {
                        missingEstimateModifier.hide();
                    }
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



$(document)
    .on('click', '.editHtml', function (event) {
        event.preventDefault();

        var $target = $(event.target);
        var idPrefix = $target.attr('data-id-prefix');
        var id = $target.attr('data-id');

        $target.hide();
        $('#' + idPrefix + '-edit-' + id).show();
        $('#' + idPrefix + '-view-' + id).hide();
        $('#' + idPrefix + '-content-' + id).froalaEditor('events.focus');
    })
    .on('click', '.cancelHtmlEdit', function (event) {
        event.preventDefault();
    })
    // edit comment button click handler
    .on('click', '.js-edit-comment', editComment)


function editComment() {
    var commentsList = $(this).parents('.js-comments-list');
    var commentsListItem = $(this).parents('.js-comment-list-item');
    var commentHtml = commentsListItem.find('.js-comment-html').html();
    var formWrapper = commentsList.siblings('.js-comment-form-wrapper');
    var submitButton = $(formWrapper.find('.add-comment-submit'));
    var editorElement = $(formWrapper.find('.htmlEditor'));
    var commentId = $(this).attr('data-comment-id')
    var hiddenInputCommentId = $(formWrapper.find('input[name="Id"]'))

    // Hide commentsList
    commentsList.hide()

    // Change 'add comment form' layout to 'edit comment form's
	editorElement.froalaEditor('html.set', commentHtml) // set comment's html to form textarea
	submitButton.html('Save') // rename submit button 'Add' -> 'Save'
    submitButton.before(
    	'<button class="btn btn-link btn-sm js-cancel-editing mr-1">Cancel</button>'
    ) // append Cancel button
    hiddenInputCommentId.val(commentId) // set comment id to hidden field

    var buttonCancel = $(formWrapper.find('.js-cancel-editing'));

    // Bind event listeners
    buttonCancel.on('click', cancelCommentEditing.bind(this, {
    	editorElement: editorElement,
    	formWrapper: formWrapper,
    	commentsList: commentsList,
    	submitButton: submitButton,
    	buttonCancel: buttonCancel,
        hiddenInputCommentId: hiddenInputCommentId
    }))


	// Display form
	formWrapper.show()

	// Set focus on textarea field
	editorElement.froalaEditor('events.focus');
}

function cancelCommentEditing(params) {
	// Change 'edit comment form' layout to 'add comment form
	params.editorElement.froalaEditor('html.set', ''); // Reset form textarea
	params.submitButton.html('Add'); // rename submit button 'Save' to 'Add' 
	params.buttonCancel.remove(); // remove Cancel button
    params.hiddenInputCommentId.val('') // reset comment id

	// Hide form
	params.formWrapper.hide()

	// Display commentsList
	params.commentsList.show()
}

$('.htmlEditor').on('froalaEditor.image.beforeUpload', function(event, editor, images) {
    console.group('before upload image, update params');
    editor.opts.imageUploadParams = {
        'folderName': $(event.target).data('folder-name'),
        'id': Number($(event.target).data('id')),
    }
    console.log(editor.opts.imageUploadParams);
    console.groupEnd();
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

var labelCheckboxes = document.querySelectorAll('.labelCheckboxLink');
labelCheckboxes.forEach(function (e) {
    e.addEventListener('click', MultiSelectCheckboxLinkOnClick);
});

var itemDetailButtons = document.querySelectorAll('.btn-item-detail');
itemDetailButtons.forEach(function (ele) {
    ele.addEventListener('click', function (ev) {
        var divId = ev.target.getAttribute('data-target');
        var div = document.getElementById(divId);
        $(div).slideToggle('fast', function () {
            if ($(this).is(':visible')) {
                var editor = $(this).find('.htmlEditor');
                editor.froalaEditor('events.focus');
            }
        });        
    });
});

var selfStartLinks = document.querySelectorAll('.self-start-activity');
selfStartLinks.forEach(function (ele) {
    ele.addEventListener('click', function (ev) {
        var element = ev.target;
        if (element.tagName == 'I') element = element.parentElement;

        var loading = $(element).find('img');
        $(loading).show();

        var data = {
            id: element.getAttribute('data-number'),
            activityId: element.getAttribute('data-activity-id')
        };

        var formData = getFormData(data);
        
        fetch('/WorkItem/SelfStartActivity', {
            method: 'post',
            body: formData
        }).then(function (response) {            
            return response.json();
        }).then(function (result) {
            $(loading).hide();
            $(ev.target).hide();
            if (result.success) {
                console.log('next success', $(ev.target).next('.success'));
                $(ev.target).next('.success').show();
            } else {
                $(ev.target).next('.error').show();
            }
        });
    });
});

var closeLinks = document.querySelectorAll('.close-item');
closeLinks.forEach(function (ele) {
    ele.addEventListener('click', function (ev) {
        AssignActionEventHandler(ev, '/WorkItem/Close', function (e) {
            return {
                id: e.getAttribute('data-number'),
                reasonId: e.getAttribute('data-reason-id')
            };
        });   
        RemoveWorkItem(ev);
    });
});

function RemoveWorkItem(ev) {
    var card = $(ev.target).parents('.work-item-card');
    var number = $(card).data('number');
    $('#card-' + number).slideUp();
}

var resumeWorkLinks = document.querySelectorAll('.resume-work-item');
resumeWorkLinks.forEach(function (ele) {
    ele.addEventListener('click', function (ev) {
        AssignActionEventHandler(ev, '/WorkItem/ResumeActivity');
    });
});

var cancelWorkLinks = document.querySelectorAll('.cancel-activity');
cancelWorkLinks.forEach(function (ele) {
    ele.addEventListener('click', function (ev) {
        AssignActionEventHandler(ev, '/WorkItem/CancelActivity');
    });
});

var unassignWorkLinks = document.querySelectorAll('.unassign-work-item');
unassignWorkLinks.forEach(function (ele) {
    ele.addEventListener('click', function (ev) {
        AssignActionEventHandler(ev, '/WorkItem/UnassignMe');
        RemoveWorkItem(ev);
    });    
});

var workOnNextLinks = document.querySelectorAll('.work-on-next');
workOnNextLinks.forEach(function (ele) {
    ele.addEventListener('click', function (ev) {
        AssignActionEventHandler(ev, '/WorkItem/WorkOnNext');
    });
});

var removePriorityLinks = document.querySelectorAll('.remove-priority');
removePriorityLinks.forEach(function (ele) {
    ele.addEventListener('click', function (ev) {
        AssignActionEventHandler(ev, '/WorkItem/RemovePriority');
    });
});

var assignToLinks = document.querySelectorAll('.assign-to');
assignToLinks.forEach(function (ele) {
    ele.addEventListener('click', function (ev) {
        AssignActionEventHandler(ev, '/WorkItem/AssignTo', function (e) {
            return {
                id: e.getAttribute('data-number'),
                userId: e.getAttribute('data-user-id')
            };
        });
    });
});

function AssignActionEventHandler(ev, postUrl, dataFunction) {
    ev.preventDefault();
    var loading = $(ev.target).find('img');
    $(loading).show();

    // when assign again but to another person then hide previous success message
    $(ev.target).siblings('.assign-to + .success').hide();

    var data = (typeof dataFunction == 'function')
                ? dataFunction(ev.target)
                : {
                    id: ev.target.getAttribute('data-number')
                };

    var formData = getFormData(data);
    fetch(postUrl, {
        method: 'post',
        body: formData
    }).then(function (response) {
        return response.json();
    }).then(function (result) {
        $(loading).hide();
        $(ev.target).hide();
        if (result.success) {
            // show success message
            $(ev.target).next('.success').show();
        } else {
            $(ev.target).next('.error').show();
        }
    });
}

$(document)
.on('click', '.add-comment-submit', function(ev) {
    ev.preventDefault();
    var frm = ev.target.form;

    if (frm.HtmlBody.value == '') {
        alert('Comment may not be empty.');
        return;
    }

    let formData = new FormData(frm);

    $(ev.currentTarget).attr('disabled', true);

    fetch('/WorkItem/SaveComment', {
        method: 'post',
        body: formData
    }).then(function (response) {
        return response.text();
    }).then(function (html) {
        var objectId = frm.ObjectId.value;
        var objectType = frm.ObjectType.value;

        $('#comments-' + objectId + '-' + objectType + '-output').first().html(html);
        $('#addComment-' + objectId + '-HtmlBody').froalaEditor(GetFroalaSettings());
    });
})
.on('click', '.addComment', function(event) {
    event.preventDefault();

    var button = $(event.target).closest('.addComment')[0];
    var target = button.getAttribute('data-target');
    var div = document.getElementById(target);
    $(div).slideToggle('fast', function () {
        if ($(div).is(':visible')) {
            var objectId = button.getAttribute('data-id');
            $('#addComment-' + objectId + '-HtmlBody').froalaEditor('events.focus');            
        }
    });
})
.on('click', '.search-box-container', function(event) {
    event.stopPropagation();
})
.on('input', '.search-box', function(event) {
    var $input = $(event.currentTarget);
    var searchTerm = $input.val().toLowerCase();
    var $dropdownItems = $input.parents('.dropdown-menu').find('.dropdown-item');

    if (!searchTerm.length) {
        $dropdownItems.show();
        return;
    }

    $dropdownItems.hide();

    $dropdownItems.each(function(index, item) {
        var $item = $(item);

        if (~$item.find('.badge').text().toLowerCase().indexOf(searchTerm)) {
            $item.show();
        }
    });
});

var noPropagateItems = document.querySelectorAll('.no-propagation');
noPropagateItems.forEach(function (ele) {
    ele.addEventListener('click', function (ev) {
        ev.stopPropagation();
    });
});

$(document).ready(function () {
    var hash = window.location.hash;
    if (hash) {
        var workItemNum = hash.substring(1);
        var workItemAnchor = $('a[name=' + workItemNum + ']');
        if (workItemAnchor.length) {
            var tabId = workItemAnchor.parents('.tab-pane').data('tab-id');
            $('#' + tabId).tab('show');
        } else {
            fetch('/WorkItem/InfoBanner/' + workItemNum, {
                method: 'get'
            }).then(function (response) {
                return response.text();
            }).then(function (html) {
                $('#infoBanner').html(html);
            });
        }        
    } else {
        // no current work item, so show first tab by default
        $('.nav-tabs li:first-child a').tab('show');
    }
    
    InitWorkItemSortable();
    initDraggableItems();
    InitProjectCrosstabWorkItemDroppable();
    InitTableBodySortable();
});

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
        connectWith: '.project-card, .milestone-items .sortable, #assignedUserTab .nav-link:not(.active)',
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

    if (list.length) {
        list.find('.work-item-card').each(function (taskIndex, workItemElement) {
            $(workItemElement).find('.work-item-priority').text(taskIndex + 1);
        });
    }

    if (list.length === 0 || task == null) {
        return;
    }

    if (list.parents('#assignedUserTab').length) {
        updateSortableListAssignedUserTab(list, task)
        return;
    } else if (list.hasClass('project-card')) {
        updateSortableListProjectCards(list, task)
        return;
    }

    var milestoneId = list.parents('[data-milestone-id]').data('milestone-id'); 
    var userId = list.data('user-id');
    var number = task.data('number');
    var groupFieldName = list.data('group-field');
    var groupFieldValue = list.data('group-value');
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
        groupFieldName: groupFieldName,
        groupFieldValue: groupFieldValue,
        items: tasksArray,
    })
}

function TaskReorder(data) {
    console.group('TaskReorder data object');
    console.log(data);
    console.groupEnd();

    fetch('/WorkItem/SetPriorities', {
        method: 'post',
        headers: {
            "Content-Type": "application/json",
            "RequestVerificationToken": getAntiForgeryToken()
        },
        body: JSON.stringify(data)
    }).then(function (response) {
        // success fail info?
        // show new ordering in card title (target class .work-item-priority, see Dashboard/Items/_Priority.cshtml)
        return response.json();
    });
}

function updateSortableListAssignedUserTab(list, task) {
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
}

function updateSortableListProjectCards(list, task) {
    var data = {
        projectId: list.data('project-id'),
        id: task.data('number'),
    };

    console.group('updateSortableListProjectCards');
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

    fetch('/WorkItem/SetMilestone', {
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

$(document).ready(function () {
    $('.editable').each(function (index, element) {
        $(element).editable($(this).data('url'), {
            id: 'elementId',
            name: 'newName',
            before: function() {
                if ($(element).parents('h5').length) {
                    $(element).width('100%');
                } else {
                    $(element).outerWidth($(element).outerWidth());
                }
            },
            onsubmit: function() {
                $(element).width('auto');
            },
            onreset: function() {
                $(element).width('auto');
            }
        });
    });

    $('.editable')
    .on('focus', 'input', function() {
        $(this).parents('.editable').addClass('active');
    })
    .on('blur', 'input', function() {
        $(this).parents('.editable').removeClass('active');
        });

    $('.ms-datepicker-input').datepicker({
        onSelect: function (dateText, inst) {
            var msId = $(this).data('milestone-id');
            SetMilestoneDate(msId, dateText);
        },
        beforeShow: function (ele, ui) {
            var link = $(ele);
            ui.dpDiv.offset({
                top: link.offset().top + 10,
                left: link.offset().left + 10
            });
        }
    });

    $('.ms-datepicker').click(function () {
        $(this).next('.ms-datepicker-input').datepicker('show');
    });

    $('.clear-notification').click(function (ev) {        
        var data = {
            id: $(ev.target).data("work-item-number")
        };

        var formData = getFormData(data);
        fetch('/WorkItem/ClearNotification', {
            method: 'post',
            body: formData
        }).then(function (response) {
            return response.json();
        }).then(function (data) {
            if (data.success) {
                RemoveWorkItem(ev);
            } else {
                console.log(data.message);
            }
        });        
    });
});

function SetMilestoneDate(milestoneId, dateText) {

    let data = {
        milestoneId: milestoneId,
        dateText: dateText
    };

    fetch('/Update/MilestoneDate', {
        method: 'post',
        headers: {
            "Content-Type": "application/json",
            "RequestVerificationToken": getAntiForgeryToken()
        },
        body: JSON.stringify(data)
    }).then(function (response) {
        // success fail info?
        return response.text();
    }).then(function (text) {
        $(".ms-datepicker[data-milestone-id='" + milestoneId + "']").html(text);
    });
}