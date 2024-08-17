using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using TechTales.Data.Models;
using TechTales.DTOs;

namespace TechTales.Hubs;

public class CommentHub : Hub
{
    public async Task DeleteComment(Guid commentId)
    {
// Notify all connected clients that a comment has been deleted
        await Clients.All.SendAsync("CommentDeleted", commentId);
    }

    public async Task AddComment(CommentDTO comment)
    {
// Notify all connected clients except of caller that a comment has been added
        await Clients.AllExcept(Context.ConnectionId).SendAsync("CommentAddedExceptCaller", comment);
        
// Notify caller that a comment has been added (in js it will be with edit/remove buttons)
        await Clients.Caller.SendAsync("CommentAddedCaller", comment);
    }

    public async Task EditComment(Guid commentId, string content)
    {
        await Clients.All.SendAsync("CommentEdit", commentId, content);
    }
}