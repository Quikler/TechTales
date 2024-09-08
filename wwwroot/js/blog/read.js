// intial value comments
const initialComments = Array.from(document.querySelectorAll(".comment-content")).reduce((acc, comment) => {
    acc[comment.id] = comment.innerHTML;
    return acc;
}, {});

// Handle <ol> comments-list class element that associated with comments <li>
document.querySelector(".comments-list").addEventListener("click", function (event) {

// Target button that was clicked
    const target = event.target;

    try {
        // Find the parent element (<li>) that contains comment
        var commentContainer = target.closest("li");
        var commentContent = commentContainer.querySelector(".comment-content");
    } catch (error) {
        return;
    }

//#region Handle edit comment button
    if (target.classList.contains("edit-comment-btn")) {
        setContentEditable(commentContent, commentContainer);
    }
//#endregion

//#region Handle submit comment button [SignalR]
    else if (target.classList.contains("submit-comment-button")) {
        const newCommentContent = commentContent.innerHTML;

        if (initialComments[commentContent.id] == newCommentContent) {
            removeContentEditable(commentContent, commentContainer);
            target.blur();
            return;
        }
        
        fetch(`/Comment/Edit?id=${encodeURIComponent(commentContent.id)}&content=${encodeURIComponent(newCommentContent)}`, {
            method: 'PATCH',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document
                    .getElementById('submit-edit-comment-form')
                    .querySelector('input[name="__RequestVerificationToken"]').value,
            }
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response);
            }
            return response.text();
        })
        .then(data => {
            console.log("[EditComment]:now Response received from server:", data);

            // Notify the server to broadcast the edit event to clients
            connection.invoke("EditComment", commentContent.id, data).catch(function (err) {
                return console.error(err.toString());
            });
        })
        .catch(error => console.error("[EditComment]:error There was a problem with the fetch operation:", error));

        removeContentEditable(commentContent, commentContainer);
    }
//#endregion

//#region Handle delete comment button [SignalR]
    else if (target.classList.contains("delete-comment-btn")) {
        target.disabled = true;
        commentContainer.querySelector(".edit-comment-btn").disabled = true;

        fetch(`/Comment/Delete?id=${encodeURIComponent(commentContent.id)}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document
                    .getElementById('delete-comment-form')
                    .querySelector('input[name="__RequestVerificationToken"]').value,
            }
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(response);
            }
            return response.text();
        })
        .then(data => {
            console.log("[DeleteComment]:now Response received:", data);

            // Notify the server to broadcast the deletion event to clients
            connection.invoke("DeleteComment", commentContent.id).catch(function (err) {
                return console.error(err.toString());
            });
        })
        .catch(error => console.error("[DeleteComment]:error There was a problem with the fetch operation:", error));
    }
//#endregion

//#region Handle cancel edit comment button
    else if (target.classList.contains("cancel-comment-button")) {
        // if comment edit was canceled, assign previous value to comment
        commentContent.textContent = initialComments[commentContent.id];
        removeContentEditable(commentContent, commentContainer);
    }
//#endregion

});

//#region Handle add comment button [SignalR]
document.querySelectorAll(".add-comment-btn").forEach(addButton => {
    addButton.addEventListener("click", function () {
        const blogId = document.querySelector('input[name="BlogId"]');
        const readerId = document.querySelector('input[name="ReaderId"]');
        const textAreaComment = document.querySelector(".addcomment");
        const commentValue = textAreaComment.value;

        // Reset textarea value
        textAreaComment.value = '';

        fetch(`/Comment/Add?content=${encodeURIComponent(commentValue)}&blogId=${encodeURIComponent(blogId.value)}&readerId=${encodeURIComponent(readerId.value)}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document
                    .getElementById('add-comment-form')
                    .querySelector('input[name="__RequestVerificationToken"]').value,
            },
        })
        .then(async response => {
            if (response.status == 403) {
                // Redirect to the Forbidden page
                window.location.href = '/Error/Forbidden';
                return; // Stop further processing
            }
        
            if (!response.ok) {
                const error = await response.json();
                throw error;
            }
        
            return response.json();
        })
        .then(data => {
            console.log("[AddComment]:now Response received:", data);

            initialComments[data.id] = data.content;

            // Notify the server to broadcast the addition event to clients
            connection.invoke("AddComment", data).catch(function (err) {
                return console.error(err.toString());
            });
        })
        .catch(error => {
            showModal(error.title, error.text);
            console.error("[AddComment]:error There was a problem with the fetch operation:", error)
        });
    });
});

// #endregion

function createLi(comment, editable = false) {
    const editSection = editable ? `
    <div class="d-flex justify-content-end align-items-start">
        <a class="edit-comment-btn bg-transparent btn-outline-light me-2 p-0 border-0 btn" 
            href="javascript:void(0);">
            <svg viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" width="72px" height="72px" fill="darkblue"><path d="M834.3 705.7c0 82.2-66.8 149-149 149H325.9c-82.2 0-149-66.8-149-149V346.4c0-82.2 66.8-149 149-149h129.8v-42.7H325.9c-105.7 0-191.7 86-191.7 191.7v359.3c0 105.7 86 191.7 191.7 191.7h359.3c105.7 0 191.7-86 191.7-191.7V575.9h-42.7v129.8z"></path><path d="M889.7 163.4c-22.9-22.9-53-34.4-83.1-34.4s-60.1 11.5-83.1 34.4L312 574.9c-16.9 16.9-27.9 38.8-31.2 62.5l-19 132.8c-1.6 11.4 7.3 21.3 18.4 21.3 0.9 0 1.8-0.1 2.7-0.2l132.8-19c23.7-3.4 45.6-14.3 62.5-31.2l411.5-411.5c45.9-45.9 45.9-120.3 0-166.2zM362 585.3L710.3 237 816 342.8 467.8 691.1 362 585.3zM409.7 730l-101.1 14.4L323 643.3c1.4-9.5 4.8-18.7 9.9-26.7L436.3 720c-8 5.2-17.1 8.7-26.6 10z m449.8-430.7l-13.3 13.3-105.7-105.8 13.3-13.3c14.1-14.1 32.9-21.9 52.9-21.9s38.8 7.8 52.9 21.9c29.1 29.2 29.1 76.7-0.1 105.8z"></path></svg>
        </a>
        <a class="delete-comment-btn bg-transparent btn-outline-light p-0 border-0 btn"
            href="javascript:void(0);">
            <svg viewBox="0 0 24 24" fill="darkblue" xmlns="http://www.w3.org/2000/svg" height="72px" width="72px"><path d="M7 4a2 2 0 0 1 2-2h6a2 2 0 0 1 2 2v2h4a1 1 0 1 1 0 2h-1.069l-.867 12.142A2 2 0 0 1 17.069 22H6.93a2 2 0 0 1-1.995-1.858L4.07 8H3a1 1 0 0 1 0-2h4V4zm2 2h6V4H9v2zM6.074 8l.857 12H17.07l.857-12H6.074zM10 10a1 1 0 0 1 1 1v6a1 1 0 1 1-2 0v-6a1 1 0 0 1 1-1zm4 0a1 1 0 0 1 1 1v6a1 1 0 1 1-2 0v-6a1 1 0 0 1 1-1z"></path></svg>
        </a>
    </div>` : '';

    const liStr = `
    <li class="mb-4 p-4 bg-light border rounded shadow-sm">
        <div class="d-flex justify-content-between">
            <div class="mb-3 d-flex flex-grow-1">
                <a href="/Profile/Detail/${comment.author.id}">
                    <img class="size-50-fit-cover user-img rounded-circle" src="${comment.author.avatar}">
                </a>
                <div class="ms-3 d-flex justify-content-center flex-column">
                    <p id="${comment.author.id}" class="comment-author-id m-0">
                        <a class="text-blueviolet" href="/Profile/Detail/${comment.author.id}">${comment.author.userName}</a>
                    </p>
                    <p class="m-0 text-muted">${comment.creationDate}</p>
                </div>
            </div>
            ${editSection}
        </div>
        <p id="${comment.id}" class=" m-0 mt-2 comment-content text-break ws-pl">${comment.content}</p>
        <div class="cancel-submit-comment-container text-end" hidden>
            <div class="d-flex justify-content-end">
                <button type="button"
                        class="cancel-comment-button btn-secondary btn px-4 mt-2 ms-auto text-white">Cancel</button>
                <button type="button"
                        class="submit-comment-button btn-primary btn px-4 ms-2 mt-2 text-white">Submit</button>
            </div>
        </div>
    </li>
    `;

    return liStr;
}

function removeContentEditable(tag, li) {
    tag.removeAttribute("contentEditable");
    tag.style.borderBottom = "0";
    tag.style.padding = "0";

    const buttonsContainer = li.querySelector(".cancel-submit-comment-container.text-end");
    buttonsContainer.setAttribute("hidden", "");
}

function setContentEditable(tag, li) {
    tag.setAttribute("contentEditable", "");
    tag.style.borderBottom = "1px solid blueviolet";
    tag.style.padding = "8px 16px";

    const buttonsContainer = li.querySelector(".cancel-submit-comment-container.text-end");
    buttonsContainer.removeAttribute("hidden");
}