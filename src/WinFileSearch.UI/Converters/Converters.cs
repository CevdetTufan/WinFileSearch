using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using WinFileSearch.Data.Models;
using Application = System.Windows.Application;
using Visibility = System.Windows.Visibility;

namespace WinFileSearch.UI.Converters;

/// <summary>
/// Converts FileCategory to appropriate icon path
/// </summary>
public class FileCategoryToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not FileCategory category)
            return GetDefaultIcon();

        return category switch
        {
            FileCategory.Document => GetDocumentIcon(),
            FileCategory.Image => GetImageIcon(),
            FileCategory.Media => GetMediaIcon(),
            FileCategory.Archive => GetArchiveIcon(),
            FileCategory.Code => GetCodeIcon(),
            _ => GetDefaultIcon()
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static Geometry GetDocumentIcon() => 
        Geometry.Parse("M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M18,20H6V4H13V9H18V20Z");

    private static Geometry GetImageIcon() => 
        Geometry.Parse("M21,3H3C2,3 1,4 1,5V19A2,2 0 0,0 3,21H21C22,21 23,20 23,19V5C23,4 22,3 21,3M5,17L8.5,12.5L11,15.5L14.5,11L19,17H5Z");

    private static Geometry GetMediaIcon() => 
        Geometry.Parse("M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M10,16.5V7.5L16,12L10,16.5Z");

    private static Geometry GetArchiveIcon() => 
        Geometry.Parse("M20,6H12L10,4H4A2,2 0 0,0 2,6V18A2,2 0 0,0 4,20H20A2,2 0 0,0 22,18V8A2,2 0 0,0 20,6M18,12H14V16H18V18H10V16H6V14H10V10H6V8H10V6H14V10H18V12Z");

    private static Geometry GetCodeIcon() => 
        Geometry.Parse("M8,3A2,2 0 0,0 6,5V9A2,2 0 0,1 4,11H3V13H4A2,2 0 0,1 6,15V19A2,2 0 0,0 8,21H10V19H8V14A2,2 0 0,0 6,12A2,2 0 0,0 8,10V5H10V3M16,3A2,2 0 0,1 18,5V9A2,2 0 0,0 20,11H21V13H20A2,2 0 0,0 18,15V19A2,2 0 0,1 16,21H14V19H16V14A2,2 0 0,1 18,12A2,2 0 0,1 16,10V5H14V3H16Z");

    private static Geometry GetDefaultIcon() => 
        Geometry.Parse("M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M18,20H6V4H13V9H18V20Z");
}

/// <summary>
/// Converts FileCategory to appropriate brush color
/// </summary>
public class FileCategoryToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not FileCategory category)
            return Application.Current.Resources["OtherIconBrush"];

        return category switch
        {
            FileCategory.Document => Application.Current.Resources["DocumentIconBrush"],
            FileCategory.Image => Application.Current.Resources["ImageIconBrush"],
            FileCategory.Media => Application.Current.Resources["MediaIconBrush"],
            FileCategory.Archive => Application.Current.Resources["ArchiveIconBrush"],
            FileCategory.Code => Application.Current.Resources["CodeIconBrush"],
            _ => Application.Current.Resources["OtherIconBrush"]
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts file size in bytes to human-readable format
/// </summary>
public class FileSizeConverter : IValueConverter
{
    private static readonly string[] SizeSuffixes = { "B", "KB", "MB", "GB", "TB" };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not long size)
            return "0 B";

        if (size == 0)
            return "0 B";

        int mag = (int)Math.Log(size, 1024);
        mag = Math.Min(mag, SizeSuffixes.Length - 1);
        double adjustedSize = size / Math.Pow(1024, mag);

        return $"{adjustedSize:N1} {SizeSuffixes[mag]}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts DateTime to relative time string
/// </summary>
public class RelativeTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not DateTime dateTime)
            return "";

        var timeSpan = DateTime.Now - dateTime;

        if (timeSpan.TotalMinutes < 1)
            return "Just now";
        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes}m ago";
        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours}h ago";
        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays}d ago";
        if (timeSpan.TotalDays < 30)
            return $"{(int)(timeSpan.TotalDays / 7)}w ago";
        if (timeSpan.TotalDays < 365)
            return dateTime.ToString("MMM d");

        return dateTime.ToString("MMM d, yyyy");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts boolean to visibility
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            bool invert = parameter?.ToString() == "Invert";
            return (boolValue ^ invert) ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Highlights search text in filename
/// </summary>
public class SearchHighlightConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is not string text || values[1] is not string searchText)
            return values[0] ?? "";

        return text; // For simplicity, just return text. Actual highlighting would need a TextBlock with Inlines
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
