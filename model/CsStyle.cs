namespace UtilExpress
{
    public class CsStyle
    {
        public static CsStyle Bold
        {
            get {return new CsStyle("\x1b[1m");}
        }

        public static CsStyle Reset
        {
            get {return new CsStyle("\x1b[0m");}
        }

        public static CsStyle Dim
        {
            get {return new CsStyle("\x1b[2m");}
        }

        public string Value {get; private set;}

        private CsStyle(string value)
        {
            Value = value;
        }

        public static class ForegroundColor
        {
            public static CsStyle BrightGreen
            {
                get {return new CsStyle("\x1b92m");}
            }

            public static CsStyle BrightMagenta
            {
                get {return new CsStyle("\x1b[95m");}
            }

            public static CsStyle BrightCyan
            {
                get {return new CsStyle("\x1b[96m");}
            }
        }
    }
}