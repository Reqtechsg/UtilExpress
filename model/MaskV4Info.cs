using System;
using System.Collections.Generic;
//using System.Collections.Immutable;
using System.Linq;

namespace UtilExpress
{
    public class MaskV4Info : IComparable, IComparable<MaskV4Info>
    {
        #region variables
        //private static readonly ImmutableArray<byte> _valid_mask = ImmutableArray.Create(new byte[]{255, 254, 252, 248, 240, 224, 192, 128});

        // escape codes for emphasizing network and host bits
        public static CsStyle FormatNetworkBits     = CsStyle.ForegroundColor.BrightMagenta;
        public static CsStyle FormatHostBits        = CsStyle.ForegroundColor.BrightCyan;
        private static readonly CsStyle FormatReset = CsStyle.Reset;

        // 4-byte array representation
        private readonly byte[] _bytes = new byte[4];

        // prefix length representation
        private byte _prefix_length;

        // 32-bit decimal value of the mask
        private uint _mask_value;

        // subnet size
        private ULongInfo _network_size;

        #endregion variables

        #region properties

        public byte[] Bytes
        {
            get {
                // returns a copy of the array instead,
                // so that the elements of the actual one cannot be modified from outside the object
                return (byte[])_bytes.Clone();
            }
        }

        public byte PrefixLength
        {
            get {return _prefix_length;}
            set {
                if(value > 32)
                    throw new ArgumentException("Prefix length cannot be greater than 32");

                // save prefix length
                _prefix_length = value;

                // save subnet mask value
                _mask_value = _prefix_length == 0 ? uint.MinValue : 0xffffffff << (32 - _prefix_length);

                // save byte values of the mask
                _bytes[0] = _bytes[1] = _bytes[2] = _bytes[3] = 255;

                if(_prefix_length < 32)
                {
                    // walk the bytes from right to left
                    for(int i = 3; i >= 0 ; i--)
                    {
                        // if prefix length is less than the 32-bit index of this byte's first bit,
                        // zero all bits for this byte and move to the next one
                        if (_prefix_length < i * 8)
                            _bytes[i] = 0;
                        else
                        {
                            // zero out the required number of bits from the right
                            _bytes[i] = (byte)(_bytes[i] << 8 - _prefix_length % 8);

                            // prefix length stops at this byte, break the loop!
                            break;
                        }
                    }
                }

                // save network size
                var inverted_mask_value = ~_mask_value & 0xff_ff_ff_ff;
                var subnet_size_value  = (ulong)inverted_mask_value + 1;
                _network_size          = new ULongInfo(subnet_size_value);
            }
        }

        public ULongInfo NetworkSize
        {
            get { return _network_size;}
        }

        public string Mask
        {
            get {return ToString();}
        }

        public string Bits
        {
            get {return ToEmphasizedBinary(string.Empty);}
        }

        internal uint MaskValue
        {
            get {return _mask_value;}
        }

        #endregion properties

        #region constructors

        public MaskV4Info(byte prefixLength)
        {
            PrefixLength = prefixLength;
        }

        #endregion constructors

        #region methods

        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}.{3}", _bytes[0], _bytes[1], _bytes[2], _bytes[3]);
        }

        public string ToBinary(string separator = " ")
        {
            var binary = _bytes.Select(octet => octet.ToString("b8")).ToArray();
            return string.Join(separator, binary);
        }

        public string ToEmphasizedBinary(string separator = " ")
        {
            // convert each byte element to its binary form
            var bytes_in_binary = _bytes.Select(octet => octet.ToString("b8")).ToArray();

            // save a 32-bit binary representation of the IP
            var binary = string.Join(string.Empty, bytes_in_binary);

            // save the network bits to a string variable
            var network_bits = binary.Substring(0, _prefix_length);

            // save the host bits to a string variable
            var host_bits = _prefix_length == 32 ? string.Empty : binary.Substring(_prefix_length, 32 - _prefix_length);

            // save the index to insert the first separator to host bits first
            var num_network_bits = network_bits.Length;
            var first_host_separator_index = 8 - (num_network_bits % 8);

            // insert separators to network bits
            for(int i = 8 ; i <= network_bits.Length && i < 32 ; i += 8 + separator.Length)
                network_bits = network_bits.Insert(i, separator);

            // insert separators to host bits
            for(int j = first_host_separator_index ; j < host_bits.Length ; j += 8 + separator.Length)
                host_bits = host_bits.Insert(j, separator);

            // get the escapes code to emphasize the network and host bits
            var format_network_bits = FormatNetworkBits.Value;
            var format_host_bits    = FormatHostBits.Value;
            var format_reset        = FormatReset.Value;

            // return emphasized string
            return $"{format_network_bits}{network_bits}{format_reset}{format_host_bits}{host_bits}{format_reset}";
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            if (!(obj is MaskV4Info other))
                throw new ArgumentException("Object is not MaskV4Info");
            else
                return CompareTo(other);
        }

        public int CompareTo(MaskV4Info other)
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

        public static MaskV4Info[] ListAll()
        {
            var masks = new List<MaskV4Info>();
            for(byte prefix_length = 0 ; prefix_length <= 32 ; prefix_length++)
                masks.Add(new MaskV4Info(prefix_length));

            return masks.ToArray();
        }

        #endregion methods
    }
}