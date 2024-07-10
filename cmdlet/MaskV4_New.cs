using System.Management.Automation;

namespace UtilExpress
{
    [Cmdlet(VerbsCommon.New, "MaskV4")]
    [Alias("New-Mask")]
    [OutputType(typeof(MaskV4Info))]
    public class MaskV4_New : PSCmdlet
    {
        [Parameter(ParameterSetName = "PrefixLength", Position = 0, Mandatory = true)]
        [ValidateRange(0, 33)]
        public int PrefixLength {get; set;}

        [Parameter(ParameterSetName = "All", Mandatory = true)]
        public SwitchParameter All {get; set;}

        protected override void ProcessRecord()
        {
            if (ParameterSetName == "PrefixLength")
            {
                WriteObject(new MaskV4Info((byte)PrefixLength));
                return;
            }

            if(ParameterSetName == "All")
            {
                if(All.IsPresent)
                    WriteObject(MaskV4Info.ListAll(), true);
                return;
            }
        }
    }
}