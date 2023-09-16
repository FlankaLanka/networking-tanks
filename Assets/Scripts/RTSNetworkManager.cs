using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.SceneManagement;

public class RTSNetworkManager : NetworkManager
{
    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;

    public List<RTSPlayer> Players { get; } = new List<RTSPlayer>();
    private bool isGameInProgress = false;

    [SerializeField] private GameObject unitBasePrefab;
    [SerializeField] private GameOverHandler gameOverHandlerPrefab;


    #region Server

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        if (!isGameInProgress)
            return;
        conn.Disconnect();
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();
        Players.Remove(player);
        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        Players.Clear();
        isGameInProgress = false;
    }

    public void StartGame()
    {
        isGameInProgress = true;
        ServerChangeScene("Scene_Map_01");
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();
        Players.Add(player);
        player.SetDisplayName($"Player {Players.Count}");
        player.SetTeamColor(new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1.0f));
        player.SetPartyOwner(Players.Count == 1);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (SceneManager.GetActiveScene().name.StartsWith("Scene_Map"))
        {
            GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);
            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);

            foreach(RTSPlayer player in Players)
            {
                GameObject baseInstance = Instantiate(unitBasePrefab, GetStartPosition().position, Quaternion.identity);
                NetworkServer.Spawn(baseInstance, player.connectionToClient);
            }
        }
    }

    #endregion


    #region Client

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        ClientOnConnected?.Invoke();
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        ClientOnDisconnected?.Invoke();
    }

    public override void OnStopClient()
    {
        Players.Clear();
    }

    #endregion
}
