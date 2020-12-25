using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;

    private void Awake() {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this);
    }

    public void SpawnPlayer(int id, string username, Vector3 position, Quaternion rotation) {
        GameObject player = Instantiate(id == Client.instance.myId ? localPlayerPrefab : playerPrefab,
            position, rotation);
        PlayerManager playerManager = player.GetComponent<PlayerManager>();
        playerManager.id = id;
        playerManager.username = username;
        players.Add(id, playerManager);
    }
}