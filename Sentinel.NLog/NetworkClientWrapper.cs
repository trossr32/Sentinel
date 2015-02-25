namespace Sentinel.NLog
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class NetworkClientWrapper : IDisposable
    {
        private readonly bool isUdp;

        private readonly UdpClient udpClient;

        private readonly TcpClient tcpClient;

        public NetworkClientWrapper(NetworkProtocol protocol, IPEndPoint endPoint)
        {
            isUdp = protocol == NetworkProtocol.Udp;
            if (isUdp)
            {
                udpClient = new UdpClient();
                udpClient.ExclusiveAddressUse = false;
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpClient.Client.Bind(endPoint);
            }
            else
            {
                tcpClient = new TcpClient(endPoint);
            }
        }

        public byte[] Receive(ref IPEndPoint remoteEndPoint)
        {
            if (isUdp)
            {
                return udpClient.Receive(ref remoteEndPoint);
            }

            var returnBuffer = new List<byte>();
            var stream = tcpClient.GetStream();
            var buffer = new byte[10240];

            int i;
            while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                returnBuffer.AddRange(buffer.Take(i));
            }

            return returnBuffer.ToArray();
        }

        public void Dispose()
        {
            if (udpClient != null) udpClient.Close();
            if (tcpClient != null) tcpClient.Close();
        }

        public void SetRecieveTimeout(int timeout)
        {
            if (isUdp)
            {
                udpClient.Client.ReceiveTimeout = 1000;
            }
            else
            {
                tcpClient.Client.ReceiveTimeout = 1000;
            }
        }
    }
}