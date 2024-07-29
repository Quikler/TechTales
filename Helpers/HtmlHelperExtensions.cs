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
}