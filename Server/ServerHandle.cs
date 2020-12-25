using System;
using System.Numerics;

namespace Server {
    public class ServerHandle {
        public static void WelcomeReceived(int fromClient, Packet packet) {
            var clientIdCheck = packet.ReadInt();
            var username = packet.ReadString();

            Console.WriteLine($"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} " +
                              $"connected successfully and is now player {fromClient}");
            if (fromClient != clientIdCheck) {
                Console.WriteLine($"Player \"{username}\" (ID: {fromClient}) " +
                                  $"has assumed the wrong client ID ({clientIdCheck})");
            }
            Server.clients[fromClient].SendIntoGame(username);
        }

        public static void PlayerMovement(int fromClient, Packet packet) {
            bool[] inputs = new bool[packet.ReadInt()];
            for (int i = 0; i < inputs.Length; i++) {
                inputs[i] = packet.ReadBool();
            }
            Quaternion rotation = packet.ReadQuaternion();

            Server.clients[fromClient].player.SetInput(inputs, rotation);
            /*Console.WriteLine($"Input: W({inputs[0]}) S({inputs[1]}) A({inputs[2]}) D({inputs[3]})\n" +
                              $"Quaternion: X({rotation.X}) Y({rotation.Y}) Z({rotation.Z}) W({rotation.W})");*/
        }
    }
}