using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Client : MonoBehaviour {
    public static Client instance;
    public static int dataBufferSize = 4096;

    public string ip = "127.0.0.1";
    public int port = 26950;
    public int myId;
    public TCP tcp;
    public UDP udp;

    private delegate void PacketHandler(Packet packet);

    private static Dictionary<int, PacketHandler> packetHandlers;

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(this);
        }
    }

    private void Start() {
        tcp = new TCP();
        udp = new UDP();
    }

    public void ConnectToServer() {
        InitializeClientData();
        tcp.Connect();
    }

    public class TCP {
        public TcpClient socket;
        private NetworkStream stream;
        private Packet receiveData;
        private byte[] receiveBuffer;

        public void Connect() {
            socket = new TcpClient {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult result) {
            socket.EndConnect(result);
            if (!socket.Connected) {
                return;
            }

            stream = socket.GetStream();
            receiveData = new Packet();
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        public void SendData(Packet packet) {
            try {
                if (socket != null) {
                    stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
            } catch (Exception e) {
                Debug.Log($"Error sending data to server via TCP: {e}");
            }
        }

        private void ReceiveCallback(IAsyncResult result) {
            try {
                int byteLength = stream.EndRead(result);
                if (byteLength <= 0) {
                    //TODO disconnect
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength);

                receiveData.Reset(HandleData(data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            } catch (Exception e) {
                Console.WriteLine($"Error reciving TCP data: {e}");
                //TODO disconnect 
            }
        }

        private bool HandleData(byte[] data) {
            int packetLenght = 0;
            receiveData.SetBytes(data);

            if (receiveData.UnreadLength() >= 4) {
                packetLenght = receiveData.ReadInt();
                if (packetLenght <= 0) {
                    return true;
                }
            }

            while (packetLenght > 0 && packetLenght <= receiveData.UnreadLength()) {
                byte[] packetBytes = receiveData.ReadBytes(packetLenght);
                ThreadManager.ExecuteOnMainThread(() => {
                    using (Packet packet = new Packet(packetBytes)) {
                        int packetId = packet.ReadInt();
                        packetHandlers[packetId](packet);
                    }
                });

                packetLenght = 0;
                if (receiveData.UnreadLength() >= 4) {
                    packetLenght = receiveData.ReadInt();
                    if (packetLenght <= 0) {
                        return true;
                    }
                }
            }

            if (packetLenght <= 1) {
                return true;
            }

            return false;
        }
    }

    public class UDP {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP() {
            endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        public void Connect(int localPort) {
            socket = new UdpClient(localPort);
            socket.Connect(endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            using (Packet packet = new Packet()) {
                SendData(packet);
            }
        }

        public void SendData(Packet packet) {
            try {
                packet.InsertInt(instance.myId);
                if (socket != null) {
                    socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
                }
            } catch (Exception e) {
                Debug.Log($"Error sending data to server via UDP: {e}");
            }
        }

        private void ReceiveCallback(IAsyncResult result) {
            try {
                byte[] data = socket.EndReceive(result, ref endPoint);
                if (data.Length < 4) {
                    //TODO disconnect
                    return;
                }

                HandleData(data);
            } catch (Exception e) {
                //TODO disconnect
            }
        }

        private void HandleData(byte[] data) {
            using (Packet packet = new Packet(data)) {
                int packetLength = packet.ReadInt();
                data = packet.ReadBytes(packetLength);
            }
            
            ThreadManager.ExecuteOnMainThread(() => {
                using (Packet packet = new Packet(data)) {
                    int packetId = packet.ReadInt();
                    packetHandlers[packetId](packet);
                }
            });
        }
    }

    private void InitializeClientData() {
        packetHandlers = new Dictionary<int, PacketHandler>() {
            {(int) ServerPackets.welcome, ClientHandle.Welcome},
            {(int) ServerPackets.udpTest, ClientHandle.UDPTest}
        };
        Debug.Log("Initialized packets.");
    }
}