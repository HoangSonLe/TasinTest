using Tasin.Website.Common.CommonModels.BaseModels;

namespace Tasin.Website.Common.Helper
{
    public class SolarDate
    {
        public int day;
        public int month;
        public int year;


        public LunarDate ToLunarDate()
        {
            return VietCalendar.convertSolar2Lunar(this, 7.0);
        }

        public DateTime ToDateTime()
        {
            return new DateTime(year, month, day);
        }
    }
    public class LunarDate
    {
        public int day;
        public int month;
        public bool isLeapMonth;
        public int year;

        public SolarDate ToSolarDate()
        {
            return VietCalendar.convertLunar2Solar(this, 7.0);
        }

    }
    /**
  * @author duc
  *
*/
    public class VietCalendar
    {
        public static double PI = Math.PI;
        /**
         *
         * @param dd
         * @param mm
         * @param yy
         * @return the number of days since 1 January 4713 BC (Julian calendar)
         */
        public static int jdFromDate(int dd, int mm, int yy)
        {
            int a = (14 - mm) / 12;
            int y = yy + 4800 - a;
            int m = mm + 12 * a - 3;
            int jd = dd + (153 * m + 2) / 5 + 365 * y + y / 4 - y / 100 + y / 400 - 32045;
            if (jd < 2299161)
            {
                jd = dd + (153 * m + 2) / 5 + 365 * y + y / 4 - 32083;
            }
            //jd = jd - 1721425;
            return jd;
        }
        /**
         * http://www.tondering.dk/claus/calendar.html
         * Section: Is there a formula for calculating the Julian day number?
         * @param jd - the number of days since 1 January 4713 BC (Julian calendar)
         * @return
         */
        public static SolarDate jdToDate(int jd)
        {
            int a, b, c;
            if (jd > 2299160)
            { // After 5/10/1582, Gregorian calendar
                a = jd + 32044;
                b = (4 * a + 3) / 146097;
                c = a - (b * 146097) / 4;
            }
            else
            {
                b = 0;
                c = jd + 32082;
            }
            int d = (4 * c + 3) / 1461;
            int e = c - (1461 * d) / 4;
            int m = (5 * e + 2) / 153;
            int day = e - (153 * m + 2) / 5 + 1;
            int month = m + 3 - 12 * (m / 10);
            int year = b * 100 + d - 4800 + m / 10;
            return new SolarDate()
            {
                year = year,
                month = month,
                day = day,
            };
        }
        /**
         * Solar longitude in degrees
         * Algorithm from: Astronomical Algorithms, by Jean Meeus, 1998
         * @param jdn - number of days since noon UTC on 1 January 4713 BC
         * @return
         */
        public static double SunLongitude(double jdn)
        {
            //return CC2K.sunLongitude(jdn);
            return SunLongitudeAA98(jdn);
        }
        public static double SunLongitudeAA98(double jdn)
        {
            double T = (jdn - 2451545.0) / 36525; // Time in Julian centuries from 2000-01-01 12:00:00 GMT
            double T2 = T * T;
            double dr = PI / 180; // degree to radian
            double M = 357.52910 + 35999.05030 * T - 0.0001559 * T2 - 0.00000048 * T * T2; // mean anomaly, degree
            double L0 = 280.46645 + 36000.76983 * T + 0.0003032 * T2; // mean longitude, degree
            double DL = (1.914600 - 0.004817 * T - 0.000014 * T2) * Math.Sin(dr * M);
            DL = DL + (0.019993 - 0.000101 * T) * Math.Sin(dr * 2 * M) + 0.000290 * Math.Sin(dr * 3 * M);
            double L = L0 + DL; // true longitude, degree
            L = L - 360 * (INT(L / 360)); // Normalize to (0, 360)
            return L;
        }
        public static double NewMoon(int k)
        {
            //return CC2K.newMoonTime(k);
            return NewMoonAA98(k);
        }
        /**
         * Julian day number of the kth new moon after (or before) the New Moon of 1900-01-01 13:51 GMT.
         * Accuracy: 2 minutes
         * Algorithm from: Astronomical Algorithms, by Jean Meeus, 1998
         * @param k
         * @return the Julian date number (number of days since noon UTC on 1 January 4713 BC) of the New Moon
         */

        public static double NewMoonAA98(int k)
        {
            double T = k / 1236.85; // Time in Julian centuries from 1900 January 0.5
            double T2 = T * T;
            double T3 = T2 * T;
            double dr = PI / 180;
            double Jd1 = 2415020.75933 + 29.53058868 * k + 0.0001178 * T2 - 0.000000155 * T3;
            Jd1 = Jd1 + 0.00033 * Math.Sin((166.56 + 132.87 * T - 0.009173 * T2) * dr); // Mean new moon
            double M = 359.2242 + 29.10535608 * k - 0.0000333 * T2 - 0.00000347 * T3; // Sun's mean anomaly
            double Mpr = 306.0253 + 385.81691806 * k + 0.0107306 * T2 + 0.00001236 * T3; // Moon's mean anomaly
            double F = 21.2964 + 390.67050646 * k - 0.0016528 * T2 - 0.00000239 * T3; // Moon's argument of latitude
            double C1 = (0.1734 - 0.000393 * T) * Math.Sin(M * dr) + 0.0021 * Math.Sin(2 * dr * M);
            C1 = C1 - 0.4068 * Math.Sin(Mpr * dr) + 0.0161 * Math.Sin(dr * 2 * Mpr);
            C1 = C1 - 0.0004 * Math.Sin(dr * 3 * Mpr);
            C1 = C1 + 0.0104 * Math.Sin(dr * 2 * F) - 0.0051 * Math.Sin(dr * (M + Mpr));
            C1 = C1 - 0.0074 * Math.Sin(dr * (M - Mpr)) + 0.0004 * Math.Sin(dr * (2 * F + M));
            C1 = C1 - 0.0004 * Math.Sin(dr * (2 * F - M)) - 0.0006 * Math.Sin(dr * (2 * F + Mpr));
            C1 = C1 + 0.0010 * Math.Sin(dr * (2 * F - Mpr)) + 0.0005 * Math.Sin(dr * (2 * Mpr + M));
            double deltat;
            if (T < -11)
            {
                deltat = 0.001 + 0.000839 * T + 0.0002261 * T2 - 0.00000845 * T3 - 0.000000081 * T * T3;
            }
            else
            {
                deltat = -0.000278 + 0.000265 * T + 0.000262 * T2;
            };
            double JdNew = Jd1 + C1 - deltat;
            return JdNew;
        }
        public static int INT(double d)
        {
            return (int)Math.Floor(d);
        }
        public static double getSunLongitude(int dayNumber, double timeZone)
        {
            return SunLongitude(dayNumber - 0.5 - timeZone / 24);
        }
        public static int getNewMoonDay(int k, double timeZone)
        {
            double jd = NewMoon(k);
            return INT(jd + 0.5 + timeZone / 24);
        }
        public static int getLunarMonth11(int yy, double timeZone)
        {
            double off = jdFromDate(31, 12, yy) - 2415021.076998695;
            int k = INT(off / 29.530588853);
            int nm = getNewMoonDay(k, timeZone);
            int sunLong = INT(getSunLongitude(nm, timeZone) / 30);
            if (sunLong >= 9)
            {
                nm = getNewMoonDay(k - 1, timeZone);
            }
            return nm;
        }
        public static int getLeapMonthOffset(int a11, double timeZone)
        {
            int k = INT(0.5 + (a11 - 2415021.076998695) / 29.530588853);
            int last; // Month 11 contains point of sun longutide 3*PI/2 (December solstice)
            int i = 1; // We start with the month following lunar month 11
            int arc = INT(getSunLongitude(getNewMoonDay(k + i, timeZone), timeZone) / 30);
            do
            {
                last = arc;
                i++;
                arc = INT(getSunLongitude(getNewMoonDay(k + i, timeZone), timeZone) / 30);
            } while (arc != last && i < 14);
            return i - 1;
        }
        /**
         *
         * @param dd
         * @param mm
         * @param yy
         * @param timeZone
         * @return array of [lunarDay, lunarMonth, lunarYear, leapOrNot]
         */
        public static LunarDate convertSolar2Lunar(SolarDate solarDate, double timeZone)
        {
            int lunarDay, lunarMonth, lunarYear, lunarLeap;
            int dayNumber = jdFromDate(solarDate.day, solarDate.month, solarDate.year);
            int k = INT((dayNumber - 2415021.076998695) / 29.530588853);
            int monthStart = getNewMoonDay(k + 1, timeZone);
            if (monthStart > dayNumber)
            {
                monthStart = getNewMoonDay(k, timeZone);
            }
            int a11 = getLunarMonth11(solarDate.year, timeZone);
            int b11 = a11;
            if (a11 >= monthStart)
            {
                lunarYear = solarDate.year;
                a11 = getLunarMonth11(solarDate.year - 1, timeZone);
            }
            else
            {
                lunarYear = solarDate.year + 1;
                b11 = getLunarMonth11(solarDate.year + 1, timeZone);
            }
            lunarDay = dayNumber - monthStart + 1;
            int diff = INT((monthStart - a11) / 29);
            lunarLeap = 0;
            lunarMonth = diff + 11;
            if (b11 - a11 > 365)
            {
                int leapMonthDiff = getLeapMonthOffset(a11, timeZone);
                if (diff >= leapMonthDiff)
                {
                    lunarMonth = diff + 10;
                    if (diff == leapMonthDiff)
                    {
                        lunarLeap = 1;
                    }
                }
            }
            if (lunarMonth > 12)
            {
                lunarMonth = lunarMonth - 12;
            }
            if (lunarMonth >= 11 && diff < 4)
            {
                lunarYear -= 1;
            }
            return new LunarDate()
            {
                day = lunarDay,
                month = lunarMonth,
                year = lunarYear,
                isLeapMonth = lunarLeap == 1
            };
            //return new int[] { lunarDay, lunarMonth, lunarYear, lunarLeap };
        }
        public static SolarDate convertLunar2Solar(LunarDate lunarDate, double timeZone)
        {
            int a11, b11;
            int lunarDay = lunarDate.day;
            int lunarMonth = lunarDate.month;
            int lunarYear = lunarDate.year;
            int lunarLeap = lunarDate.isLeapMonth == true ? 1 : 0;
            if (lunarMonth < 11)
            {
                a11 = getLunarMonth11(lunarYear - 1, timeZone);
                b11 = getLunarMonth11(lunarYear, timeZone);
            }
            else
            {
                a11 = getLunarMonth11(lunarYear, timeZone);
                b11 = getLunarMonth11(lunarYear + 1, timeZone);
            }
            int k = INT(0.5 + (a11 - 2415021.076998695) / 29.530588853);
            int off = lunarMonth - 11;
            if (off < 0)
            {
                off += 12;
            }
            if (b11 - a11 > 365)
            {
                int leapOff = getLeapMonthOffset(a11, timeZone);
                int leapMonth = leapOff - 2;
                if (leapMonth < 0)
                {
                    leapMonth += 12;
                }
                if (lunarLeap != 0 && lunarMonth != leapMonth)
                {
                    throw new Exception("Invalid input!");
                }
                else if (lunarLeap != 0 || off >= leapOff)
                {
                    off += 1;
                }
            }
            int monthStart = getNewMoonDay(k + off, timeZone);
            return jdToDate(monthStart + lunarDay - 1);
        }


        /**
     * Check if a given lunar date is valid.
     * @param lunarDay - The day of the lunar month (1-30)
     * @param lunarMonth - The month of the lunar year (1-12)
     * @param lunarYear - The lunar year
     * @param lunarLeap - Indicates if it is a leap month (1 for leap month, 0 otherwise)
     * @param timeZone - The time zone offset in hours (e.g., 7 for UTC+7)
     * @return true if the lunar date is valid; otherwise, false.
     */
        public static bool IsValidLunarDate(DateTime needCheckDate, double timeZone = 7.0)
        {
            // Validate lunar month and day
            int lunarDay = needCheckDate.Day;
            int lunarMonth = needCheckDate.Month;
            int lunarYear = needCheckDate.Year;
            if (lunarMonth < 1 || lunarMonth > 12)
                return false;

            if (lunarDay < 1 || lunarDay > 30)
                return false;

            try
            {
                var lunarDate = new LunarDate()
                {
                    day = lunarDay,
                    month = lunarMonth,
                    year = lunarYear,
                    isLeapMonth = false
                };
                Func<LunarDate, bool> checkLeap = (lunarDate) =>
                {
                    try
                    {
                        var tmp = VietCalendar.convertLunar2Solar(lunarDate, timeZone);
                        return false;
                    }
                    catch (Exception ex)
                    {
                        return true;
                    }
                };
                lunarDate.isLeapMonth = checkLeap(lunarDate);
                // Attempt to convert lunar date to solar date
                var solarDate = VietCalendar.convertLunar2Solar(lunarDate, timeZone);
                var revertToLunarDate = VietCalendar.convertSolar2Lunar(new SolarDate()
                {
                    day = solarDate.day,
                    month = solarDate.month,
                    year = solarDate.year,
                },timeZone);
                if(lunarDate.day == revertToLunarDate.day && lunarDate.month == revertToLunarDate.month && lunarDate.year == revertToLunarDate.year)
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                // Catch any conversion errors or invalid dates
                return false;
            }
        }
        //public static void main(String[] args)
        //{
        //    double TZ = 7.0;
        //    int start = jdFromDate(1, 1, 2001);
        //    int step = 15;
        //    int count = -1;
        //    while (count++ < 240)
        //    {
        //        int jd = start + step * count;
        //        int[] s = jdToDate(jd);
        //        int[] l = convertSolar2Lunar(s[0], s[1], s[2], TZ);
        //        int[] s2 = convertLunar2Solar(l[0], l[1], l[2], l[3], TZ);
        //        if (s[0] == s2[0] && s[1] == s2[1] && s[2] == s2[2])
        //        {
        //            Console.WriteLine("OK! " + s[0] + "/" + s[1] + "/" + s[2] + " -> " + l[0] + "/" + l[1] + "/" + l[2] + (l[3] == 0 ? "" : " leap"));
        //        }
        //        else
        //        {
        //            Console.WriteLine("ERROR! " + s[0] + "/" + s[1] + "/" + s[2] + " -> " + l[0] + "/" + l[1] + "/" + l[2] + (l[3] == 0 ? "" : " leap"));
        //        }
        //    }
        //}
    }
}
