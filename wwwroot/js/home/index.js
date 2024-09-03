const input = document.querySelector(".input-block > input");
const button = document.querySelector(".input-block > button");

function redirectToUrl() {
    const url = `/Filter/Find?request=${encodeURIComponent(input.value)}`;
    window.location.href = url;
}

input.addEventListener("keydown", function (e) {
    if (e.keyCode == 13 && input.value) {
        button.focus();
        redirectToUrl();
    } else {
        input.reportValidity();
    }
});

button.addEventListener("click", function () {
    input.value ? redirectToUrl() : input.reportValidity();
});