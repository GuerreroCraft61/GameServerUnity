using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Server {
    public class Server {
        public static int maxPlayers { get; private set; }
        public static int port { get; private set; }
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        public delegate void PacketHandler(int fromClient, Packet packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        private static TcpListener tcpListener;
        private static UdpClient udpListener;
        
        public static void Start(int maxPlayers, int port) {
            Server.maxPlayers = maxPlayers;
            Server.port = port;

            Console.WriteLine("Server starting...");
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Server.port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            
            udpListener = new UdpClient(Server.port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            Console.WriteLine($"Server started on {Server.port}.");
        }

        private static void TCPConnectCallback(IAsyncResult result) {
            var client = tcpListener.EndAcceptTcpClient(result);
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}...");

            for (var i = 1; i < maxPlayers; i++) {
                if (clients[i].tcp.socket == null) {
                    clients[i].tcp.Connect(client);
                    return;
                }
            }

            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: Server full");
        }

        private static void UDPReceiveCallback(IAsyncResult result) {
            try {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpListener.EndReceive(result, ref clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (data.Length < 4) {
                    return;
                }

                using (Packet packet = new Packet(data)) {
                    int clientId = packet.ReadInt();
                    if (clientId == 0) {
                        return;
                    }
                    Client.UDP udp = clients[clientId].udp;
                    if (udp.endPoint == null) {
                        udp.Connect(clientEndPoint);
                        return;
                    }
                    if (udp.endPoint.ToString() == clientEndPoint.ToString()) {
                        udp.HandleData(packet);
                    }
                }
            } catch (Exception e) {
                Console.WriteLine($"Error receiving UDP data: {e}");
            }
        }

        public static void SendUDPData(IPEndPoint clientEndPoint, Packet packet) {
            try {
                if (clientEndPoint != null) {
                    udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
                }
            } catch (Exception e) {
                Console.WriteLine($"Error sending data to {clientEndPoint} via UDP: {e}");
            }
        }

        private static void InitializeServerData() {
            for (var i = 1; i <= maxPlayers; i++) clients.Add(i, new Client(i));

            packetHandlers = new Dictionary<int, PacketHandler> {
                {(int) ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived},
                {(int) ClientPackets.playerMovement, ServerHandle.PlayerMovement}
            };
            Console.WriteLine("Initialized packets");
        }
    }
}