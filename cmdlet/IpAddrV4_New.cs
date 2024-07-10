using System.Management.Automation;

namespace UtilExpress
{
    [Cmdlet(VerbsCommon.New, "IpAddrV4")]
    [Alias("New-Ip")]
    [OutputType(typeof(IpAddrV4Info))]
    public class IpAddrV4_New : PSCmdlet
    {
        [Parameter(ParameterSetName = "DotDecimal", Position = 0)]
        [ValidatePattern(@"^(([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])(\/([0-9]|[1-2][0-9]|3[0-2]))?$")]
        public string IpString {get; set;}

        protected override void ProcessRecord()
        {
            if (string.IsNullOrEmpty(IpString))
                WriteObject(IpAddrV4Info.GetRandom());
            else
                WriteObject(IpAddrV4Info.Parse(IpString));
        }
    }
}