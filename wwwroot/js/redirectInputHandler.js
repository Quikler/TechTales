const input = document.querySelector(".input-block > input");
const button = document.querySelector(".input-block > button");

function redirectToUrl() {
    const url = `${input.dataset.url}?request=${encodeURIComponent(input.value)}`;
    window.location.href = url;
}

input.addEventListener("keydown", function (e) {
    if (e.key === "Enter" && input.value) {
        redirectToUrl();
    } else if (e.key === "Enter") {
        input.reportValidity();
    }
});

button.addEventListener("click", function () {
    input.value ? redirectToUrl() : input.reportValidity();
});