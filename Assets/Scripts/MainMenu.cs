using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject landingPagePanel;

    public void HostLobby()
    {
        landingPagePanel.SetActive(false);
        NetworkManager.singleton.StartHost();
    }
}
