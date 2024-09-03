document.querySelector(".submit-button").addEventListener("click", function () {
    var titleInput = document.querySelector('input[aria-label="Title"]');
    var contentTextarea = document.querySelector('textarea[aria-label="Content"]');

    var isTitleValid = titleInput.value.trim() !== "";
    var isContentValid = contentTextarea.value.trim() !== "";

    if (!isTitleValid) {
        document.querySelector('#modalCenter .modal-body').textContent = "'Title' field cannot be empty.";
        console.error("'Title' field is empty.");

        $('#modalCenter').modal('show');
        titleInput.focus();
        return;
    }

    if (!isContentValid) {
        document.querySelector('#modalCenter .modal-body').textContent = "'Content' field cannot be empty.";
        console.error("'Content' field is empty.");

        $('#modalCenter').modal('show');
        contentTextarea.focus();
        return;
    }

    document.querySelector(".create-form").submit();
});