using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TechTales.Helpers;

public static class HtmlHelperExtensions
{
    public static HtmlString EditDeleteButtons(this IHtmlHelper htmlHelper, string editHref = "#",
        string deleteHref = "#", string editFill = "black", string deleteFill = "black",
        int width = 72, int height = 72, string editType = "button", string deleteType = "button",
        string editClass = "", string deleteClass = "")
    {
        var edit = SvgContainerHelper.GetEdit(width, height, editFill);
        var delete = SvgContainerHelper.GetDelete(width, height, deleteFill);
        
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