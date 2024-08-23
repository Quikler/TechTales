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
        var value = htmlHelper.ViewData.ModelExplorer.GetExplorerForProperty(aspFor)?.Model?.ToString() ?? string.Empty;
        
        return new HtmlString(
            $"""
            <input class="underline-input" 
                name="{aspFor}"
                value="{value}"
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
        var value = htmlHelper.ViewData.ModelExplorer.GetExplorerForProperty(aspFor)?.Model?.ToString() ?? string.Empty;

        return new HtmlString(
            $"""
            <div class="underline-password-input-container">
                <input placeholder="{placeholder}"
                    type="password"
                    name="{aspFor}"
                    value="{value}"
                    id="{id}"
                    autocomplete="new-password"
                    autofill="off"
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
        string width = "initial", string type = "button", string js = "", string @class = "")
    {
        return new HtmlString(
            $"""
            <button type="{type}" 
                {js}
                class="{@class}"
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
        string deleteHref = "#", string editFill = "black", string deleteFill = "black",
        int width = 72, int height = 72, string editType = "button", string deleteType = "button",
        string editClass = "", string deleteClass = "")
    {
        var edit = SvgContainerHelper.GetEdit(width, height, editFill);
        var delete = SvgContainerHelper.GetDelete(width, height, deleteFill);
        
        // var editLink = editHref == "#" ? edit : $"<a class=\"d-flex\" href=\"{editHref}\">{edit}</a>";
        // var deleteLink = deleteHref == "#" ? delete : $"<a class=\"d-flex\" href=\"{deleteHref}\">{delete}</a>";

        // var editLink = editHref == "#" ? edit : $"<a class=\"{editClass} p-0 border-0 btn btn-outline-light\" href=\"{editHref}\">{edit}</a>";
        // var deleteLink = deleteHref == "#" ? delete : $"<a class=\"{deleteClass} p-0 border-0 btn btn-outline-light\" href=\"{deleteHref}\">{delete}</a>";

        // return new HtmlString(
        //     $"""
        //     <button type="{editType}" class="{editClass}" style="width: {width}px; height: {height}px;">
        //         {editLink}
        //     </button>
        //     <button type="{deleteType}" class="{deleteClass}" style="width: {width}px; height: {height}px;">
        //         {deleteLink}
        //     </button>
        //     """
        // );

        editHref = editHref == "#" ? "javascript:void(0);" : editHref;
        deleteHref = deleteHref == "#" ? "javascript:void(0);" : deleteHref;

        return new HtmlString(
            $"""
            <a class="{editClass} p-0 border-0 btn" 
                href="{editHref}">
                {edit}
            </a>
            <a class="{deleteClass} p-0 border-0 btn"
                href="{deleteHref}">
                {delete}
            </a>
            """
        );
    }
}