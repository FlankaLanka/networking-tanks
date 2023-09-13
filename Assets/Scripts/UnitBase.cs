using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class UnitBase : NetworkBehaviour
{
    [SerializeField] private Health health;

    #region Server

    /*
    public override void OnStartServer()
    {
        health.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
    }

    public void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }
    
     */

    #endregion


    #region Client


    #endregion

}
