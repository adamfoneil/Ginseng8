function UpdateField(frm, url, divId, linkClass, callbackFunction) {
    let formData = new FormData(frm);
    fetch(url, {
        method: 'post',
        body: new URLSearchParams(formData)
    }).then(function (response) {
        response.text().then(function (text) {
            var output = document.getElementById(divId);
            output.innerHTML = text;
            var links = output.querySelectorAll(linkClass);
            // re-create the event handler on newly inserted content
            links.forEach(function (e) {
                e.addEventListener("click", callbackFunction);
            });
        });
    });
}

function OnSetMilestone(e) {
    var link = e.target;
    var number = link.getAttribute("data-number");
    var msId = link.getAttribute("data-milestone-id");

    var frm = document.getElementById("frmSetMilestone");
    frm.number.value = number;
    frm.milestoneId.value = msId;

    UpdateField(frm, "/WorkItem/SetMilestone", "Milestone-" + number, ".setMilestone", OnSetMilestone);
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

    UpdateField(frm, "/WorkItem/SetActivity", "Activity-" + number, ".setActivity", OnSetActivity);
}

var activityLinks = document.querySelectorAll(".setActivity");
activityLinks.forEach(function (e) {
    e.addEventListener("click", OnSetActivity);
});

function OnSetEstimate(e) {
    var link = e.target;
    var number = link.getAttribute("data-number");
    var sizeId = link.getAttribute("data-size-id");

    var frm = document.getElementById("frmSetEstimate");
    frm.number.value = number;
    frm.sizeId.value = sizeId;

    UpdateField(frm, "/WorkItem/SetEstimate", "Estimate-" + number, ".setEstimate", OnSetEstimate);
}

var estimateLinks = document.querySelectorAll(".setEstimate");
estimateLinks.forEach(function (e) {
    e.addEventListener("click", OnSetEstimate);
});