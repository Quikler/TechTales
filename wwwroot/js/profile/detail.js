window.addEventListener('load', function() {
    const blogs = document.querySelector('.blogs-container');
    const userInfoContainer = document.querySelector('.user-info-container');

    const maxHeight = `${userInfoContainer.offsetHeight}px`;
    blogs.style.maxHeight = maxHeight;
});