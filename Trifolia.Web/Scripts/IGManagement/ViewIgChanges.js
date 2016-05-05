function ToggleChanges(source, dest) {
    $(source).next(dest).toggle();

    var currentText = $(source).html();
    if (currentText.indexOf("Show") == 0)
        $(source).html(currentText.replace("Show", "Hide"));
    else
        $(source).html(currentText.replace("Hide", "Show"));
}