var _trID = null;
var _rowID = null;
var _saveForm = null;
var _aSpanIDs = null;

function DataGridEditRow(trID, aSpanIDs, rowID, saveFormID, onEditFunction) {
    if (_trID != null) DataGridCancelEdit();

    for (var i = 0; i < aSpanIDs.length; i++) {
        document.getElementById("edit-" + aSpanIDs[i]).style.display = "block";
        document.getElementById("display-" + aSpanIDs[i]).style.display = "none";
    }

    document.getElementById(trID + "-clean").style.display = "none";
    document.getElementById(trID + "-dirty").style.display = "block";

    _aSpanIDs = aSpanIDs;
    _trID = trID;
    _rowID = rowID;
    _saveForm = document.getElementById(saveFormID);
    if (_saveForm) _saveForm.elements["ID"].value = rowID;

    if (onEditFunction != null) onEditFunction(rowID);
}

function DataGridCancelEdit(onActionCompletedFunction) {
    if (_aSpanIDs != null) {
        for (var i = 0; i < _aSpanIDs.length; i++) {
            document.getElementById("edit-" + _aSpanIDs[i]).style.display = "none";
            document.getElementById("display-" + _aSpanIDs[i]).style.display = "block";
        }

        document.getElementById(_trID + "-clean").style.display = "block";
        document.getElementById(_trID + "-dirty").style.display = "none";

        if (onActionCompletedFunction != null) onActionCompletedFunction(_rowID);

        _rowID = null;
        _trID = null;
        _aSpanIDs = null;
        _saveForm = null;
        _deleteForm = null;
    }
}

function DataGridDeleteRow(deleteFormID, rowID) {
    if (confirm("This will delete the record.")) {
        var form = document.getElementById(deleteFormID);
        form.elements["ID"].value = rowID;
        form.submit();
    }
}

function DataGridInsertRow(saveFormID, aFieldIDs, aFieldNames, validationFunction, onActionCompletedFunction) {
    var form = document.getElementById(saveFormID);
    CopyFieldValues(form, aFieldIDs, aFieldNames);
    form.submit();
    if (onActionCompleteFunction != null) onActionCompletedFunction(0);
}

function DataGridSaveRow(aFieldIDs, aFieldNames, validationFunction, onActionCompletedFunction) {
    CopyFieldValues(_saveForm, aFieldIDs, aFieldNames);
    _saveForm.submit();
    if (onActionCompletedFunction != null) onActionCompletedFunction(_rowID);
}

function CopyFieldValues(targetForm, aFieldIDs, aFieldNames) {
    for (var i = 0; i < aFieldIDs.length; i++) {
        var targetField = targetForm.elements[aFieldNames[i]];
        var sourceField = document.getElementById(aFieldIDs[i]);
        switch (sourceField.type) {
            case "text":
            case "textarea":
            case "select-one":
            case "hidden":
                var value = $(sourceField).val() + "";
                // leading dollar signs need to be trimmed
                if (value.substring(0, 1) == "$") value = value.substring(1, value.length);
                $(targetField).val(value);
                break;
            case "checkbox":
                $(targetField).val($(sourceField).prop('checked') ? "true" : "false");
                break;
        }
    }
}

function BuildDataObject(aFieldIDs, aFieldNames, extraFields, extraValues) {
    // thanks to http://stackoverflow.com/questions/4215737/convert-array-to-object

    var result = {};
    for (var i = 0; i < aFieldNames.length; i++) {
        var sourceField = document.getElementById(aFieldIDs[i]);
        switch (sourceField.type) {
            case "text":
            case "select-one":
            case "hidden":
                var value = $("#" + aFieldIDs[i]).val();
                // leading dollar signs need to be trimmed
                if (value.substring(0, 1) == "$") value = value.substring(1, value.length - 1);
                result[aFieldNames[i]] = value;
                break;
            case "checkbox":
                result[aFieldNames[i]] = $("#" + aFieldIDs[i]).is(":checked");
                break;
        }
    }

    if (extraFields != null) {
        for (var i = 0; i < extraFields.length; i++) {
            result[extraFields[i]] = extraValues[i];
        }
    }

    return result;
}