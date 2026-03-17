using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using NewSideGame;

public enum TimeFormatType
{
    DD_HH_MM_SS,//01:23:59:59
    DD_HH_MM,//01:23:59
    HH_MM_SS,//23:59:59
    MM_SS,//10:59
    HH_MM,//10:59
    DD_HH,
    DD,
    MM,
    HH,
    SS,
}

public class TimeUtils
{
    /// <summary>
    /// 时间格式转换
    /// </summary>
    /// <param name="totalSeconds"> 倒计时 ：s </param>
    /// <param name="formatType"> 需要的时间格式 </param>
    /// <param name="placeholder"> 格式占位符 </param>
    /// <param name="fillzero"> 单个数字 小于0 是否需要补充0 </param>
    /// <returns></returns>
    public static string TimeFormat(int totalSeconds, TimeFormatType formatType, string placeholder, bool fillzero = true, bool rounding = true)
    {
        if (rounding)
        {
            //- 所有的倒计时，24h统一写23h，0h统一写1h
            if (totalSeconds > 0 && totalSeconds < 3600)
            {
                totalSeconds = 3600;
            }
            else if (totalSeconds % 86400 == 0)
            {
                totalSeconds -= 1;
            }
        }

        if (totalSeconds < 0) totalSeconds = 0;


        int days = (totalSeconds / 3600) / 24;
        int hours = (totalSeconds / 3600) - (days * 24);
        int minutes = (totalSeconds - (hours * 3600) - (days * 86400)) / 60;
        int seconds = totalSeconds - (hours * 3600) - (minutes * 60) - (days * 86400);

        switch (formatType)
        {
            case TimeFormatType.DD_HH_MM_SS:
                return string.Format(placeholder,
                    fillzero && days < 10 ? "0" + days : days.ToString(),
                    fillzero && hours < 10 ? "0" + hours : hours.ToString(),
                    fillzero && minutes < 10 ? "0" + minutes : minutes.ToString(),
                    fillzero && seconds < 10 ? "0" + seconds : seconds.ToString());

            case TimeFormatType.DD_HH_MM:
                return string.Format(placeholder,
                    fillzero && days < 10 ? "0" + days : days.ToString(),
                    fillzero && hours < 10 ? "0" + hours : hours.ToString(),
                    fillzero && minutes < 10 ? "0" + minutes : minutes.ToString());

            case TimeFormatType.HH_MM_SS:
                hours = days * 24 + hours;
                return string.Format(placeholder,
                    fillzero && hours < 10 ? "0" + hours : hours.ToString(),
                    fillzero && minutes < 10 ? "0" + minutes : minutes.ToString(),
                    fillzero && seconds < 10 ? "0" + seconds : seconds.ToString());
            case TimeFormatType.HH_MM:
                hours = days * 24 + hours;
                return string.Format(placeholder,
                    fillzero && hours < 10 ? "0" + hours : hours.ToString(),
                    fillzero && minutes < 10 ? "0" + minutes : minutes.ToString());

            case TimeFormatType.MM_SS:
                minutes = (days * 24 + hours) * 60 + minutes;
                return string.Format(placeholder,
                    fillzero && minutes < 10 ? "0" + minutes : minutes.ToString(),
                    fillzero && seconds < 10 ? "0" + seconds : seconds.ToString());

            case TimeFormatType.DD_HH:
                //四舍五入
                // int addHour = (minutes > 30 && rounding) ? 1 : 0;
                // hours += addHour;
                return string.Format(placeholder,
                    fillzero && days < 10 ? "0" + days : days.ToString(),
                    fillzero && hours < 10 ? "0" + hours : hours.ToString());

            case TimeFormatType.DD:
                return string.Format(placeholder,
                    fillzero && days < 10 ? "0" + days : days.ToString());

            case TimeFormatType.MM:
                minutes = (days * 24 + hours) * 60 + minutes;
                return string.Format(placeholder,
                    fillzero && minutes < 10 ? "0" + minutes : minutes.ToString());

            case TimeFormatType.SS:
                return string.Format(placeholder,
                    fillzero && totalSeconds < 10 ? "0" + totalSeconds : totalSeconds.ToString());

            case TimeFormatType.HH:
                hours = (days * 24 + hours);
                int _addHour = minutes > 30 ? 1 : 0;
                hours += _addHour;
                return string.Format(placeholder, hours);

            default:
                break;
        }

        return "format error";
    }

    public static string TimeFormatHHMMSS(int totalSeconds, string placeholder, bool fillzero = true)
    {
        if (totalSeconds < 0) totalSeconds = 0;
        int days = (totalSeconds / 3600) / 24;
        int hours = (totalSeconds / 3600) - (days * 24);
        int minutes = (totalSeconds - (hours * 3600) - (days * 86400)) / 60;
        int seconds = totalSeconds - (hours * 3600) - (minutes * 60) - (days * 86400);

        hours = days * 24 + hours;
        return string.Format(placeholder,
            fillzero && hours < 10 ? "0" + hours : hours.ToString(),
            fillzero && minutes < 10 ? "0" + minutes : minutes.ToString(),
            fillzero && seconds < 10 ? "0" + seconds : seconds.ToString());
    }

    public static string TimeFormatMMss(int totalSeconds)
    {
        string placeholder = string.Empty;
        TimeFormatType timeFormatType;
        placeholder = "{0}:{1}";
        timeFormatType = TimeFormatType.MM_SS;
        return TimeUtils.TimeFormat(totalSeconds, timeFormatType, placeholder, true, false);
    }

    public static string TimeFormatMerge(int totalSeconds)
    {
        string placeholder = string.Empty;
        TimeFormatType timeFormatType;
        if (totalSeconds > 3600)
        {
            if (totalSeconds % 3600 != 0)
            {
                placeholder = "{0}时{1}分";
                timeFormatType = TimeFormatType.HH_MM;
            }
            else
            {
                placeholder = "{0}时";
                timeFormatType = TimeFormatType.HH;
            }
        }
        else
        {
            if (totalSeconds % 60 != 0)
            {
                placeholder = "{0}分{1}秒";
                timeFormatType = TimeFormatType.MM_SS;
            }
            else
            {
                placeholder = "{0}分";
                timeFormatType = TimeFormatType.MM;
            }
        }
        return TimeUtils.TimeFormat(totalSeconds, timeFormatType, placeholder, true, false);
    }

    public static string TimeFormatMerge2(int totalSeconds)
    {
        string placeholder = string.Empty;
        TimeFormatType timeFormatType;
        if (totalSeconds > 3600)
        {
            if (totalSeconds % 3600 != 0)
            {
                placeholder = "{0}时{1}分";
                timeFormatType = TimeFormatType.HH_MM;
            }
            else
            {
                placeholder = "{0}时";
                timeFormatType = TimeFormatType.HH;
            }
        }
        else
        {
            if (totalSeconds % 60 != 0)
            {
                placeholder = "{0}分{1}秒";
                timeFormatType = TimeFormatType.MM_SS;
            }
            else
            {
                placeholder = "{0}分";
                timeFormatType = TimeFormatType.MM;
            }
        }
        return TimeUtils.TimeFormat(totalSeconds, timeFormatType, placeholder, true, false);
    }


    public static string TimeFormatSpecialBox(int totalSeconds)
    {
        string placeholder = string.Empty;
        TimeFormatType timeFormatType;
        if (totalSeconds > 60)
        {
            placeholder = "{0}分";
            timeFormatType = TimeFormatType.MM;
        }
        else
        {
            placeholder = "{0}秒";
            timeFormatType = TimeFormatType.SS;
        }
        return TimeUtils.TimeFormat(totalSeconds, timeFormatType, placeholder, false, false);
    }

    public static string GetFormatDateTime(long totalSeconds)
    {
        return GetFormatDateTime(GetDateTime(totalSeconds));
    }

    public static string GetFormatDateTime(DateTime dateTime)
    {
        return string.Format("{0:D4}.{1:D2}.{2:D2}",
            dateTime.Year, dateTime.Month, dateTime.Day);
    }


    /// <summary>
    /// 获取当前时间戳
    /// </summary>
    /// <param name="bflag">为真时获取10位时间戳,为假时获取13位时间戳.</param>
    /// <returns></returns>
    public static long GetTimeStamp(bool bflag = true)
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        long ret;
        if (bflag)
            ret = Convert.ToInt64(ts.TotalSeconds);
        else
            ret = Convert.ToInt64(ts.TotalMilliseconds);
        return ret;
    }

    // 全用NowTimeStamp去调用！！
    public static int GetTimeStampOfSeconds()
    {
        return (int)NowTimeStamp;
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt32(ts.TotalSeconds);
    }

    public static long GetTimeStampOfMilliseconds()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalMilliseconds);
    }

    public static int GetTimeStampOfSeconds(DateTime dateTime)
    {
        TimeSpan ts = dateTime - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt32(ts.TotalSeconds);
    }

    public static long GetTimeStampByDataTime(DateTime dataTime)
    {
        System.DateTime startTime = new System.DateTime(1970, 1, 1);
        startTime = startTime.ToLocalTime();
        TimeSpan timeSpan = dataTime - startTime;

        return Convert.ToInt64(timeSpan.TotalSeconds);
    }

    public static double GetTimeStamp_Double()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return ts.TotalSeconds;
    }

    public static DateTime GetToday()
    {
        return GetDateTime(GetTimeStampOfSeconds());
    }

    public static DateTime GetDateTime(long totalSeconds)
    {
        DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        DateTime dt = startTime.AddSeconds(totalSeconds);
        return dt;
    }

    public static DateTime GetNextDayDateTime(long totalSeconds, int x)
    {
        DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        DateTime dt = startTime.AddSeconds(totalSeconds);

        DateTime day = new DateTime(dt.Year, dt.Month, dt.Day);
        day = dt.AddSeconds(24 * 3600 * x);
        return day;
    }


    public static DateTime GetDateTimeWithTS(long ts)
    {
        DateTime startTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), TimeZoneInfo.Local);
        return startTime.AddSeconds(ts);
    }

    public static bool IsToday(DateTime dateTime)
    {
        return IsSameDayUseServerTime(dateTime);
        // DateTime today = DateTime.Now;
        // if (today.Year == dateTime.Year
        //     && today.Month == dateTime.Month
        //     && today.Day == dateTime.Day)
        // {
        //     return true;
        // }
        // else
        // {
        //     return false;
        // }
    }

    public static bool IsSameDay(DateTime date1, DateTime date2)
    {
        if (date1.Year == date2.Year
            && date1.Month == date2.Month
            && date1.Day == date2.Day)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 返回当前时间的秒值
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static long ConvertDateTimeLong(System.DateTime time)
    {
        System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
        return (long)(time - startTime).TotalMilliseconds / 1000;
    }

    /// <summary>
    /// 当前系统时间戳：单位：s
    /// </summary>
    public static long NowTimeStamp => GameManager.Instance.GetTimeStampOfSeconds();


    public static int DateDiff(long startTS, long endTS)
    {
        DateTime startDate = Convert.ToDateTime(GetDateTime(startTS).ToShortDateString());
        DateTime endDate = Convert.ToDateTime(GetDateTime(endTS).ToShortDateString());
        TimeSpan sp = endDate.Subtract(startDate);
        return sp.Days;
    }

    public static string CalculateTimeDifference(long timestamp1, long timestamp2)
    {
        TimeSpan difference = TimeSpan.FromSeconds(timestamp2) - TimeSpan.FromSeconds(timestamp1);

        int days = difference.Days;
        int hours = difference.Hours;
        int minutes = difference.Minutes;

        return $"{days}d{hours}h{minutes}m";
    }

    public static bool IsSameDay(DateTime dateTime)
    {
        return IsSameDayUseServerTime(dateTime);

        // DateTime today = DateTime.Now;
        // if (today.Year == dateTime.Year
        //     && today.Month == dateTime.Month
        //     && today.Day == dateTime.Day)
        // {
        //     return true;
        // }
        // else
        // {
        //     return false;
        // }
    }

    public static bool IsSameDayUseServerTime(DateTime dateTime, bool IsNextNDay = false)
    {
        DateTime today = GetDateTime(GetTimeStampOfSeconds());
        // DateTime today = DateTime.Now;
        if (today.Year == dateTime.Year
            && today.Month == dateTime.Month
            && today.Day == dateTime.Day)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // public static bool IslessDayUseServerTime(DateTime dateTime, int d)
    // {
    //     DateTime today = GetDateTime(GetTimeStampOfSeconds());
    //     dateTime = dateTime.AddSeconds(24 * 3600 * d);

    //     return dateTime > today;
    // }

    public static bool IsOver7Days(DateTime start, DateTime end)
    {
        DateTime targetEnd = start.AddDays(7);
        return DateTime.Compare(end, targetEnd) > 0;
    }

    // public static DateTime GetNextDay()
    // {
    //     DateTime day = DateTime.Now;
    //     day = new DateTime(day.Year, day.Month, day.Day);
    //     day = day.AddSeconds(24 * 3600);
    //     return day;
    // }

    public static DateTime GetNextDay(DateTime now, int day = 1)
    {
        now = new DateTime(now.Year, now.Month, now.Day);
        now = now.AddSeconds(24 * 3600 * day);
        return now;
    }

    //根据指定时间间隔，获取当前时间之后的时间点
    public static DateTime GetNextTargetDateTime(int minutes)
    {
        DateTime now = GetDateTime(GetTimeStampOfSeconds());
        DateTime day = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
        while (DateTime.Compare(now, day) > 0)
        {
            day = day.AddMinutes(minutes);
        }
        return day;
    }

    //是否为传入时间下个自然日 以及以后的日子
    public static bool IsNextNDay(int timestamp)
    {
        DateTime theDate = GetDateTime(timestamp);
        if (IsSameDayUseServerTime(theDate))
            return false;
        return true;
    }

    public static DateTime GetStartOfMondy()
    {
        DateTime today = DateTime.Today;
        int dayOfWeek = (int)today.DayOfWeek == 0 ? 7 : (int)today.DayOfWeek;
        DateTime startOfWeek = today.AddDays(1 - dayOfWeek);
        DateTime startOfMonday = new DateTime(startOfWeek.Year, startOfWeek.Month, startOfWeek.Day, 0, 0, 0, DateTimeKind.Local);

        return startOfMonday;
    }

    public static int GetSubtractDay(int time)
    {
        DateTime now = DateTime.Today;
        DateTime d = GetDateTime(time);
        d = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0);
        TimeSpan t = now.Subtract(d);
        return Mathf.Abs(t.Days);
    }

    public static string Seconds_To_DHMS(long totalTime)
    {
        if (totalTime < 0) totalTime = 0;
        string date = string.Empty;
        int seconds = (int)(totalTime % 60);
        int sec = (int)(totalTime - seconds) / 60;
        int minutes = sec % 60;
        sec = (sec - minutes) / 60;
        int hours = sec % 24;
        int day = (int)(sec - hours) / 24;
        if (seconds < 0) seconds = 0;
        if (day > 0)
            date = string.Format("{0}D {1}H", day, hours);
        else if (hours > 0)
            date = string.Format("{0}H {1}M", hours, minutes);
        else
            date = string.Format("{0}M {1}S", minutes, seconds);
        return date;
    }

    /// <summary>
    /// 获取传入天整点的时间戳
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static int GetTimeStampStartOfDay(DateTime dateTime)
    {
        return Convert.ToInt32(GetTimeStampByDataTime(new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0)));
    }
}
