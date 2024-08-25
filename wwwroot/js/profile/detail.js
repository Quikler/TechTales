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