using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TechTales.Helpers;

public static class HtmlHelperExtensions
{
    public static HtmlString UnderlineInput(this IHtmlHelper htmlHelper,
        string type, string name, string placeholder = "", string id = "", bool required = false)
    {
        string requiredStr = required ? "required" : string.Empty;
        return new HtmlString(
            $"""
            <input class="underline-input" 
                   type="{type}"
                   name="{name}"
                   placeholder="{placeholder}"
                   id="{id}"
                   {requiredStr} />
            """
        );
    }

    public static HtmlString UnderlinePasswordInput(this IHtmlHelper htmlHelper,
        string name, string? placeholder = "", string? id = "", bool required = false, string? eyeId = "")
    {
        string requiredStr = required ? "required" : string.Empty;
        return new HtmlString(
            $"""
            <div class="underline-password-input-container">
                <input placeholder="{placeholder}"
                       type="password"
                       id="{id}"
                       name="{name}"
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
        string width = "initial")
    {
        return new HtmlString(
            $"""
            <button style="
                text-align: center;
                color:white;
                background-color: {background};
                border-radius: 32px;
                padding: {padding};
                font-size: 18px;
                text-transform: uppercase;
                display: flex;
                justify-content: center;
                margin: {margin};
                width: {width};
            "
            >{text}</button>
            """
        );
    }

    public static HtmlString EditBlogButtons(this IHtmlHelper htmlHelper, string editHref = "#",
        string deleteHref = "#")
    {
        var edit = SvgContainerHelper.GetEdit();
        var delete = SvgContainerHelper.GetDelete();
        
        var editLink = editHref == "#" ? edit : $"<a href=\"{editHref}\">{edit}</a>";
        var deleteLink = deleteHref == "#" ? delete : $"<a href=\"{deleteHref}\">{delete}</a>";

        return new HtmlString(
            $"""
            <div class="edit-blog-section">
                <button>
                    {editLink}
                </button>
                <button>
                    {deleteLink}
                </button>
            </div>
            """
        );
    }
}