using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Server {
    public class Server {
        public delegate void PacketHandler(int fromClient, Packet packet);

        private static TcpListener tcpListener;
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

        public static Dictionary<int, PacketHandler> packetHandlers;

        public static int maxPlayers { get; private set; }
        public static int port { get; private set; }


        public static void Start(int maxPlayers, int port) {
            Server.maxPlayers = maxPlayers;
            Server.port = port;

            Console.WriteLine("Server starting...");
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Server.port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            Console.WriteLine($"Server started on {Server.port}.");
        }

        private static void TCPConnectCallback(IAsyncResult result) {
            var client = tcpListener.EndAcceptTcpClient(result);
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}...");

            for (var i = 1; i < maxPlayers; i++)
                if (clients[i].tcp.socket == null) {
                    clients[i].tcp.Connect(client);
                    return;
                }

            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: Server full");
        }

        private static void InitializeServerData() {
            for (var i = 1; i < maxPlayers; i++) clients.Add(i, new Client(i));

            packetHandlers = new Dictionary<int, PacketHandler> {
                {(int) ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived}
            };
            Console.WriteLine("Initialized packets");
        }
    }
}