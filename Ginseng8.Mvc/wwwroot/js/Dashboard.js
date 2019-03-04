var milestoneLinks = document.querySelectorAll(".setMilestone");
milestoneLinks.forEach(function (e) {
    e.addEventListener("click", function (e) {
        var link = e.target;
        var number = link.getAttribute("data-number");
        var msId = link.getAttribute("data-milestone-id");
        var frm = document.getElementById("frmSetMilestone");
        frm.number.value = number;
        frm.milestoneId.value = msId;
        frm.submit();
    });
});

var frmMilestone = document.getElementById("frmSetMilestone");
frmMilestone.onsubmit = () => {
    var msId = frmMilestone.milestoneId.value;
    let formData = new FormData(frmMilestone);
    fetch('/WorkItem/SetMilestone', {
        method: 'post',
        body: new URLSearchParams(formData)
    }).then(function (response) {
        response.text().then(function (text) {
            var output = document.getElementById("Milestone-" + msId);
            output.innerHTML = text;
        });
    });
    return false;
};