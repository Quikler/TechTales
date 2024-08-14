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

    public static string EllipsisString(this string input, int end, int maxParagraphs = 4)
    {
        var paragraphs = input.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None);

        if (paragraphs.Length > maxParagraphs)
        {
            var biggestThan = paragraphs.Count(s => s.Length >= 60);

            if (biggestThan >= 2)
            {
                input = string.Join("\r\n", paragraphs.Take(2)) + "...";
            }
            else
            {
                input = string.Join("\r\n", paragraphs.Take(maxParagraphs)) + "...";
            }
            //return input;
        }

        if (input.Length > end)
        {
            input = string.Concat(input[..end], "...");
        }
        return input;
    }

    public static void Update<TEntity>(this ICollection<TEntity> existingItems, List<TEntity> newItems) where TEntity : class
    {
        var itemsToRemove = existingItems.Except(newItems).ToList();
        var itemsToAdd = newItems.Except(existingItems).ToList();

        foreach (var item in itemsToRemove)
        {
            existingItems.Remove(item);
        }

        foreach (var item in itemsToAdd)
        {
            existingItems.Add(item);
        }
    }
}