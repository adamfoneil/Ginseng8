var milestoneLinks = document.querySelectorAll(".setMilestone");
milestoneLinks.forEach(function (e) {
    e.addEventListener("click", function (e) {
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
            });
        });

    });
});
