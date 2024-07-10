using System;


namespace UtilExpress
{
    public class ULongInfo : IComparable, IComparable<NumberInfo>
    {
        private static readonly string[] _sizes = {string.Empty, " K", " M", " B", " T", " q", " Q"};

        public static bool FormatHumanReadable = false;
        public static bool FormatThousandsSeparator = true;

        public ulong Number { get; set; }

        public ULongInfo(ulong number)
        {
            Number = number;
        }

        public override string ToString()
        {
            string format_number;

            if(FormatHumanReadable)
            {
                double number = Number;
                int    order  = 0;

                while(number >= 1000)
                {
                    number /= 1000;
                    order++;
                }
                format_number = $"{number:0.0}{_sizes[order]}";
            }
            else
            {
                if(FormatThousandsSeparator)
                    format_number = Number.ToString("N0");
                else
                    format_number = Number.ToString();
            }
            return format_number;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            if (!(obj is NumberInfo other))
                throw new ArgumentException("Object is not NumberInfo");
            else
                return Number.CompareTo(other.Number);
        }

        public int CompareTo(NumberInfo other)
        {
            if (other == null)
                return 1;

            return Number.CompareTo(other.Number);
        }
    }
}
