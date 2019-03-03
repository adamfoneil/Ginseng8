var milestoneLinks = document.querySelectorAll(".setMilestone");
milestoneLinks.forEach(function (e) {
    e.addEventListener("click", function (e) {
        var link = e.target;
        var number = link.getAttribute("data-number");
        var msId = link.getAttribute("data-milestone-id");
        
    });
});