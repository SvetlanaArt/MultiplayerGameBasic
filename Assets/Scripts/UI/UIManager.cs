using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI messageText;
    [SerializeField] MessageText messageTextValue;
    [SerializeField] Button joinButton;
    [SerializeField] TextMeshProUGUI playerName;
    [SerializeField] LobbyManager joinManager;
    [SerializeField] List<GameObject> managers;


    void Awake()
    {
        foreach (GameObject manager in managers)
        {
            IConnection connectionEventManager = manager.GetComponent<IConnection>();
            if (connectionEventManager != null)
                connectionEventManager.OnConnectionEvent += ShowMessage;
        }
           
        joinButton.onClick.AddListener(joinManager.QuickJoinGame);
        joinManager.OnPlayerNameGanged += SetPlayerName;
    }

    private void ShowMessage(GameMessageType type)
    {
        messageText.text = messageTextValue.GetMessage(type);
    }

    private void SetPlayerName(string name)
    {
        playerName.text = name;
    }
}

