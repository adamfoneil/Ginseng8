var updateFields = document.querySelectorAll('.itemUpdate');
updateFields.forEach(function (e) {
    e.addEventListener("change", function (e) {
        var frm = e.target.form;
        var number = frm['Number'].value;
        var loadingImg = document.getElementById('loading-' + number);
        loadingImg.classList.add('visible');

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
            failImg.setAttribute('title', result.message);
        } finally {
            $(loadingImg).hide();
        }
    });
});