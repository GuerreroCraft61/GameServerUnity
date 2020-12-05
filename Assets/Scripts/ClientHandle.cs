using UnityEngine;
using UnityEngine.UI;

public class ClientHandle : MonoBehaviour {

	public static void Welcome(Packet packet) {
		string msg = packet.ReadString();
		int myId = packet.ReadInt();
		
		Debug.Log($"Message from server: {msg}");
		Client.instance.myId = myId;
		ClientSend.WelcomeReceived();
	}
}
