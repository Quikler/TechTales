window.addEventListener('load', function() {
    try {
        const mainContainer = document.querySelector('.main-container');
        const blogsContainer = mainContainer.querySelector('.blogs-container');
    
        const userInfoContainerChild = document.querySelector('.user-info-container > div');

        const availableBlogsContainer = document.querySelector('.available-blogs-container');

        const maxHeight = `${userInfoContainerChild.offsetHeight - availableBlogsContainer.offsetHeight - 14}px`;
        console.log(`Available blogs max-height: ${maxHeight}`);
        blogsContainer.style.maxHeight = maxHeight;
    } catch (error) {
        
    }
});

// document.getElementById("changePicture").addEventListener("change", function(event) {
//     const file = event.target.files[0];
//     if (file) {
//         const reader = new FileReader();
//         reader.onload = function(e) {
//             document.getElementById("user-profile-image").src = e.target.result;
//         }
//         reader.readAsDataURL(file);
//     }
// });