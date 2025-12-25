using System.Globalization;

namespace AdminPanel.Application.Features.Calendar.DTOs;

/// <summary>
/// Represents a calendar event
/// </summary>
public class CalendarEventDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Start { get; set; }
    public DateTime? End { get; set; }
    public bool AllDay { get; set; }
    public string? Color { get; set; }
    public string? TextColor { get; set; }
    public string? Url { get; set; }
    public bool Editable { get; set; } = true;
    public Dictionary<string, object>? ExtendedProps { get; set; }
}

/// <summary>
/// DTO for creating a calendar event
/// </summary>
public class CreateCalendarEventDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Start { get; set; }
    public DateTime? End { get; set; }
    public bool AllDay { get; set; }
    public string? Color { get; set; }
}

/// <summary>
/// DTO for updating a calendar event
/// </summary>
public class UpdateCalendarEventDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
    public bool? AllDay { get; set; }
    public string? Color { get; set; }
}

/// <summary>
/// Hijri date information
/// </summary>
public class HijriDateDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
    public int DayOfWeek { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public string MonthNameArabic { get; set; } = string.Empty;
    public string DayName { get; set; } = string.Empty;
    public string DayNameArabic { get; set; } = string.Empty;
    public string FormattedDate { get; set; } = string.Empty;
    public string FormattedDateArabic { get; set; } = string.Empty;
}

/// <summary>
/// Combined Gregorian and Hijri date
/// </summary>
public class DualDateDto
{
    public DateTime GregorianDate { get; set; }
    public HijriDateDto HijriDate { get; set; } = new();
    public string GregorianFormatted { get; set; } = string.Empty;
}

/// <summary>
/// Date conversion request
/// </summary>
public class DateConversionRequestDto
{
    public DateTime? GregorianDate { get; set; }
    public int? HijriYear { get; set; }
    public int? HijriMonth { get; set; }
    public int? HijriDay { get; set; }
}

/// <summary>
/// Calendar view settings
/// </summary>
public class CalendarSettingsDto
{
    public string DefaultView { get; set; } = "dayGridMonth";
    public string Locale { get; set; } = "ar";
    public bool ShowHijri { get; set; } = true;
    public bool ShowGregorian { get; set; } = true;
    public int FirstDayOfWeek { get; set; } = 6; // Saturday for Arabic
    public string Direction { get; set; } = "rtl";
}

/// <summary>
/// Month information for calendar
/// </summary>
public class CalendarMonthDto
{
    public int GregorianYear { get; set; }
    public int GregorianMonth { get; set; }
    public string GregorianMonthName { get; set; } = string.Empty;
    public int HijriYear { get; set; }
    public int HijriMonth { get; set; }
    public string HijriMonthName { get; set; } = string.Empty;
    public string HijriMonthNameArabic { get; set; } = string.Empty;
    public List<CalendarDayDto> Days { get; set; } = new();
}

/// <summary>
/// Day information for calendar
/// </summary>
public class CalendarDayDto
{
    public DateTime GregorianDate { get; set; }
    public int GregorianDay { get; set; }
    public int HijriDay { get; set; }
    public int HijriMonth { get; set; }
    public int HijriYear { get; set; }
    public bool IsToday { get; set; }
    public bool IsCurrentMonth { get; set; }
    public bool IsWeekend { get; set; }
    public List<CalendarEventDto> Events { get; set; } = new();
}
