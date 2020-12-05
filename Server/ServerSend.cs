namespace Server {
    public class ServerSend {
        public static void SendTCPData(int toClient, Packet packet) {
            packet.WriteLength();
            Server.clients[toClient].tcp.SendData(packet);
        }

        public static void Welcome(int toClient, string msg) {
            using (var packet = new Packet((int) ServerPackets.welcome)) {
                packet.Write(msg);
                packet.Write(toClient);

                SendTCPData(toClient, packet);
            }
        }
    }
}