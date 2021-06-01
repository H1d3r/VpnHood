﻿using Microsoft.Extensions.Logging;
using PacketDotNet;
using PacketDotNet.Utils;
using System;
using System.Net;
using VpnHood.Logging;

namespace VpnHood.Tunneling
{
    public static class TunnelUtil
    {
        public const int SocketStackSize_Datagram = 65536;
        public const int SocketStackSize_Stream = 65536 * 2;
        public const int TlsHandshakeLength = 5000;

        public static void UpdateICMPChecksum(IcmpV4Packet icmpPacket)
        {
            icmpPacket.Checksum = 0;
            var buf = icmpPacket.Bytes;
            icmpPacket.Checksum = (ushort)ChecksumUtils.OnesComplementSum(buf, 0, buf.Length);
        }
        
        public static ulong RandomLong()
        {
            var random = new Random();
            byte[] bytes = new byte[8];
            random.NextBytes(bytes);
            return BitConverter.ToUInt64(bytes, 0);
        }

        public static int RandomInt()
        {
            var random = new Random();
            return random.Next();
        }

        public static IPPacket ReadNextPacket(byte[] buffer, ref int bufferIndex)
        {
            var packetLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, bufferIndex + 2));
            if (packetLength < IPv4Packet.HeaderMinimumLength)
                throw new Exception($"A packet with invalid length has been received! Length: {packetLength}");

            var segment = new ByteArraySegment(buffer, bufferIndex, packetLength);
            bufferIndex += packetLength;
            return new IPv4Packet(segment);
        }

        public static void LogPackets(IPPacket[] ipPackets, string operation)
        {
            foreach (var ipPacket in ipPackets)
                LogPacket(ipPacket, operation);
        }

        public static void LogPacket(IPPacket ipPacket, string operation)
        {
            // log ICMP
            if (VhLogger.IsDiagnoseMode && ipPacket.Protocol == ProtocolType.Icmp)
            {
                var icmpPacket = ipPacket.Extract<IcmpV4Packet>();
                var payload = icmpPacket.PayloadData ?? Array.Empty<byte>();
                VhLogger.Current.Log(LogLevel.Information, GeneralEventId.Ping, $"ICMP had been {operation}. DestAddress: {ipPacket.DestinationAddress}, DataLen: {payload.Length}, Data: {BitConverter.ToString(payload, 0, Math.Min(10, payload.Length))}.");
            }

            // log Udp
            if (VhLogger.IsDiagnoseMode && ipPacket.Protocol == ProtocolType.Udp)
            {
                var udp = ipPacket.Extract<UdpPacket>();
                var payload = udp.PayloadData ?? Array.Empty<byte>();
                VhLogger.Current.Log(LogLevel.Information, GeneralEventId.Udp, $"UDP had been {operation}. DestAddress: {ipPacket.DestinationAddress}:{udp.DestinationPort}, DataLen: {payload.Length}, Data: {BitConverter.ToString(payload, 0, Math.Min(10, payload.Length))}.");
            }
        }

    }
}
