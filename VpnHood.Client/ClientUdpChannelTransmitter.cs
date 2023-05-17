﻿using System.Net;
using System.Net.Sockets;
using VpnHood.Tunneling;

namespace VpnHood.Client;

public class ClientUdpChannelTransmitter : UdpChannelTransmitter
{
    private readonly UdpChannel2 _udpChannel;

    public ClientUdpChannelTransmitter(UdpChannel2 udpChannel, UdpClient udpClient, IPEndPoint serverEndPoint, byte[] serverKey) 
        : base(udpClient, serverKey)
    {
        _udpChannel = udpChannel;
        udpChannel.SetRemote(this, serverEndPoint);
    }

    protected override void OnReceiveData(ulong sessionId, IPEndPoint remoteEndPoint, long channelCryptorPosition, byte[] buffer, int bufferIndex)
    {
        _udpChannel.OnReceiveData(channelCryptorPosition, buffer, bufferIndex);
    }
}