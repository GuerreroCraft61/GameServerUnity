namespace Server {
    public class ServerSend {
        private static void SendTCPData(int toClient, Packet packet) {
            packet.WriteLength();
            Server.clients[toClient].tcp.SendData(packet);
        }

        private static void SendUDPData(int toClient, Packet packet) {
            packet.WriteLength();
            Server.clients[toClient].udp.SendData(packet);
        }

        private static void SendTCPDataToAll(Packet _packet) {
            _packet.WriteLength();
            for (int i = 1; i <= Server.maxPlayers; i++) {
                Server.clients[i].tcp.SendData(_packet);
            }
        }

        private static void SendTCPDataToAll(int _exceptClient, Packet _packet) {
            _packet.WriteLength();
            for (int i = 1; i <= Server.maxPlayers; i++) {
                if (i != _exceptClient) {
                    Server.clients[i].tcp.SendData(_packet);
                }
            }
        }

        private static void SendUDPDataToAll(Packet _packet) {
            _packet.WriteLength();
            for (int i = 1; i <= Server.maxPlayers; i++) {
                Server.clients[i].udp.SendData(_packet);
            }
        }

        private static void SendUDPDataToAll(int _exceptClient, Packet _packet) {
            _packet.WriteLength();
            for (int i = 1; i <= Server.maxPlayers; i++) {
                if (i != _exceptClient) {
                    Server.clients[i].udp.SendData(_packet);
                }
            }
        }

        public static void Welcome(int toClient, string msg) {
            using (var packet = new Packet((int) ServerPackets.welcome)) {
                packet.Write(msg);
                packet.Write(toClient);

                SendTCPData(toClient, packet);
            }
        }

        public static void UDPTest(int toClient) {
            using (Packet packet = new Packet((int)ServerPackets.udpTest)) {
                packet.Write("A test packet for UDP.");
                
                SendUDPData(toClient, packet);
            }
        }
    }
}