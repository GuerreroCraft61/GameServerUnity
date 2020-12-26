using System;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace Server {
    public class Client {
        public static int dataBufferSize = 4096;

        public int id;
        public Player player;
        public TCP tcp;
        public UDP udp;

        public Client(int clientId) {
            id = clientId;
            tcp = new TCP(id);
            udp = new UDP(id);
        }

        public class TCP {
            public TcpClient socket;
            
            private readonly int id;
            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;
            
            public TCP(int id) {
                this.id = id;
            }

            public void Connect(TcpClient socket) {
                this.socket = socket;
                this.socket.ReceiveBufferSize = dataBufferSize;
                this.socket.SendBufferSize = dataBufferSize;

                stream = this.socket.GetStream();
                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                ServerSend.Welcome(id, "Welcome to the server!");
            }

            public void SendData(Packet packet) {
                try {
                    if (socket != null) {
                        stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                } catch (Exception e) {
                    Console.WriteLine($"Error sending data to player {id} via TCP: {e}");
                }
            }

            private void ReceiveCallback(IAsyncResult result) {
                try {
                    var byteLength = stream.EndRead(result);
                    if (byteLength <= 0) {
                        Server.clients[id].Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);

                    receivedData.Reset(HandleData(data));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                } catch (Exception e) {
                    Console.WriteLine($"Error reciving TCP data: {e}");
                    Server.clients[id].Disconnect(); 
                }
            }

            private bool HandleData(byte[] data) {
                var packetLenght = 0;
                
                receivedData.SetBytes(data);

                if (receivedData.UnreadLength() >= 4) {
                    packetLenght = receivedData.ReadInt();
                    if (packetLenght <= 0) return true;
                }

                while (packetLenght > 0 && packetLenght <= receivedData.UnreadLength()) {
                    byte[] packetBytes = receivedData.ReadBytes(packetLenght);
                    ThreadManager.ExecuteOnMainThread(() => {
                        using (Packet packet = new Packet(packetBytes)) {
                            int packetId = packet.ReadInt();
                            Server.packetHandlers[packetId](id, packet);
                        }
                    });

                    packetLenght = 0;
                    if (receivedData.UnreadLength() >= 4) {
                        packetLenght = receivedData.ReadInt();
                        if (packetLenght <= 0) return true;
                    }
                }

                if (packetLenght <= 1) return true;

                return false;
            }

            public void Disconnect() {
                socket.Close();
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }

        public class UDP {
            public IPEndPoint endPoint;

            private int id;

            public UDP(int id) {
                this.id = id;
            }

            public void Connect(IPEndPoint endPoint) {
                this.endPoint = endPoint;
            }

            public void SendData(Packet packet) {
                Server.SendUDPData(endPoint, packet);
            }

            public void HandleData(Packet packetData) {
                int packetLength = packetData.ReadInt();
                byte[] packetBytes = packetData.ReadBytes(packetLength);

                ThreadManager.ExecuteOnMainThread(() => {
                    using (Packet packet = new Packet(packetBytes)) {
                        int packetId = packet.ReadInt();
                        Server.packetHandlers[packetId](id, packet);
                    }
                });
            }

            public void Disconnect() {
                endPoint = null;
            }
        }

        public void SendIntoGame(string playerName) {
            player = new Player(id, playerName, new Vector3(0, 0, 0));

            foreach (Client client in Server.clients.Values) {
                if (client.player != null) {
                    if (client.id != id) {
                        ServerSend.SpawnPlayer(id, client.player);
                    }
                }
            }

            foreach (Client client in Server.clients.Values) {
                if (client.player != null) {
                    ServerSend.SpawnPlayer(client.id, player);
                }
            }
        }

        private void Disconnect() {
            Console.WriteLine($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");

            player = null;
            
            tcp.Disconnect();
            udp.Disconnect();
        }
    }
}