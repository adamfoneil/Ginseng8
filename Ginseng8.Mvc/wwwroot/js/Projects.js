var viewProjectDetailLinks = document.querySelectorAll('.viewProjectBody');
viewProjectDetailLinks.forEach(function (e) {
    e.addEventListener('click', function (e) {
        var projectId = e.target.getAttribute('data-project-id');
        var hasBody = (e.target.getAttribute('data-project-has-body') == 'true');
        var showDiv = (hasBody) ?
            document.getElementById('project-view-' + projectId) :
            document.getElementById('project-edit-' + projectId);

        $(showDiv).slideToggle();
    });
});