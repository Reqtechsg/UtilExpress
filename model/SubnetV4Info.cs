using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace UtilExpress
{
    public class SubnetV4Info
    {
        #region variables

        // regex for CIDR notation
        private static readonly string _cidr_pattern = @"^(([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\/([0-9]|[1-2][0-9]|3[0-2])$";

        // we keep the network address for this subnet
        private readonly IpAddrV4Info _network_address;

        #endregion variables

        #region properties

        public string CIDR
        {
            get
            {
                var prefix = _network_address.ToString();
                var suffix = _network_address.PrefixLength;
                return $"{prefix}/{suffix}";
            }
        }

        public IpAddrV4Info FirstIP
        {
            get {return _network_address.Clone();}
        }

        public IpAddrV4Info LastIP
        {
            get
            {
                var subnet_mask_value = _network_address.SubnetMask.MaskValue;
                var host_mask_value   = ~subnet_mask_value;
                var last_ip_value     = _network_address.IpValue | host_mask_value;

                var octet1        = (byte)((last_ip_value & 0xff000000) >> 24);
                var octet2        = (byte)((last_ip_value & 0x00ff0000) >> 16);
                var octet3        = (byte)((last_ip_value & 0x0000ff00) >>  8);
                var octet4        = (byte)((last_ip_value & 0x000000ff) >>  0);
                var prefix_length = _network_address.SubnetMask.PrefixLength;

                return new IpAddrV4Info(octet1, octet2, octet3, octet4, prefix_length);
            }
        }

        public MaskV4Info SubnetMask
        {
            get {return _network_address.SubnetMask;}
        }

        public ULongInfo SubnetSize
        {
            get {return _network_address.SubnetMask.NetworkSize;}
        }

        public byte PrefixLength
        {
            get {return _network_address.PrefixLength;}

            set {
                // save prefix length
                _network_address.PrefixLength = value;

                // normalize the CIDR
                _network_address.Bytes = _network_address.NetworkId.Bytes;
            }
        }

        #endregion properties

        #region constructors

        public SubnetV4Info(byte octet1, byte octet2, byte octet3, byte octet4, byte prefix_length)
        {
            _network_address = new IpAddrV4Info(octet1, octet2, octet3, octet4, prefix_length);
        }

        public SubnetV4Info() : this(0, 0, 0, 0, 0)
        { }

        private SubnetV4Info(string cidr)
        {
            _network_address = IpAddrV4Info.Parse(cidr).NetworkId;
        }

        #endregion constructors

        #region methods

        public SubnetV4Info[] Divide(byte bits)
        {
            var prefix_length = _network_address.PrefixLength;

            if(bits == 0 || bits > 32)
                throw new ArgumentException("Bits must be between 1 and 32");

            if(bits + prefix_length > 32)
                throw new OverflowException("Network bits exceeded 32.");

            var divisor = (ulong)Math.Pow(2, bits);
            return this / divisor;
        }

        public IpAddrV4Info[] GetIpAddresses()
        {
            var ip_list       = new List<IpAddrV4Info>();
            var subnet_size   = _network_address.SubnetMask.NetworkSize.Number;
            var prefix_length = _network_address.PrefixLength;
            var subnet_value  = _network_address.IpValue;

            for(uint i = 0; i < subnet_size; i++)
            {
                var ip_value = subnet_value + i;
                var ip       = new IpAddrV4Info(ip_value){PrefixLength = prefix_length};

                ip_list.Add(ip);
            }
            return ip_list.ToArray();
        }

        public string[] ListIpAddresses()
        {
            var ip_list      = new List<string>();
            var subnet_size  = _network_address.SubnetMask.NetworkSize.Number;
            var subnet_value = _network_address.IpValue;

            for(uint i = 0; i < subnet_size; i++)
            {
                var ip_value = subnet_value + i;

                var octet1 = (byte)((ip_value & 0xff000000) >> 24);
                var octet2 = (byte)((ip_value & 0x00ff0000) >> 16);
                var octet3 = (byte)((ip_value & 0x0000ff00) >>  8);
                var octet4 = (byte)((ip_value & 0x000000ff) >>  0);

                var ip = string.Format("{0}.{1}.{2}.{3}", octet1, octet2, octet3, octet4);
                ip_list.Add(ip);
            }
            return ip_list.ToArray();
        }

        public override string ToString()
        {
            return CIDR;
        }

        public static SubnetV4Info Parse(string cidr)
        {
            var regex = new Regex(_cidr_pattern);
            var cidr_normalized = cidr.Replace(" ", string.Empty);

            if(!regex.IsMatch(cidr_normalized))
                throw new ArgumentException("CIDR is not valid");

            return new SubnetV4Info(cidr_normalized);
        }

        public static SubnetV4Info GetRandom()
        {
            var octets        = new byte[4];
            var rand          = new Random();
            var prefix_length = (byte)rand.Next(33);
            rand.NextBytes(octets);

            return Parse(string.Format("{0}.{1}.{2}.{3}/{4}", octets[0], octets[1], octets[2], octets[3], prefix_length));
        }

        public static SubnetV4Info[] operator /(SubnetV4Info dividend, ulong divisor)
        {
            var is_divisor_less_than_2             = divisor < 2;
            var is_divisor_power_of_2              = (divisor & (divisor - 1)) == 0;
            var is_divisor_bigger_than_subnet_size = divisor > dividend.SubnetSize.Number;

            if (is_divisor_less_than_2 || !is_divisor_power_of_2 || is_divisor_bigger_than_subnet_size)
                throw new ArgumentException($"Subnet cannot be divided by {divisor}");

            // a list to hold child subnets
            var subnets = new List<SubnetV4Info>();

            var prefix_length       = dividend._network_address.PrefixLength;
            var suffix_length       = (byte)(32 - prefix_length);
            var divisor_num_bits    = (byte)Math.Log(divisor, 2);
            var parent_subnet_value = dividend._network_address.IpValue;

            // prefix length of the divided subnets
            var child_prefix_length = prefix_length + divisor_num_bits;

            // number of bits to right-shift the divisor bits to just after the parent network bits
            var offset = suffix_length - divisor_num_bits;

            for(uint i = 0; i < divisor ; i++)
            {
                // value of the network bits to add for this child subnet
                var increment          = i << offset;

                // ip value for this child subnet
                var child_subnet_value = parent_subnet_value + increment;

                // octets for this child subnet
                var octet1 = (byte)((child_subnet_value >> 24) & 0xff);
                var octet2 = (byte)((child_subnet_value >> 16) & 0xff);
                var octet3 = (byte)((child_subnet_value >>  8) & 0xff);
                var octet4 = (byte)((child_subnet_value >>  0) & 0xff);

                var child_subnet = new SubnetV4Info($"{octet1}.{octet2}.{octet3}.{octet4}/{child_prefix_length}");
                subnets.Add(child_subnet);
            }

            return subnets.ToArray();
        }

        public static SubnetV4Info Summarize(SubnetV4Info[] subnets)
        {
            var network_ids = subnets.Select(s => s._network_address.Bytes);
            var first_id    = network_ids.ElementAt(0);

            var mask_bytes = new byte[] {0, 0, 0, 0};

            byte i; for (i = 0 ; i < 32 ; i++)
            {
                var byte_index = (byte)(i / 8);
                var bit_index  = (byte)(i % 8);
                var byte_mask  = (byte)(128 >> bit_index);

                var ref_byte = first_id[byte_index] & byte_mask;

                var is_common_bit = network_ids.All(compare_bytes => {
                    var compare_byte = compare_bytes[byte_index] & byte_mask;
                    return compare_byte == ref_byte;
                });

                if (is_common_bit)
                    mask_bytes[byte_index] |= byte_mask;
                else
                    break;
            }

            var prefix_length = i;
            var summary       = new byte[4];
            summary[0]        = (byte)(first_id[0] & mask_bytes[0]);
            summary[1]        = (byte)(first_id[1] & mask_bytes[1]);
            summary[2]        = (byte)(first_id[2] & mask_bytes[2]);
            summary[3]        = (byte)(first_id[3] & mask_bytes[3]);

            return new SubnetV4Info(summary[0], summary[1], summary[2], summary[3], prefix_length);
        }

        #endregion methods
    }
}