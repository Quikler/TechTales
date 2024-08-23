document.querySelector(".change-picture-btn")?.addEventListener("click", function () {
    document.getElementById('changePicture')?.click();
});

document.getElementById("changePicture")?.addEventListener("change", function(event) {
    const inputFile = event?.target as HTMLInputElement;
    const file = inputFile?.files == null ? null : inputFile.files[0];

    if (file) {
        const reader = new FileReader();
        reader.onload = function(e) {
            const profileImage = document.getElementById("user-profile-image") as HTMLImageElement;

            if (profileImage) {
                profileImage.src = e?.target?.result as string;
            }
        }
        reader.readAsDataURL(file);
    }
});