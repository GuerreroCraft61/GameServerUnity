using System;
using System.Net.Sockets;

namespace Server {
    public class Client {
        public static int dataBufferSize = 4096;
        public int id;
        public TCP tcp;

        public Client(int clientId) {
            id = clientId;
            tcp = new TCP(id);
        }

        public class TCP {
            private readonly int id;
            private byte[] receiveBuffer;
            private Packet receiveData;
            public TcpClient socket;
            private NetworkStream stream;

            public TCP(int id) {
                this.id = id;
            }

            public void Connect(TcpClient socket) {
                this.socket = socket;
                this.socket.ReceiveBufferSize = dataBufferSize;
                this.socket.SendBufferSize = dataBufferSize;

                stream = this.socket.GetStream();
                receiveData = new Packet();
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
                    throw;
                }
            }

            private void ReceiveCallback(IAsyncResult result) {
                try {
                    var byteLength = stream.EndRead(result);
                    if (byteLength <= 0) //TODO disconnect
                        return;

                    var data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);

                    receiveData.Reset(HandleData(data));
                } catch (Exception e) {
                    Console.WriteLine($"Error reciving TCP data: {e}");
                    //TODO disconnect 
                }
            }

            private bool HandleData(byte[] data) {
                var packetLenght = 0;
                receiveData.SetBytes(data);

                if (receiveData.UnreadLength() >= 4) {
                    packetLenght = receiveData.ReadInt();
                    if (packetLenght <= 0) return true;
                }

                while (packetLenght > 0 && packetLenght <= receiveData.UnreadLength()) {
                    var packetBytes = receiveData.ReadBytes(packetLenght);
                    ThreadManager.ExecuteOnMainThread(() => {
                        using (var packet = new Packet(packetBytes)) {
                            var packetId = packet.ReadInt();
                            Server.packetHandlers[packetId](id, packet);
                        }
                    });

                    packetLenght = 0;
                    if (receiveData.UnreadLength() >= 4) {
                        packetLenght = receiveData.ReadInt();
                        if (packetLenght <= 0) return true;
                    }
                }

                if (packetLenght <= 1) return true;

                return false;
            }
        }
    }
}