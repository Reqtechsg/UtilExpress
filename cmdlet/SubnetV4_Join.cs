using System.Collections.Generic;
using System.Management.Automation;

namespace UtilExpress
{
    [Cmdlet(VerbsCommon.Join, "SubnetV4")]
    [Alias("Join-Subnet")]
    [OutputType(typeof(SubnetV4Info))]
    public class SubnetV4_Join : PSCmdlet
    {
        private List<SubnetV4Info>subnet_list;

        [Parameter(ParameterSetName = "SubnetV4Info", Position = 0, Mandatory = true, ValueFromPipeline = true)]
        public SubnetV4Info Subnet {get; set;}

        [Parameter(ParameterSetName = "CIDR", Position = 0, Mandatory = true, ValueFromPipeline = true)]
        [ValidatePattern(@"^(([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\/([0-9]|[1-2][0-9]|3[0-2])$")]
        public string CIDR { get; set;}

        protected override void BeginProcessing()
        {
           subnet_list = new List<SubnetV4Info>();
        }

        protected override void ProcessRecord()
        {
            if (ParameterSetName == "SubnetV4Info")
            {
                subnet_list.Add(Subnet);
                return;
            }

            if (ParameterSetName == "CIDR")
            {
                subnet_list.Add(SubnetV4Info.Parse(CIDR));
                return;
            }
        }

        protected override void EndProcessing()
        {
            WriteObject(SubnetV4Info.Summarize(subnet_list.ToArray()));
        }
    }
}