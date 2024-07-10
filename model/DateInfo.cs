using System;

namespace UtilExpress
{
    public class DateInfo : IComparable, IComparable<DateInfo>
    {
        public static bool FormatUtc = false;
        public static bool Format24Hours = true;

        public DateTime Date { get; set;}

        public DateInfo(DateTime date)
        {
            Date = date;
        }

        public override string ToString()
        {
            string format_dateinfo;

            if(FormatUtc)
                if(Format24Hours)
                    format_dateinfo = Date.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");
                else
                    format_dateinfo = Date.ToUniversalTime().ToString("yyyy-MM-dd hh:mm:ss tt");
            else
                if(Format24Hours)
                    format_dateinfo = Date.ToString("yyyy-MM-dd HH:mm:ss");
                else
                    format_dateinfo = Date.ToString("yyyy-MM-dd hh:mm:ss tt");

            return format_dateinfo;
        }

        public int CompareTo(object obj)
        {
            if(obj == null)
                return 1;

            if(!(obj is DateInfo other))
                throw new ArgumentException();
            else
                return Date.CompareTo(other.Date);
        }

        public int CompareTo(DateInfo other)
        {
            if(other == null)
                return 1;

            return Date.CompareTo(other.Date);
        }
    }
}