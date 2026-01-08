using System.Globalization;

namespace Household.Shared.Helpers;

public class Common
{

    //var sunday = DateTime.Now.AddDays(-((int)DateTime.Now.DayOfWeek + 7) % 7).Date;
    //string firstDayOfWeek = string.Format("{0:yyyy-MM-dd}", start);
    //string lastDayOfWeek = string.Format("{0:yyyy-MM-dd}", end);

    /// <summary>
    /// Used to determine what episode to watch.
    /// </summary>
    /// <param name="episodes">Total number of epidsodes in the series</param>
    /// <param name="startDate">The start date of the series</param>
    /// <param name="isCompleted">Is the season over</param>
    /// <param name="isOver">Has the series finale aired</param>
    /// <returns></returns>
    //    public static string GetNextEpisode(int episodes, DateTime startDate, bool isCompleted, bool isOver)
    public static int GetNextEpisode(int episodes, DateTime startDate, bool isCompleted, bool isOver)
    {
        //if (isOver || isCompleted == false || episodes == 0)
        //    return string.Empty;

        //DateTime currentDate = DateTime.Now.Date;
        //int daysToAdd = episodes * 7 - 7;
        //DateTime lastDate = startDate.AddDays(daysToAdd);

        //if (lastDate.CompareTo(startDate) < 0)
        //    return string.Empty;

        //if (lastDate.CompareTo(startDate) == 0)
        //    return "Last episode!";

        //if (lastDate.CompareTo(startDate) > 0)
        //{
        //    int totalWeeks = (int)Math.Ceiling((lastDate - currentDate).TotalDays / 7);

        //    if (totalWeeks > 0)
        //        return $"Episode: {episodes - totalWeeks + 1}";
        //    else
        //        return "Last episode!";
        //}

        //return string.Empty;

        if (isOver || isCompleted == false || episodes == 0)
            return -1;

        DateTime currentDate = DateTime.Now.Date;
        int daysToAdd = episodes * 7 - 7;
        DateTime lastDate = startDate.AddDays(daysToAdd);

        if (lastDate.CompareTo(startDate) < 0)
            return -1;

        if (lastDate.CompareTo(startDate) == 0)
            return 0;

        if (lastDate.CompareTo(startDate) > 0)
        {
            int totalWeeks = (int)Math.Ceiling((lastDate - currentDate).TotalDays / 7);

            if (totalWeeks > 0)
                return episodes - (totalWeeks + 1);
            else
                return 0;
        }

        return -1;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ConvertBoolToYesNo(bool? value)
    {
        if (value == null)
            return "No";
        else if (value == true)
            return "Yes";
        else
            return "No";
    }

    /// <summary>
    /// Used to adjust the time to better align the number of episodes.
    /// </summary>
    /// <param name="date">Original datetime</param>
    /// <param name="startDate">New start datetime</param>
    /// <returns></returns>
    public static DateTime GetLowerDateWithNewTime(string date, string startDate)
    {
        // Parse input strings to DateTime objects
        DateTime date1 = DateTime.Parse(date);
        DateTime date2 = DateTime.Parse(startDate);

        // Determine the lower date
        DateTime lowerDate = date1 < date2 ? date1 : date2;

        // Create a new DateTime with the lower date and a specific time (e.g., 12:00 PM)
        DateTime result = new(lowerDate.Year, lowerDate.Month, lowerDate.Day, date1.Hour, date1.Minute, date1.Second);

        return result;
    }

    /// <summary>
    /// Used to consistantly format the date portion MM/dd/yyyy
    /// </summary>
    /// <param name="date"></param>
    /// <returns>Formated date or empty string</returns>
    public static string FormatDateTime(string? date)
    {
        if (date.IsNullOrWhiteSpace())
            return string.Empty;

        bool isConverted = DateTime.TryParse(date, out DateTime dateTime);

        if (isConverted)
        {
            return dateTime.ToShortDateString();
        }

        return string.Empty;
    }

    /// <summary>
    /// Used to format a phone number string for display. (XXX) XXX-XXXX
    /// </summary>
    /// <param name="phoneNumber"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">String must be a length of 10</exception>
    public static string FormatPhoneNumber(string phoneNumber)
    {
        // Check if the input string length is valid
        if (phoneNumber.Length != 10)
        {
            throw new ArgumentException("Phone number must be a 10-digit string.");
        }

        // Extracting parts of the phone number
        ReadOnlySpan<char> areaCode = phoneNumber.AsSpan(0, 3);
        ReadOnlySpan<char> firstPart = phoneNumber.AsSpan(3, 3);
        ReadOnlySpan<char> secondPart = phoneNumber.AsSpan(6, 4);

        // Formatting the phone number
        string formattedNumber = $"({areaCode}) {firstPart}-{secondPart}";

        return formattedNumber;
    }

    /// <summary>
    /// Determine the week in the year
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static int GetIso8601WeekOfYear(DateTime time)
    {
        DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
        if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
        {
            time = time.AddDays(3);
        }

        return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Sunday);
    }

    /// <summary>
    /// Dtermine the first day of the week for the supplied week and year
    /// </summary>
    /// <param name="year"></param>
    /// <param name="weekOfYear"></param>
    /// <param name="ci"></param>
    /// <returns></returns>
    public static DateTime FirstDateOfWeek(int year, int weekOfYear, System.Globalization.CultureInfo ci)
    {
        DateTime jan1 = new(year, 1, 1);
        int daysOffset = (int)ci.DateTimeFormat.FirstDayOfWeek - (int)jan1.DayOfWeek;
        DateTime firstWeekDay = jan1.AddDays(daysOffset);
        int firstWeek = ci.Calendar.GetWeekOfYear(jan1, ci.DateTimeFormat.CalendarWeekRule, ci.DateTimeFormat.FirstDayOfWeek);
        if ((firstWeek <= 1 || firstWeek >= 52) && daysOffset >= -3)
        {
            weekOfYear -= 1;
        }
        return firstWeekDay.AddDays(weekOfYear * 7);
    }

    public static class DateValidation
    {
        private const int MinSqlYear = 1753;
        private const int MaxSqlYear = 9999;

        public static bool IsValidSqlDate(int year, int month, int day)
        {
            if (year < MinSqlYear || year > MaxSqlYear)
                return false;

            if (month < 1 || month > 12)
                return false;

            int maxDay = DateTime.DaysInMonth(year, month);
            return day >= 1 && day <= maxDay;
        }

        public static bool TryCreateSqlDate(int year, int month, int day, out DateTime result)
        {
            if (IsValidSqlDate(year, month, day))
            {
                result = new DateTime(year, month, day);
                return true;
            }

            result = default;
            return false;
        }
    }
}
