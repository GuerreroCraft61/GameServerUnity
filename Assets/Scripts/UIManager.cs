using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    public static UIManager instance;

    public GameObject startMenu;
    public InputField userNameField;

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(this);
        }
    }

    public void ConnectToServer() {
        startMenu.SetActive(false);
        userNameField.interactable = false;
        Client.instance.ConnectToServer();
    }
}