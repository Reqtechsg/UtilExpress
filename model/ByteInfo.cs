using System;

namespace UtilExpress
{
    public class ByteInfo : IComparable, IComparable<ByteInfo>
    {
        private static readonly string[] _sizes = {"B", "KB", "MB", "GB", "TB", "PB", "EB"};

        public static bool FormatHumanReadable = true;
        public static bool FormatThousandsSeparator = true;

        public long Bytes { get; set; }

        public ByteInfo(long bytes)
        {
            Bytes = bytes;
        }

        public override string ToString()
        {
            string format_byte;

            if(FormatHumanReadable)
            {
                double number = Bytes;
                int    order  = 0;

                while(number >= 1024)
                {
                    number /= 1024;
                    order++;
                }
                format_byte = $"{number:0.0} {_sizes[order]}";
            }
            else
            {
                if(FormatThousandsSeparator)
                    format_byte = Bytes.ToString("N0");
                else
                    format_byte = Bytes.ToString();
            }

            return format_byte;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            if (!(obj is ByteInfo other))
                throw new ArgumentException("Object is not ByteInfo");
            else
                return Bytes.CompareTo(other.Bytes);
        }

        public int CompareTo(ByteInfo other)
        {
            if (other == null)
                return 1;

            return Bytes.CompareTo(other.Bytes);
        }
    }
}