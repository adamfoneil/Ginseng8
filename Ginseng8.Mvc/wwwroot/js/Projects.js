var viewProjectDetailLinks = document.querySelectorAll('.viewProjectBody');
viewProjectDetailLinks.forEach(function (e) {
    e.addEventListener('click', function (e) {
        var projectId = e.target.getAttribute('data-project-id');
        var viewDiv = document.getElementById('project-view-' + projectId);
        $(viewDiv).slideToggle();
    });
});