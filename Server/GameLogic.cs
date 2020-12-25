namespace Server {
    public class GameLogic {
        public static void Update() {
            foreach (Client client in Server.clients.Values) {
                client.player?.Update();
            }
            ThreadManager.UpdateMain();
        }
    }
}