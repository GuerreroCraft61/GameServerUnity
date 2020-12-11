using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour {
    private static void SendTCPData(Packet packet) {
        packet.WriteLength();
        Client.instance.tcp.SendData(packet);
    }

    private static void SendUDPData(Packet packet) {
        packet.WriteLength();
        Client.instance.udp.SendData(packet);
    }

    public static void WelcomeReceived() {
        using (Packet packet = new Packet((int) ClientPackets.welcomeReceived)) {
            packet.Write(Client.instance.myId);
            packet.Write(UIManager.instance.userNameField.text);
            
            SendTCPData(packet);
        }
    }

    public static void UDPTestReceived() {
        using (Packet packet = new Packet((int) ClientPackets.udpTestReceived)) {
            packet.Write("Received UDP packet.");
            
            SendUDPData(packet);
        }
    }
}