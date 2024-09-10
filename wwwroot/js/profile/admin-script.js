function deleteUser(anchor) {
    const idToDelete = anchor.dataset.id;

    fetch(`/Profile/DeleteUser?id=${encodeURIComponent(idToDelete)}`, {
        method: 'DELETE',
        headers: {
            'RequestVerificationToken': document
                .querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
    .then(async response => {
        if (response.ok) {
            window.location.href = '/Profile/List';
        } else {
            const errorData = await response.json();
            alert('Error: ' + errorData.message);
        }
    })
    .catch(error => console.error('Error:', error));
    
    return false;
}