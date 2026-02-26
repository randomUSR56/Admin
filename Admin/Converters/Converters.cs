using System.Globalization;

namespace Admin.Converters;

public class InvertBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return !b;
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return !b;
        return value;
    }
}

public class BusyTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isBusy && parameter is string texts)
        {
            var parts = texts.Split('|');
            if (parts.Length == 2)
                return isBusy ? parts[1] : parts[0];
        }
        return value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class StatusColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isOnline)
            return isOnline ? Colors.LimeGreen : Colors.Red;
        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class StatusTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isOnline)
            return isOnline ? "Server Online" : "Server Offline";
        return "Unknown";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isActive)
            return isActive ? Colors.SeaGreen : Colors.Gray;
        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class CategoryColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string category)
        {
            return category.ToLowerInvariant() switch
            {
                "engine" => Colors.OrangeRed,
                "transmission" => Colors.MediumPurple,
                "electrical" => Colors.Gold,
                "brakes" => Colors.Crimson,
                "suspension" => Colors.DodgerBlue,
                "steering" => Colors.Teal,
                "body" => Colors.SteelBlue,
                "other" => Colors.DarkGray,
                _ => Colors.LightGray
            };
        }
        return Colors.LightGray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class TicketStatusColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status.ToLowerInvariant() switch
            {
                "open" => Colors.DodgerBlue,
                "assigned" => Colors.Orange,
                "in_progress" => Colors.MediumPurple,
                "completed" => Colors.SeaGreen,
                "closed" => Colors.Gray,
                _ => Colors.LightGray
            };
        }
        return Colors.LightGray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class TicketPriorityColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string priority)
        {
            return priority.ToLowerInvariant() switch
            {
                "urgent" => Colors.Red,
                "high" => Colors.OrangeRed,
                "medium" => Colors.Gold,
                "low" => Colors.SteelBlue,
                _ => Colors.LightGray
            };
        }
        return Colors.LightGray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}