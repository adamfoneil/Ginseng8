var frm = document.getElementById("frmNewItem");
frm.onsubmit = () => {
    let formData = new FormData(frm);
    fetch('/WorkItem/Create', {
        method: 'post',
        body: new URLSearchParams(formData)
    }).then(function (response) {
        response.text().then(function (text) {
            var output = document.getElementById("workItemResult");
            output.innerHTML = text;
        });
    });
    return false;
};

var btnItemDetail = document.getElementById("btnAddItemDetail");
btnItemDetail.addEventListener("click", function (e) {
    var div = document.getElementById("newItemEditorDiv");
    $(div).slideToggle();
});