var _a, _b;
(_a = document.querySelector(".change-picture-btn")) === null || _a === void 0 ? void 0 : _a.addEventListener("click", function () {
    var _a;
    (_a = document.getElementById('changePicture')) === null || _a === void 0 ? void 0 : _a.click();
});
(_b = document.getElementById("changePicture")) === null || _b === void 0 ? void 0 : _b.addEventListener("change", function (event) {
    var inputFile = event === null || event === void 0 ? void 0 : event.target;
    var file = (inputFile === null || inputFile === void 0 ? void 0 : inputFile.files) == null ? null : inputFile.files[0];
    if (file) {
        var reader = new FileReader();
        reader.onload = function (e) {
            var _a;
            var profileImage = document.getElementById("user-profile-image");
            if (profileImage) {
                profileImage.src = (_a = e === null || e === void 0 ? void 0 : e.target) === null || _a === void 0 ? void 0 : _a.result;
            }
        };
        reader.readAsDataURL(file);
    }
});
