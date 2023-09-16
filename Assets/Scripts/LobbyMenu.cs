using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject lobbyUI;
    [SerializeField] private Button startGameButton;
    [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[4];

    void Start()
    {
        RTSNetworkManager.ClientOnConnected += HandleClientConnected;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdated;
        RTSPlayer.ClientOnInfoUpdated += ClientHandleInfoUpdated;
    }

    private void OnDestroy()
    {
        RTSNetworkManager.ClientOnConnected -= HandleClientConnected;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated -= AuthorityHandlePartyOwnerStateUpdated;
        RTSPlayer.ClientOnInfoUpdated -= ClientHandleInfoUpdated;
    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool isPartyLeader)
    {
        startGameButton.gameObject.SetActive(isPartyLeader);
    }

    private void ClientHandleInfoUpdated()
    {
        List<RTSPlayer> players = ((RTSNetworkManager)NetworkManager.singleton).Players;
        for(int i = 0; i < players.Count; i++)
        {
            playerNameTexts[i].text = players[i].GetDisplayName();
        }
        for(int i = players.Count; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = "waiting for player...";
        }
        startGameButton.interactable = players.Count >= 2;
    }

    private void HandleClientConnected()
    {
        lobbyUI.SetActive(true);
    }

    public void StartGame()
    {
        NetworkClient.connection.identity.GetComponent<RTSPlayer>().CmdStartGame();
    }

    public void LeaveLobby()
    {
        if(NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
            //StopHost() automatically reloads scene
        }
        else
        {
            NetworkManager.singleton.StopClient();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
