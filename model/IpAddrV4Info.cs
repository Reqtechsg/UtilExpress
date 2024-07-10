using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace UtilExpress
{
    public class IpAddrV4Info : IComparable, IComparable<IpAddrV4Info>
    {
        #region variables

        // regex for decimal dotted notation - i.e. ip addresss
        private static readonly string _ip_pattern = @"^(([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])$";
        private static readonly Regex _ip_regex    = new Regex(_ip_pattern);

        // regex for cidr notation
        private static readonly string _cidr_pattern = _ip_pattern.TrimEnd('$') + @"\/([0-9]|[1-2][0-9]|3[0-2])$";
        private static readonly Regex _cidr_regex    = new Regex(_cidr_pattern);

        // escape codes for emphasizing network and host bits
        public static CsStyle FormatNetworkBits     = CsStyle.ForegroundColor.BrightMagenta;
        public static CsStyle FormatHostBits        = CsStyle.ForegroundColor.BrightCyan;
        private static readonly CsStyle FormatReset = CsStyle.Reset;
        private static readonly CsStyle FormatDim   = CsStyle.Dim;

        // 4-byte array representation
        private readonly byte[] _bytes;

        // subnet mask
        private readonly MaskV4Info _subnet_mask;

        #endregion variables

        #region properties

        public byte[] Bytes
        {
            get {return (byte[])_bytes.Clone();}

            set {
                if (value.Length != 4)
                    throw new ArgumentException();

                _bytes[0] = value[0];
                _bytes[1] = value[1];
                _bytes[2] = value[2];
                _bytes[3] = value[3];
            }
        }

        public byte PrefixLength
        {
            get {return _subnet_mask.PrefixLength;}
            set {_subnet_mask.PrefixLength = value;}
        }

        public string IpAddress
        {
            get {return string.Format("{0}.{1}.{2}.{3}", _bytes[0], _bytes[1], _bytes[2], _bytes[3]);}
        }

        public MaskV4Info SubnetMask
        {
            get {return _subnet_mask;}
        }

        public IpAddrV4Info NetworkId
        {
            get {
                // prepare byte array to save network ID
                var network_bytes = new byte[4];

                // save ip bytes, mask bytes and prefix length
                var ip_bytes      = _bytes;
                var mask_bytes    = _subnet_mask.Bytes;
                var prefix_length = _subnet_mask.PrefixLength;

                // filter out network bits
                for(int i = 0; i < 4; i++)
                    network_bytes[i] = (byte)(ip_bytes[i] & mask_bytes[i]);

                // return network ID
                return new IpAddrV4Info(network_bytes[0], network_bytes[1], network_bytes[2], network_bytes[3], prefix_length);
            }
        }

        public IpAddrV4Info HostId
        {
            get {
                // prepare byte array to save host ID
                var host_bytes = new byte[4];

                // save ip bytes, mask bytes and prefix length
                var ip_bytes      = _bytes;
                var mask_bytes    = _subnet_mask.Bytes;
                var prefix_length = _subnet_mask.PrefixLength;

                // filter out host bits
                for(int i = 0; i < 4; i++)
                    host_bytes[i] = (byte)(ip_bytes[i] & ~mask_bytes[i]);

                // return host ID
                return new IpAddrV4Info(host_bytes[0], host_bytes[1], host_bytes[2], host_bytes[3], prefix_length);
            }
        }

        internal uint IpValue
        {
            get {return GetValue(_bytes);}
        }

        internal bool IsNetworkId
        {
            get {
                var ip_value       = GetValue(_bytes);
                var wildcard_value = ~_subnet_mask.MaskValue;
                var is_nid         = (ip_value & wildcard_value) == 0;

                return is_nid;
            }
        }

        internal bool IsHostId
        {
            get {
                var ip_value   = GetValue(_bytes);
                var mask_value = _subnet_mask.MaskValue;
                var is_hid     = (ip_value & mask_value) == 0;

                return is_hid;
            }
        }

        #endregion properties

        #region constructors

        public IpAddrV4Info(byte octet1, byte octet2, byte octet3, byte octet4, byte prefixLength)
        {
            _bytes = new byte[4];
            _bytes[0] = octet1;
            _bytes[1] = octet2;
            _bytes[2] = octet3;
            _bytes[3] = octet4;

            _subnet_mask = new MaskV4Info(prefixLength);
        }

        public IpAddrV4Info(byte octet1, byte octet2, byte octet3, byte octet4) : this(octet1, octet2, octet3, octet4, 32)
        { }

        public IpAddrV4Info(uint value, byte prefixLength) : this(
            (byte)((value >> 24) & 0xff),
            (byte)((value >> 24) & 0xff),
            (byte)((value >>  8) & 0xff),
            (byte)((value >>  0) & 0xff),
            prefixLength
        ) { }

        public IpAddrV4Info(uint value) : this(value, 32)
        { }

        #endregion constructors

        #region utility methods

        public void ShowDetails()
        {
            var ip   = _bytes;
            var mask = _subnet_mask.Bytes;
            var nid  = NetworkId.Bytes;
            var hid  = HostId.Bytes;

            string line1 = string.Format("IP Address  : {0,3}.{1,3}.{2,3}.{3,3}",   ip[0],   ip[1],   ip[2],   ip[3]);
            string line2 = string.Format("Subnet Mask : {0,3}.{1,3}.{2,3}.{3,3}", mask[0], mask[1], mask[2], mask[3]);
            string line3 = string.Format("Network ID  : {0,3}.{1,3}.{2,3}.{3,3}",  nid[0],  nid[1],  nid[2],  nid[3]);
            string line4 = string.Format("Host ID     : {0,3}.{1,3}.{2,3}.{3,3}",  hid[0],  hid[1],  hid[2],  hid[3]);

            Console.WriteLine();
            Console.WriteLine(line1);
            Console.WriteLine(line2);
            Console.WriteLine(line3);
            Console.WriteLine(line4);
            Console.WriteLine();
        }

        public void ShowBinaryDetails()
        {
            var ip   = _bytes;
            var mask = _subnet_mask.Bytes;
            var nid  = NetworkId.Bytes;
            var hid  = HostId.Bytes;

            var line1 = string.Format("IP Address  : {0,3:b8}.{1,3:b8}.{2,3:b8}.{3,3:b8}",   ip[0],   ip[1],   ip[2],   ip[3]);
            var line2 = string.Format("Subnet Mask : {0,3:b8}.{1,3:b8}.{2,3:b8}.{3,3:b8}", mask[0], mask[1], mask[2], mask[3]);
            var line3 = string.Format("Network ID  : {0,3:b8}.{1,3:b8}.{2,3:b8}.{3,3:b8}",  nid[0],  nid[1],  nid[2],  nid[3]);
            var line4 = string.Format("Host ID     : {0,3:b8}.{1,3:b8}.{2,3:b8}.{3,3:b8}",  hid[0],  hid[1],  hid[2],  hid[3]);

            Console.WriteLine();
            Console.WriteLine(line1);
            Console.WriteLine(line2);
            Console.WriteLine(line3);
            Console.WriteLine(line4);
            Console.WriteLine();
        }

        public void ShowEmphasizedBinaryDetails()
        {
            var ip_bytes   = _bytes;
            var mask_bytes = _subnet_mask.Bytes;
            var nid_bytes  = NetworkId.Bytes;
            var hid_bytes  = HostId.Bytes;

            var prefix_length = _subnet_mask.PrefixLength;
            var separator     = @".";

            var ip_emphasized   = Emphasize(ip_bytes, prefix_length, separator, false, false);
            var mask_emphasized = Emphasize(mask_bytes, prefix_length, separator, false, false);
            var nid_emphasized  = Emphasize(nid_bytes, prefix_length, separator, false, true);
            var hid_emphasized  = Emphasize(hid_bytes, prefix_length, separator, true, false);

            var line1 = $"IP Address  : {ip_emphasized}";
            var line2 = $"Subnet Mask : {mask_emphasized}";
            var line3 = $"Network ID  : {nid_emphasized}";
            var line4 = $"Host ID     : {hid_emphasized}";

            Console.WriteLine();
            Console.WriteLine(line1);
            Console.WriteLine(line2);
            Console.WriteLine(line3);
            Console.WriteLine(line4);
            Console.WriteLine();
        }

        public void ShowFullDetails()
        {
            var ip_bytes   = _bytes;
            var mask_bytes = _subnet_mask.Bytes;
            var nid_bytes  = NetworkId.Bytes;
            var hid_bytes  = HostId.Bytes;

            var prefix_length = _subnet_mask.PrefixLength;
            var separator     = @".";

            var ip_emphasized   = Emphasize(ip_bytes, prefix_length, separator, false, false);
            var mask_emphasized = Emphasize(mask_bytes, prefix_length, separator, false, false);
            var nid_emphasized  = Emphasize(nid_bytes, prefix_length, separator, false, true);
            var hid_emphasized  = Emphasize(hid_bytes, prefix_length, separator, true, false);

            var ip   = _bytes;
            var mask = _subnet_mask.Bytes;
            var nid  = NetworkId.Bytes;
            var hid  = HostId.Bytes;

            string ip_ddn   = string.Format("IP Address  : {0,3}.{1,3}.{2,3}.{3,3}",   ip[0],   ip[1],   ip[2],   ip[3]);
            string mask_ddn = string.Format("Subnet Mask : {0,3}.{1,3}.{2,3}.{3,3}", mask[0], mask[1], mask[2], mask[3]);
            string nid_ddn  = string.Format("Network ID  : {0,3}.{1,3}.{2,3}.{3,3}",  nid[0],  nid[1],  nid[2],  nid[3]);
            string hid_ddn  = string.Format("Host ID     : {0,3}.{1,3}.{2,3}.{3,3}",  hid[0],  hid[1],  hid[2],  hid[3]);

            string line1 = ip_ddn   + "  [ " + ip_emphasized   + " ]";
            string line2 = mask_ddn + "  [ " + mask_emphasized + " ]";
            string line3 = nid_ddn  + "  [ " + nid_emphasized  + " ]";
            string line4 = hid_ddn  + "  [ " + hid_emphasized  + " ]";

            Console.WriteLine();
            Console.WriteLine(line1);
            Console.WriteLine(line2);
            Console.WriteLine(line3);
            Console.WriteLine(line4);
            Console.WriteLine();
        }

        public SubnetV4Info[] GetAllSubnets()
        {
            var subnets  = new List<SubnetV4Info>();
            var ip_value = GetValue(_bytes);

            for(int i = 0 ; i < 33 ; i++)
            {
                var mask_value = (i == 32) ? uint.MinValue : uint.MaxValue << i;

                var network_value = ip_value & mask_value;
                var bytes         = GetBytes(network_value);
                var prefix_length = (byte)(32 - i);

                var subnet = new SubnetV4Info(bytes[0], bytes[1], bytes[2], bytes[3], prefix_length);

                subnets.Add(subnet);
            }

            return subnets.ToArray();
        }

        public IpAddrV4Info Clone()
        {
            return new IpAddrV4Info(_bytes[0], _bytes[1], _bytes[2], _bytes[3], _subnet_mask.PrefixLength);
        }

        #endregion utility methods

        #region string methods

        public override string ToString()
        {
            return IpAddress;
        }

        public string ToBinary(string separator = " ")
        {
            var binary = _bytes.Select(octet => octet.ToString("b8")).ToArray();
            return string.Join(separator, binary);
        }

        public string ToEmphasizedBinary(string separator = " ")
        {
            var bytes            = _bytes;
            var prefix_length    = _subnet_mask.PrefixLength;
            var dim_network_bits = false;
            var dim_host_bits    = false;

            return Emphasize(bytes, prefix_length, separator, dim_network_bits, dim_host_bits);
        }

        #endregion string methods

        #region comparison methods

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            if (!(obj is IpAddrV4Info other))
                throw new ArgumentException("Object is not IPAddrV4Info");
            else
                return CompareTo(other);
        }

        public int CompareTo(IpAddrV4Info other)
        {
            if (other == null)
                return 1;

            int i = 0;
            do{
                var my_byte    = _bytes[i];
                var other_byte = other.Bytes[i];
                var result     = my_byte.CompareTo(other_byte);

                if(result != 0)
                    return result;
            }while(i++ < 4);

            return 0;
        }

        #endregion comparison methods

        #region helper methods

        // get the 32-bit numeric value from a 4-byte array
        // opposite of GetBytes() method
        private static uint GetValue(byte[] bytes)
        {
            return ((uint)bytes[0] << 24) +
                   ((uint)bytes[1] << 16) +
                   ((uint)bytes[2] <<  8) +
                   ((uint)bytes[3] <<  0);
        }

        // get a 4-byte array from a 32-bit number
        // opposite of GetValue() method
        private static byte[] GetBytes(uint value)
        {
            var bytes = new byte[4];

            bytes[0]  = (byte)((value & 0xff000000) >> 24);
            bytes[1]  = (byte)((value & 0x00ff0000) >> 16);
            bytes[2]  = (byte)((value & 0x0000ff00) >>  8);
            bytes[3]  = (byte)((value & 0x000000ff) >>  0);

            return bytes;
        }

        private static string Emphasize(byte[] bytes, byte prefixLength, string separator, bool dimNetworkBits, bool dimHostBits)
        {
            Debug.Assert(bytes.Length == 4);
            Debug.Assert(prefixLength <= 32);

            // save a 32-bit binary representation of the IP
            var bytes_in_binary = bytes.Select(octet => octet.ToString("b8")).ToArray();
            var binary          = string.Join(string.Empty, bytes_in_binary);

            // save the network bits to a string variable
            var network_bits    = binary.Substring(0, prefixLength);

            // save the host bits to a string variable
            var host_bits       = prefixLength == 32 ? string.Empty : binary.Substring(prefixLength, 32 - prefixLength);

            // save the index to insert the first separator to host bits first
            var num_network_bits = network_bits.Length;
            var first_host_separator_index = 8 - (num_network_bits % 8);

            // insert separators to the network bits
            for(int i = 8 ; i <= network_bits.Length && i < 32 ; i += 8+separator.Length)
                network_bits = network_bits.Insert(i, separator);

            // insert separators to the host bits
            for(int j = first_host_separator_index ; j < host_bits.Length ; j += 8+separator.Length)
                host_bits = host_bits.Insert(j, separator);

            // get the escapes code to emphasize the network and host bits
            var format_reset        = FormatReset.Value;
            var format_dim          = FormatDim.Value;
            var format_network_bits = dimNetworkBits ? FormatNetworkBits.Value + FormatDim.Value : FormatNetworkBits.Value;
            var format_host_bits    = dimHostBits ? FormatHostBits.Value + FormatDim.Value : FormatHostBits.Value;

            // return emphasized string
            return $"{format_network_bits}{network_bits}{format_reset}{format_host_bits}{host_bits}{format_reset}";
        }

        #endregion helper methods

        #region static methods

        public static IpAddrV4Info Parse(string ip)
        {
            // try to match ip pattern
            if (_ip_regex.IsMatch(ip))
            {
                var octets = ip.Split('.').Select(octet => Convert.ToByte(octet)).ToArray();
                return new IpAddrV4Info(octets[0], octets[1], octets[2], octets[3]);
            }

            // try to match cidr pattern
            if (_cidr_regex.IsMatch(ip))
            {
                var parts = ip.Split('/');
                var prefix = parts[0];
                var suffix = Convert.ToByte(parts[1]);
                var octets = prefix.Split('.').Select(octet => Convert.ToByte(octet)).ToArray();

                return new IpAddrV4Info(octets[0], octets[1], octets[2], octets[3], suffix);
            }

            throw new ArgumentException("IP is not valid");
        }

        public static IpAddrV4Info GetRandom()
        {
            var octets        = new byte[4];
            var rand          = new Random();
            var prefix_length = (byte)rand.Next(33);
            rand.NextBytes(octets);

            return new IpAddrV4Info(octets[0], octets[1], octets[2], octets[3], prefix_length);
        }

        #endregion static methods
    }
}