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

var bodyEditLinks = document.querySelectorAll('.itemEdit');
bodyEditLinks.forEach(function (e) {
    e.addEventListener("click", function (e) {
        var number = e.target.getAttribute("data-number");
        $("#body-display-" + number).hide();
        $("#body-edit-" + number).show();
        var settings = GetFroalaSettings();
        $('#HtmlBody-' + number).froalaEditor(settings);
        $(e.target).hide();
    });
});

var editCancelLinks = document.querySelectorAll('.itemCancelEdit');
editCancelLinks.forEach(function (e) {
    e.addEventListener('click', function (e) {
        var number = e.target.getAttribute("data-number");
        $("#body-display-" + number).show();
        $("#body-edit-" + number).hide();
        $("#itemEditLink-" + number).show();
    });
});

var bodyUpdateButtons = document.querySelectorAll('.itemBodyUpdate');
bodyUpdateButtons.forEach(function (e) {
    e.addEventListener('click', function (e) {
        var frm = e.target.form;
        var number = frm['Number'].value;
        var failImg = document.getElementById('update-failed-' + number);

        try {
            let formData = new FormData(frm);
            fetch('/Update/WorkItemBody', {
                method: 'post',
                body: new URLSearchParams(formData)
            }).then(function (response) {
                return response.json();
            }).then(function (result) {
                if (result.success) {
                    $("#body-display-" + number).html($("#HtmlBody-" + number).val());
                    $("#body-display-" + number).show();
                    $("#body-edit-" + number).hide();
                    $("#itemEditLink-" + number).show();
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