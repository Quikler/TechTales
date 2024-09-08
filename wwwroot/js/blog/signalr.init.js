// establish signalR connection with '/commentHub'
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/commentHub")
    .build();

connection.on("CommentDeleted", function (commentId) {
    const commentElement = document.getElementById(commentId);
    if (commentElement) {
        const commentContainerLi = commentElement.closest("li");
        commentContainerLi.remove();
    }
});

connection.on("CommentAddedExceptCaller", function (comment) {
    const commentsList = document.querySelector(".comments-list");

    const liStr = createLi(comment, false);
    
    commentsList.insertAdjacentHTML("afterbegin", liStr);
    
    // const li = commentsList.querySelector("li");
    // li.clientHeight;
    // li.style.opacity = "1";
});

connection.on("CommentAddedCaller", function (comment) {
    const commentsList = document.querySelector(".comments-list");

    const liStr = createLi(comment, true);
    
    commentsList.insertAdjacentHTML("afterbegin", liStr);
    
    // const li = commentsList.querySelector("li");
    // li.clientHeight;
    // li.style.opacity = "1";
});

connection.on("CommentEdit", function (commentId, commentContent) {
     document.getElementById(commentId).textContent = commentContent;
});

// start listen
connection.start().catch(function (err) {
    return console.error("[SignalR]:error ", err.toString());
});