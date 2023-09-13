using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField]
    private List<Unit> myUnits = new List<Unit>();
    public List<Unit> GetMyUnits()
    {
        return myUnits;
    }

    #region Server

    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
    }

    private void ServerHandleUnitSpawned(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId)
            return;
        myUnits.Add(unit);
    }

    private void ServerHandleUnitDespawned(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId)
            return;
        myUnits.Remove(unit);
    }

    #endregion


    #region Client

    public override void OnStartClient()
    {
        Unit.AuthorityOnUnitSpawned += ClientHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned += ClientHandleUnitDespawned;
    }
    public override void OnStopClient()
    {
        Unit.AuthorityOnUnitSpawned -= ClientHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= ClientHandleUnitDespawned;
    }

    private void ClientHandleUnitSpawned(Unit unit)
    {
        if (!isOwned)
            return;
        myUnits.Add(unit);
    }

    private void ClientHandleUnitDespawned(Unit unit)
    {
        if (!isOwned)
            return;
        myUnits.Remove(unit);
    }

    #endregion
}
