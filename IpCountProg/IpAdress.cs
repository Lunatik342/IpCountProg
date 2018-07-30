using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace IpCountProg
{
    public class IpRepresentation : IPAddress
    {

        public IpRepresentation(byte[] ip) : base(ip)
        {
        }

        public byte[] ToIpParts(int b)
        {
            return Parse(b.ToString()).GetAddressBytes();
        }

        public override string ToString()
        {
            var ipOctets = GetAddressBytes();
            return $"{ipOctets[0]}.{ipOctets[1]}.{ipOctets[2]}.{ipOctets[3]}";
        }

        public virtual string BinaryView()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var addressByte in GetAddressBytes())
            {
                sb.Append(Convert.ToString(addressByte, 2).PadLeft(8, '0')+".");
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }
    }

    public class IpMask : IpRepresentation
    {
        public int MaskNumber { get; set; }
        

        public IpMask(byte[] ip) : base(ip)
        {
            for (int i = 0; i < 4; i++)
            {
                ip[i] = ReverseByte(ip[i]);
            }
            var num = CheckMask(new BitArray(ip));
            if (num == -1)
                throw new ArgumentException("Its not a  mask, you idiot");
            MaskNumber = num;
        }

        public static int CheckMask(BitArray b)
        {
            var j = 0;
            int i;
            for (i = 0; i < 32; i++)
            {
                if (!b[i])
                    break;
                j++;
            }

            if (i == 31)
                return j;

            for (; i < 32; i++)
            {
                if (b[i])
                    return -1;
            }
            return j;
        }

        protected byte ReverseByte(byte originalByte)
        {
            int result = 0;
            for (int i = 0; i < 8; i++)
            {
                result = result << 1;
                result += originalByte & 1;
                originalByte = (byte)(originalByte >> 1);
            }

            return (byte)result;
        }

        public override string ToString()
        {
            var ipOctets = GetAddressBytes();
            return string.Format("({4})  {0}.{1}.{2}.{3}", ipOctets[0], ipOctets[1], ipOctets[2], ipOctets[3], MaskNumber);
        }

        public int GetNetworkCapacity()
        {
            var d = Math.Pow(2, (32 - MaskNumber));
            return (int)d - 2;
        }
    }

    public class MyIp : IpRepresentation
    {

        public MyIp(byte[] ip) : base(ip)
        {
        }

        public IpRepresentation GetNetworkAdress(IpMask mask)
        {
            var network = new byte[4];
            var ipOctets = GetAddressBytes();
            var maskOctets = mask.GetAddressBytes();
            for (var i = 0; i < 4; i++)
            {
                network[i] = (byte)(ipOctets[i] & maskOctets[i]);
            }
            return new IpRepresentation(network);
        }

        public IpRepresentation GetBroadcastAdress(IpMask mask)
        {
            var network = new byte[4];
            var ipOctets = base.GetAddressBytes();
            var maskOctets = mask.GetAddressBytes();
            for (var i = 0; i < 4; i++)
            {
                network[i] = (byte)(ipOctets[i] | (255 - maskOctets[i]));
            }
            return new IpRepresentation(network);
        }

        public string GetIpClass()
        {
            var ipOctets = base.GetAddressBytes();
            if ((ipOctets[0] & 128) == 0)
                return "A";
            if ((ipOctets[0] & 192) == 128)
                return "B";
            if ((ipOctets[0] & 224) == 192)
                return "C";
            if ((ipOctets[0] & 240) == 224)
                return "D";
            return "E";
        }


        public static IEnumerable<string> GetLocalIpAddress()
        {
            var host = Dns.GetHostAddresses(Dns.GetHostName());
            return from ip in host
                   where ip.AddressFamily == AddressFamily.InterNetwork
                   select ip.ToString();
        }

        public static IEnumerable<PhysicalAddress> GetMacAddress()
        {
            return from nic in NetworkInterface.GetAllNetworkInterfaces()
                   where nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet && nic.Name == "Ethernet"
                   select nic.GetPhysicalAddress();
        }

        public static IpMask CountMaskFromNumber(string text)
        {
            if (text == string.Empty)
                throw new ArgumentException("No emtpy strings");
            var pcNumber = Convert.ToInt32(text);
            var maskNumber = Math.Log(pcNumber + 2, 2);
            if (Math.Floor(maskNumber) != maskNumber)
                maskNumber++;
            var count = 32 - (int)(maskNumber);
            var bytes = new byte[4];
            int i;
            for (i = 0; i < (int)(count / 8); i++)
            {
                bytes[i] = 255;
            }
            byte result = 0;
            for (int j = 0; j < count % 8; j++)
            {
                result += (byte)Math.Pow(2, (7 - j));
            }
            bytes[i] = result;
            return new IpMask(bytes);
        }
    }
}
