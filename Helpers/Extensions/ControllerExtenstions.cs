using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TechTales.Helpers.Extensions;

public static class ControllerExtensions
{
    public static void SetModalMessage(this Controller controller, string title, string content)
    {
        controller.TempData["ModalTitle"] = title;
        controller.TempData["ModalContent"] = content;
    }

    public static IEnumerable<string> ParseModalErrors(this Controller controller)
    {
        return controller.ModelState
            .Where(ms => ms.Value?.Errors.Count > 0)
            .Select(ms => $"{ms.Key} - {string.Join(", ", ms.Value!.Errors.Select(e => e.ErrorMessage))}");
    }

    public static void ParseModalErrorsAndSet(this Controller controller, string title)
    {
        var content = string.Join('\n', controller.ParseModalErrors());
        controller.SetModalMessage(title, content);
    }

    public static bool IsPageValid(this Controller controller, int page, int totalPages)
    {
        return page > 0 && page <= totalPages;
    }

    public static string? GetVisitorId(this Controller controller)
    {
        if (controller.Request.Cookies.ContainsKey("VisitorId"))
        {
            return controller.Request.Cookies["VisitorId"];
        }

        var visitorId = Guid.NewGuid().ToString();

        CookieOptions options = new CookieOptions
        {
            Expires = DateTime.Now.AddYears(1),
            HttpOnly = true,
            IsEssential = true,
        };

        controller.Response.Cookies.Append("VisitorId", visitorId, options);

        return visitorId;
    }
}