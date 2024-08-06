using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TechTales.Helpers;

public static class HtmlHelperExtensions
{
    public static HtmlString UnderlineInput(this IHtmlHelper htmlHelper,
        string type, string placeholder = "", string id = "", bool required = false,
        string aspFor = "")
    {
        string requiredStr = required ? "required" : string.Empty;
        return new HtmlString(
            $"""
            <input class="underline-input" 
                name="{aspFor}"
                type="{type}"
                placeholder="{placeholder}"
                id="{id}"
                {requiredStr} />
            """
        );
    }

    public static HtmlString UnderlinePasswordInput(this IHtmlHelper htmlHelper,
        string placeholder = "", string id = "", bool required = false, string eyeId = "",
        string aspFor = "")
    {
        string requiredStr = required ? "required" : string.Empty;
        return new HtmlString(
            $"""
            <div class="underline-password-input-container">
                <input placeholder="{placeholder}"
                    type="password"
                    name="{aspFor}"
                    id="{id}"
                    {requiredStr} />
                <img alt="eye&#x2205;" 
                    width="48px"
                    id="{eyeId}"
                    src="/images/eye_slash.svg" />
            </div>
            """
        );
    }

    public static HtmlString RoundedButton(this IHtmlHelper htmlHelper, string text, 
        string background = "black", string margin = "0", string padding = "16px 64px",
        string width = "initial", string type = "button")
    {
        return new HtmlString(
            $"""
            <button type="{type}" 
                style="
                text-align: center;
                color:white;
                background-color: {background};
                border-radius: 32px;
                padding: {padding};
                font-size: 18px;
                text-transform: uppercase;
                margin: {margin};
                width: {width};
            "
            >{text}</button>
            """
        );
    }

    public static HtmlString EditBlogButtons(this IHtmlHelper htmlHelper, string editHref = "#",
        string deleteHref = "#", string editFill = "black", string deleteFill = "black", int width = 72, int height = 72)
    {
        var edit = SvgContainerHelper.GetEdit(width, height, editFill);
        var delete = SvgContainerHelper.GetDelete(width, height, deleteFill);
        
        var editLink = editHref == "#" ? edit : $"<a href=\"{editHref}\">{edit}</a>";
        var deleteLink = deleteHref == "#" ? delete : $"<a href=\"{deleteHref}\">{delete}</a>";

        return new HtmlString(
            $"""
            <div class="edit-blog-section">
                <button style="width: {width}px; height: {height}px;">
                    {editLink}
                </button>
                <button style="width: {width}px; height: {height}px;">
                    {deleteLink}
                </button>
            </div>
            """
        );
    }
}