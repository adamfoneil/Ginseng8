function getFormData(object) {
    var formData = new FormData();
    for (var key in object) formData.append(key, object[key]);
    formData.append("__RequestVerificationToken", getAntiForgeryToken());
    return formData;
}

function getAntiForgeryToken() {
    var div = document.getElementById('antiForgeryToken');
    var input = div.getElementsByTagName('input')[0];
    return input.value;
}

