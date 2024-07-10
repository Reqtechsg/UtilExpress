using System;
using System.Management.Automation;

namespace UtilExpress
{
    [Cmdlet(VerbsCommon.Split, "SubnetV4")]
    [Alias("Split-Subnet")]
    [OutputType(typeof(SubnetV4Info[]))]
    public class SubnetV4_Split : PSCmdlet
    {
        private byte _bits = 1;

        [Parameter(ParameterSetName = "SubnetV4Info", Position = 0, Mandatory = true, ValueFromPipeline = true)]
        public SubnetV4Info Subnet {get; set;}

        [Parameter(ParameterSetName = "CIDR", Position = 0, Mandatory = true, ValueFromPipeline = true)]
        [ValidatePattern(@"^(([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\/([0-9]|[1-2][0-9]|3[0-2])$")]
        public string CIDR { get; set;}

        [Parameter(ParameterSetName = "SubnetV4Info", Position = 1)]
        [Parameter(ParameterSetName = "CIDR", Position = 1)]
        [ValidateRange(0, 33)]

        public byte Bits
        {
            get {return _bits;}
            set {_bits = value;}
        }

        protected override void ProcessRecord()
        {
            if (ParameterSetName == "SubnetV4Info")
            {
                WriteObject(Subnet.Divide(_bits), true);
                return;
            }

            if (ParameterSetName == "CIDR")
            {
                WriteObject(SubnetV4Info.Parse(CIDR).Divide(_bits), true);
                return;
            }
        }
    }
}