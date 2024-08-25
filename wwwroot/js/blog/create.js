document.querySelector(".submit-button").addEventListener("click", function () {
    var titleInput = document.querySelector('input[aria-label="Title"]');
    var contentTextarea = document.querySelector('textarea[aria-label="Content"]');

    var isTitleValid = titleInput.value.trim() !== "";
    var isContentValid = contentTextarea.value.trim() !== "";

    if (!isTitleValid) {
        document.querySelector('#errorModalCenter .modal-body').textContent = "'Title' field cannot be empty.";

        $('#errorModalCenter').modal('show');
        titleInput.focus();
        return;
    }

    if (!isContentValid) {
        document.querySelector('#errorModalCenter .modal-body').textContent = "'Content' field cannot be empty.";

        $('#errorModalCenter').modal('show');
        contentTextarea.focus();
        return;
    }

    document.querySelector(".create-form").submit();
});