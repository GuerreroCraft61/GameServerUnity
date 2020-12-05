using System;

namespace Server {
    public class ServerHandle {
        public static void WelcomeReceived(int fromClient, Packet packet) {
            var clientIdCheck = packet.ReadInt();
            var username = packet.ReadString();

            Console.WriteLine(
                $"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {fromClient}");
            if (fromClient != clientIdCheck)
                Console.WriteLine(
                    $"Player \"{username}\" (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})");
            //TODO Send player into game
        }
    }
}