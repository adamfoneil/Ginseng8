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
            id: element.getAttribute("data-number"),
            activityId: element.getAttribute("data-activity-id")            
        };
        var formData = new FormData();
        for (var key in data) formData.append(key, data[key]);
        formData.append("__RequestVerificationToken", getAntiForgeryToken());
        
        fetch('/WorkItem/SelfStartActivity', {
            method: 'post',
            body: formData
        }).then(function (response) {
            return response.json();
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
});

function getAntiForgeryToken() {
    var div = document.getElementById('antiForgeryToken');
    var input = div.getElementsByTagName('input')[0];
    return input.value;
}