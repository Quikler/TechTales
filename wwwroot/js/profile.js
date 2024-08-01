window.addEventListener('load', function() {
    const mainContainer = document.querySelector('.main-container');
    const blogsContainer = mainContainer.querySelector('.blogs-container');
   
    const userInfoContainerChild = document.querySelector('.user-info-container > div');

    //const availableBlogsH2 = document.querySelector('.profile-container > div > h2');
    const availableBlogsH2 = document.getElementById('h4-avalable-blogs');

    const maxHeight = `${userInfoContainerChild.offsetHeight - availableBlogsH2.offsetHeight - getComputedStyle(availableBlogsH2).marginBottom.replace('px', '')}px`;
    console.log(`Available blogs max-height: ${maxHeight}`);
    blogsContainer.style.maxHeight = maxHeight;
});