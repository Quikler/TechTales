// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.querySelector(".search-input").addEventListener("keydown", function (e) {
    if(e.keyCode == 13 && this.value) {
        var url = `/Filter/Find?request=${encodeURIComponent(this.value)}`;
        window.location.href = url;
    }
    else {
        this.reportValidity();
    }
});