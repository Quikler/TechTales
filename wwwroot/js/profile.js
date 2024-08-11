window.addEventListener('load', function() {
    try {
        const mainContainer = document.querySelector('.main-container');
        const blogsContainer = mainContainer.querySelector('.blogs-container');
    
        const userInfoContainerChild = document.querySelector('.user-info-container > div');

        const availableBlogsH2 = document.querySelector('.mb-3.d-flex.justify-content-between.align-items-center');

        const maxHeight = `${userInfoContainerChild.offsetHeight - availableBlogsH2.offsetHeight - getComputedStyle(availableBlogsH2).marginBottom.replace('px', '')}px`;
        console.log(`Available blogs max-height: ${maxHeight}`);
        blogsContainer.style.maxHeight = maxHeight;
    } catch (error) {
        
    }
});

document.getElementById("changePicture").addEventListener("change", function(event) {
    try {
        const file = event.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = function(e) {
                document.getElementById("user-profile-image").src = e.target.result;
            }
            reader.readAsDataURL(file);
        }
    } catch (error) {
        
    }
});