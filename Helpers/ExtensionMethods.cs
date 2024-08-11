using System.Text.RegularExpressions;

namespace TechTales.Helpers;

public static class ExtensionMethods
{
    public static string BlobToImageSrc(byte[]? bytes, 
        string defaultImage = "/images/default_user_icon.svg")
    {
        string base64String = bytes != null && bytes.Length > 0 
            ? Convert.ToBase64String(bytes) 
            : string.Empty;

        string imageSrc = !string.IsNullOrEmpty(base64String) 
            ? $"data:image/png;base64,{base64String}" 
            : defaultImage;

        return imageSrc;
    }

    public static string EllipsisString(this string input, int end)
    {
        if (input.Length > end)
        {
            input = string.Concat(input[..end], "...");
        }
        return input;
    }
}