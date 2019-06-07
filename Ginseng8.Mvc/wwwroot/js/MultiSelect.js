function MultiSelectCheckboxLinkOnClick(ev) {
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

function InitMultiSelectCheckboxes(checkboxSelector, linkSelector) {
    var checkboxes = document.querySelectorAll(checkboxSelector);
    checkboxes.forEach(function (ele) {
        ele.addEventListener('click', function (ev) {
            ev.stopPropagation();
            var frm = ev.target.form;            
            let formData = new FormData(frm);
            fetch($(ele).data('post-url'), {
                method: 'post',
                body: new URLSearchParams(formData)
            }).then(function (response) {
                return response.text();
            }).then(function (html) {
                $('#' + $(ele).data("selected-name-span")).html(html);
            });
        })
    });

    var links = document.querySelectorAll(linkSelector);
    links.forEach(function (ele) {
        ele.addEventListener('click', MultiSelectCheckboxLinkOnClick);
    });
}