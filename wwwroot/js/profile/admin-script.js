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
            const result = await response.json(); // Parse the JSON response
            alert(result.message); // Assuming your backend sends a JSON object with a message property
            window.location.href = '/Profile/List';
        } else {
            const errorData = await response.json(); // Parse the JSON response for error
            alert('Error: ' + errorData.message); // Assuming your backend sends a JSON object with a message property
        }
    })
    .catch(error => console.error('Error:', error));
    
    return false; // Prevent default action of the anchor tag
}