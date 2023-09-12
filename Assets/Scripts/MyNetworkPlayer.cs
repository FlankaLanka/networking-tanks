using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class MyNetworkPlayer : NetworkBehaviour
{
    [SerializeField]
    private TMP_Text displayNameText = null;
    [SerializeField]
    private Renderer displayColorRenderer = null;

    [SyncVar(hook=nameof(HandleDisplayNameUpdated))]
    [SerializeField]
    private string displayName = "Missing Name!!";

    [SyncVar(hook=nameof(HandleDisplayColorUpdated))]
    [SerializeField]
    private Color playerColor = Color.black;

    #region Server

    [Server]
    public void SetDisplayName(string newDisplayName)
    {
        displayName = newDisplayName;
    }

    [Server]
    public void SetColor(Color newColorSet)
    {
        playerColor = newColorSet;
    }

    [Command]
    private void CmdSetDisplayName(string newDisplayName)
    {
        if (!ValidationCheck(newDisplayName))
            return;

        RpcLogNewName(newDisplayName);
        SetDisplayName(newDisplayName);
    }

    private bool ValidationCheck(string validName)
    {
        return validName.Length > 2;
    }

    #endregion

    #region Client

    private void HandleDisplayColorUpdated(Color oldColor, Color newColor)
    {
        displayColorRenderer.material.SetColor("_BaseColor", newColor);
    }

    private void HandleDisplayNameUpdated(string oldName, string newName)
    {
        displayNameText.text = newName;
    }


    public string x = "My New Name";
    [ContextMenu("Set My Name")]
    public void SetMyName()
    {
        CmdSetDisplayName(x);
    }

    [ClientRpc]
    public void RpcLogNewName(string newDisplayName)
    {
        Debug.Log(newDisplayName);
    }

    #endregion
}
