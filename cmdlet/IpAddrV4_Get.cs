using System.Management.Automation;

namespace UtilExpress
{
    [Cmdlet(VerbsCommon.Get, "IpAddrV4")]
    [Alias("Get-Ip")]
    [OutputType(typeof(IpAddrV4Info[]))]
    public class IpAddrV4_Get : PSCmdlet
    {
        [Parameter(ParameterSetName = "Subnet", Position = 0, Mandatory = true, ValueFromPipeline = true)]
        public SubnetV4Info Subnet {get; set;}

        [Parameter(ParameterSetName = "CIDR", Position = 0, Mandatory = true, ValueFromPipeline = true)]
        [ValidatePattern(@"^(([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\/([0-9]|[1-2][0-9]|3[0-2])$")]
        public string CIDR {get; set;}

        protected override void ProcessRecord()
        {
            if (ParameterSetName == "Subnet")
            {
                WriteObject(Subnet.GetIpAddresses(), true);
                return;
            }

            if(ParameterSetName == "CIDR")
            {
                var subnet = SubnetV4Info.Parse(CIDR);
                WriteObject(subnet.GetIpAddresses(), true);
                return;
            }
        }
    }
}