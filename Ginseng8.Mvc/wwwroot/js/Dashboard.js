function OnSetMilestone(e) {
    var link = e.target;
    var number = link.getAttribute("data-number");
    var msId = link.getAttribute("data-milestone-id");

    var frm = document.getElementById("frmSetMilestone");
    frm.number.value = number;
    frm.milestoneId.value = msId;

    let formData = new FormData(frm);
    fetch('/WorkItem/SetMilestone', {
        method: 'post',
        body: new URLSearchParams(formData)
    }).then(function (response) {
        response.text().then(function (text) {
            var output = document.getElementById("Milestone-" + number);
            output.innerHTML = text;            
            var links = output.querySelectorAll(".setMilestone");
            // re-create the event handler on newly inserted content
            links.forEach(function (e) {
                e.addEventListener("click", OnSetMilestone);
            });
        });
    });
}

var milestoneLinks = document.querySelectorAll(".setMilestone");
milestoneLinks.forEach(function (e) {
    e.addEventListener("click", OnSetMilestone);
});

function OnSetActivity(e) {
    var link = e.target;
    var number = link.getAttribute("data-number");
    var activityId = link.getAttribute("data-activity-id");

    var frm = document.getElementById("frmSetActivity");
    frm.number.value = number;
    frm.activityId.value = activityId;

    let formData = new FormData(frm);
    fetch('/WorkItem/SetActivity', {
        method: 'post',
        body: new URLSearchParams(formData)
    }).then(function (response) {
        response.text().then(function (text) {
            var output = document.getElementById("Activity-" + number);
            output.innerHTML = text;
            var links = output.querySelectorAll(".setActivity");
            // re-create the event handler on newly inserted content
            links.forEach(function (e) {
                e.addEventListener("click", OnSetActivity);
            });
        });
    });
}

var activityLinks = document.querySelectorAll(".setActivity");
activityLinks.forEach(function (e) {
    e.addEventListener("click", OnSetActivity);
});
