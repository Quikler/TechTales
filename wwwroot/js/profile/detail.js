window.addEventListener('load', function() {
    try {
        const blogs = document.querySelector('.blogs-container');
        const userInfoContainer = document.querySelector('.user-info-container');

        const maxHeight = `${userInfoContainer.offsetHeight}px`;
        console.log(`Blogs container max-height: ${maxHeight}`);
        blogs.style.maxHeight = maxHeight;
    } catch (error) {
        
    }
});