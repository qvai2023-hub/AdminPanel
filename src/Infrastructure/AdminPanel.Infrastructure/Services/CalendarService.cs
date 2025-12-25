using AdminPanel.Application.Common.Interfaces;
using AdminPanel.Application.Features.Calendar.DTOs;
using System.Globalization;

namespace AdminPanel.Infrastructure.Services;

/// <summary>
/// Calendar service implementation using UmAlQuraCalendar for Hijri date support
/// </summary>
public class CalendarService : ICalendarService
{
    private readonly UmAlQuraCalendar _hijriCalendar;
    private readonly CultureInfo _arabicCulture;

    // Hijri month names in Arabic
    private static readonly string[] HijriMonthNamesArabic =
    {
        "", // 0 index placeholder
        "محرم",
        "صفر",
        "ربيع الأول",
        "ربيع الثاني",
        "جمادى الأولى",
        "جمادى الآخرة",
        "رجب",
        "شعبان",
        "رمضان",
        "شوال",
        "ذو القعدة",
        "ذو الحجة"
    };

    // Hijri month names in English
    private static readonly string[] HijriMonthNamesEnglish =
    {
        "", // 0 index placeholder
        "Muharram",
        "Safar",
        "Rabi' al-Awwal",
        "Rabi' al-Thani",
        "Jumada al-Ula",
        "Jumada al-Thani",
        "Rajab",
        "Sha'ban",
        "Ramadan",
        "Shawwal",
        "Dhu al-Qi'dah",
        "Dhu al-Hijjah"
    };

    // Day names in Arabic
    private static readonly Dictionary<DayOfWeek, string> DayNamesArabic = new()
    {
        { DayOfWeek.Sunday, "الأحد" },
        { DayOfWeek.Monday, "الاثنين" },
        { DayOfWeek.Tuesday, "الثلاثاء" },
        { DayOfWeek.Wednesday, "الأربعاء" },
        { DayOfWeek.Thursday, "الخميس" },
        { DayOfWeek.Friday, "الجمعة" },
        { DayOfWeek.Saturday, "السبت" }
    };

    // Day names in English
    private static readonly Dictionary<DayOfWeek, string> DayNamesEnglish = new()
    {
        { DayOfWeek.Sunday, "Sunday" },
        { DayOfWeek.Monday, "Monday" },
        { DayOfWeek.Tuesday, "Tuesday" },
        { DayOfWeek.Wednesday, "Wednesday" },
        { DayOfWeek.Thursday, "Thursday" },
        { DayOfWeek.Friday, "Friday" },
        { DayOfWeek.Saturday, "Saturday" }
    };

    public CalendarService()
    {
        _hijriCalendar = new UmAlQuraCalendar();
        _arabicCulture = new CultureInfo("ar-SA")
        {
            DateTimeFormat = { Calendar = _hijriCalendar }
        };
    }

    /// <summary>
    /// Convert Gregorian date to Hijri date
    /// </summary>
    public HijriDateDto ConvertToHijri(DateTime gregorianDate)
    {
        var hijriYear = _hijriCalendar.GetYear(gregorianDate);
        var hijriMonth = _hijriCalendar.GetMonth(gregorianDate);
        var hijriDay = _hijriCalendar.GetDayOfMonth(gregorianDate);
        var dayOfWeek = _hijriCalendar.GetDayOfWeek(gregorianDate);

        return new HijriDateDto
        {
            Year = hijriYear,
            Month = hijriMonth,
            Day = hijriDay,
            DayOfWeek = (int)dayOfWeek,
            MonthName = GetHijriMonthName(hijriMonth, false),
            MonthNameArabic = GetHijriMonthName(hijriMonth, true),
            DayName = GetHijriDayName(dayOfWeek, false),
            DayNameArabic = GetHijriDayName(dayOfWeek, true),
            FormattedDate = $"{hijriDay} {GetHijriMonthName(hijriMonth, false)} {hijriYear}",
            FormattedDateArabic = $"{hijriDay} {GetHijriMonthName(hijriMonth, true)} {hijriYear} هـ"
        };
    }

    /// <summary>
    /// Convert Hijri date to Gregorian date
    /// </summary>
    public DateTime ConvertToGregorian(int hijriYear, int hijriMonth, int hijriDay)
    {
        return _hijriCalendar.ToDateTime(hijriYear, hijriMonth, hijriDay, 0, 0, 0, 0);
    }

    /// <summary>
    /// Get dual date (Gregorian + Hijri)
    /// </summary>
    public DualDateDto GetDualDate(DateTime gregorianDate)
    {
        return new DualDateDto
        {
            GregorianDate = gregorianDate,
            HijriDate = ConvertToHijri(gregorianDate),
            GregorianFormatted = gregorianDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture)
        };
    }

    /// <summary>
    /// Get calendar month data (Gregorian-based)
    /// </summary>
    public CalendarMonthDto GetMonthData(int year, int month)
    {
        var firstDayOfMonth = new DateTime(year, month, 1);
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var hijriFirstDay = ConvertToHijri(firstDayOfMonth);

        var result = new CalendarMonthDto
        {
            GregorianYear = year,
            GregorianMonth = month,
            GregorianMonthName = firstDayOfMonth.ToString("MMMM", CultureInfo.InvariantCulture),
            HijriYear = hijriFirstDay.Year,
            HijriMonth = hijriFirstDay.Month,
            HijriMonthName = GetHijriMonthName(hijriFirstDay.Month, false),
            HijriMonthNameArabic = GetHijriMonthName(hijriFirstDay.Month, true),
            Days = new List<CalendarDayDto>()
        };

        // Get today for comparison
        var today = DateTime.Today;

        // Calculate days to show from previous month
        var firstDayDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
        // Adjust for Saturday start (Arabic week starts on Saturday)
        var adjustedFirstDay = (firstDayDayOfWeek + 1) % 7;

        // Add days from previous month
        if (adjustedFirstDay > 0)
        {
            var prevMonth = firstDayOfMonth.AddMonths(-1);
            var prevMonthDays = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
            for (int i = adjustedFirstDay - 1; i >= 0; i--)
            {
                var date = new DateTime(prevMonth.Year, prevMonth.Month, prevMonthDays - i);
                result.Days.Add(CreateCalendarDay(date, today, false));
            }
        }

        // Add days of current month
        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(year, month, day);
            result.Days.Add(CreateCalendarDay(date, today, true));
        }

        // Add days from next month to complete the grid (6 rows x 7 days = 42)
        var remainingDays = 42 - result.Days.Count;
        var nextMonth = firstDayOfMonth.AddMonths(1);
        for (int day = 1; day <= remainingDays; day++)
        {
            var date = new DateTime(nextMonth.Year, nextMonth.Month, day);
            result.Days.Add(CreateCalendarDay(date, today, false));
        }

        return result;
    }

    /// <summary>
    /// Get calendar month data (Hijri-based)
    /// </summary>
    public CalendarMonthDto GetHijriMonthData(int hijriYear, int hijriMonth)
    {
        var firstDayGregorian = ConvertToGregorian(hijriYear, hijriMonth, 1);
        var daysInHijriMonth = GetDaysInHijriMonth(hijriYear, hijriMonth);

        var result = new CalendarMonthDto
        {
            GregorianYear = firstDayGregorian.Year,
            GregorianMonth = firstDayGregorian.Month,
            GregorianMonthName = firstDayGregorian.ToString("MMMM", CultureInfo.InvariantCulture),
            HijriYear = hijriYear,
            HijriMonth = hijriMonth,
            HijriMonthName = GetHijriMonthName(hijriMonth, false),
            HijriMonthNameArabic = GetHijriMonthName(hijriMonth, true),
            Days = new List<CalendarDayDto>()
        };

        var today = DateTime.Today;

        // Calculate days to show from previous month
        var firstDayDayOfWeek = (int)firstDayGregorian.DayOfWeek;
        var adjustedFirstDay = (firstDayDayOfWeek + 1) % 7;

        // Add days from previous Hijri month
        if (adjustedFirstDay > 0)
        {
            int prevHijriMonth = hijriMonth == 1 ? 12 : hijriMonth - 1;
            int prevHijriYear = hijriMonth == 1 ? hijriYear - 1 : hijriYear;
            int prevMonthDays = GetDaysInHijriMonth(prevHijriYear, prevHijriMonth);

            for (int i = adjustedFirstDay - 1; i >= 0; i--)
            {
                var date = ConvertToGregorian(prevHijriYear, prevHijriMonth, prevMonthDays - i);
                result.Days.Add(CreateCalendarDay(date, today, false));
            }
        }

        // Add days of current Hijri month
        for (int day = 1; day <= daysInHijriMonth; day++)
        {
            var date = ConvertToGregorian(hijriYear, hijriMonth, day);
            result.Days.Add(CreateCalendarDay(date, today, true));
        }

        // Add days from next Hijri month to complete the grid
        var remainingDays = 42 - result.Days.Count;
        int nextHijriMonth = hijriMonth == 12 ? 1 : hijriMonth + 1;
        int nextHijriYear = hijriMonth == 12 ? hijriYear + 1 : hijriYear;

        for (int day = 1; day <= remainingDays; day++)
        {
            try
            {
                var date = ConvertToGregorian(nextHijriYear, nextHijriMonth, day);
                result.Days.Add(CreateCalendarDay(date, today, false));
            }
            catch
            {
                break; // Stop if we go beyond valid dates
            }
        }

        return result;
    }

    /// <summary>
    /// Get Hijri month name
    /// </summary>
    public string GetHijriMonthName(int month, bool arabic = true)
    {
        if (month < 1 || month > 12)
            return string.Empty;

        return arabic ? HijriMonthNamesArabic[month] : HijriMonthNamesEnglish[month];
    }

    /// <summary>
    /// Get Hijri day name
    /// </summary>
    public string GetHijriDayName(DayOfWeek dayOfWeek, bool arabic = true)
    {
        return arabic ? DayNamesArabic[dayOfWeek] : DayNamesEnglish[dayOfWeek];
    }

    /// <summary>
    /// Get date range with dual dates
    /// </summary>
    public List<DualDateDto> GetDateRange(DateTime startDate, DateTime endDate)
    {
        var dates = new List<DualDateDto>();
        var currentDate = startDate.Date;

        while (currentDate <= endDate.Date)
        {
            dates.Add(GetDualDate(currentDate));
            currentDate = currentDate.AddDays(1);
        }

        return dates;
    }

    /// <summary>
    /// Get today's dual date
    /// </summary>
    public DualDateDto GetToday()
    {
        return GetDualDate(DateTime.Today);
    }

    /// <summary>
    /// Validate Hijri date
    /// </summary>
    public bool IsValidHijriDate(int year, int month, int day)
    {
        try
        {
            // UmAlQuraCalendar supports years 1318-1500
            if (year < 1318 || year > 1500)
                return false;

            if (month < 1 || month > 12)
                return false;

            var daysInMonth = GetDaysInHijriMonth(year, month);
            if (day < 1 || day > daysInMonth)
                return false;

            // Try to convert to verify it's valid
            ConvertToGregorian(year, month, day);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get number of days in a Hijri month
    /// </summary>
    public int GetDaysInHijriMonth(int year, int month)
    {
        return _hijriCalendar.GetDaysInMonth(year, month);
    }

    /// <summary>
    /// Create a calendar day DTO
    /// </summary>
    private CalendarDayDto CreateCalendarDay(DateTime date, DateTime today, bool isCurrentMonth)
    {
        var hijri = ConvertToHijri(date);
        var dayOfWeek = date.DayOfWeek;

        return new CalendarDayDto
        {
            GregorianDate = date,
            GregorianDay = date.Day,
            HijriDay = hijri.Day,
            HijriMonth = hijri.Month,
            HijriYear = hijri.Year,
            IsToday = date.Date == today.Date,
            IsCurrentMonth = isCurrentMonth,
            IsWeekend = dayOfWeek == DayOfWeek.Friday || dayOfWeek == DayOfWeek.Saturday,
            Events = new List<CalendarEventDto>()
        };
    }
}
