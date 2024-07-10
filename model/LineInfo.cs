using System;
using System.Collections.Generic;
using System.IO;

namespace UtilExpress
{
    public class LineInfo : IComparable, IComparable<LineInfo>
    {
        public int Line    { get; set; }
        public string Text { get; set; }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            if (!(obj is LineInfo other))
                throw new ArgumentException("");
            else
                return Line.CompareTo(other.Line);
        }

        public int CompareTo(LineInfo other)
        {
            if (other == null)
                return 1;

            return Line.CompareTo(other.Line);
        }
    }

    public class Lines
    {
        private readonly List<LineInfo> _lines;

        public Lines(string s)
        {
            _lines = new List<LineInfo>();

            int i = 0;
            using (var reader = new StringReader(s))
            {
                _lines.Add(new LineInfo(){
                    Line = ++i,
                    Text = reader.ReadLine()
                });
            }
        }
    }
}