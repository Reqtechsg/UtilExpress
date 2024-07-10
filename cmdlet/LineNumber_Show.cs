using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;

namespace UtilExpress
{
    [Cmdlet(VerbsCommon.Show, "LineNumber")]
    [OutputType(typeof(List<LineInfo>))]
    public class LineNumber_Show : PSCmdlet
    {
        private List<LineInfo> _lines;
        private int _processed;

        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
        public string Text {get; set;}

        [Parameter(Position = 1)]
        [ValidatePattern(@"^[1-9]+[0-9]*\.\.[1-9]+[0-9]*$")]
        public string Range {get; set;}

        protected override void BeginProcessing()
        {
            _lines     = new List<LineInfo>();
            _processed = 0;
        }

        protected override void ProcessRecord()
        {
            if(string.IsNullOrEmpty(Text))
                return;

            using (var reader = new StringReader(Text))
            {
                while (true)
                {
                    var line = reader.ReadLine();

                    if(line == null) break;

                    _lines.Add(new LineInfo(){
                        Line = ++_processed,
                        Text = line
                    });
                }
            }
        }

        protected override void EndProcessing()
        {
            if(string.IsNullOrEmpty(Range))
            {
                WriteObject(_lines, true);
                return;
            }

            var numbers = Range.Split(new string[] {".."}, System.StringSplitOptions.None);
            var num1    = Convert.ToInt32(numbers[0]);
            var num2    = Convert.ToInt32(numbers[1]);

            var overflow = num1 > _lines.Count || num2 > _lines.Count;
            if(overflow)
            {
                var error_record = new ErrorRecord(
                    new OverflowException("Specified range has exceeded the total number of lines in the text."),
                    "InvalidRange",
                    ErrorCategory.LimitsExceeded,
                    null
                );
                ThrowTerminatingError(error_record);
                return;
            }

            var start  = Math.Min(num1, num2) - 1;
            var length = Math.Abs(num1 - num2) + 1;
            var range  = _lines.GetRange(start, length);

            if(num1 > num2)
                range.Reverse();

            WriteObject(range, true);
        }
    }
}