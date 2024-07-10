using System.Management.Automation;

namespace UtilExpress
{
    [Cmdlet(VerbsCommon.New, "SubnetV4")]
    [Alias("New-Subnet")]
    [OutputType(typeof(SubnetV4Info))]
    public class SubnetV4_New : PSCmdlet
    {
        [Parameter(Position = 0)]
        [ValidatePattern(@"^(([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\/([0-9]|[1-2][0-9]|3[0-2])$")]
        public string CIDR { get;set;}

        protected override void ProcessRecord()
        {
            if (string.IsNullOrEmpty(CIDR))
                WriteObject(SubnetV4Info.GetRandom());
            else
                WriteObject(SubnetV4Info.Parse(CIDR));
        }
    }
}