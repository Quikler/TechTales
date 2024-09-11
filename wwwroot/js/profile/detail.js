window.addEventListener('load', function() {
    resizeContainer();
});

window.addEventListener('resize', function () {
    resizeContainer();
});

function resizeContainer() {
    const blogs = document.querySelector('.blogs-container');
    const userInfoContainer = document.querySelector('.user-info-container');

    const maxHeight = `${userInfoContainer.offsetHeight}px`;
    blogs.style.maxHeight = maxHeight;
}

function deleteBlog(anchor) {
    const url = anchor.dataset.url;
    const redirect = anchor.dataset.redirect;

    fetch(url, {
        method: 'DELETE',
        headers: {
            'RequestVerificationToken': document
                .querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
    .then(response => {
        if (response.ok) {
            alert('Blog deleted successfully');
            window.location.href = redirect;
        } else {
            return response.json().then(data => {
                alert('Error: ' + data.message);
            });
        }
    })
    .catch(error => console.error('Error:', error));

    return false; // Prevent the default anchor behavior
}