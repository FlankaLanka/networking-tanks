using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
using System;

public class Unit : NetworkBehaviour
{
    [SerializeField] private UnityEvent onSelected;
    [SerializeField] private UnityEvent onDeselected;

    public static event Action<Unit> ServerOnUnitSpawned;
    public static event Action<Unit> ServerOnUnitDespawned;

    public static event Action<Unit> AuthorityOnUnitSpawned;
    public static event Action<Unit> AuthorityOnUnitDespawned;

    [SerializeField] private UnitMovement unitMovement;
    public UnitMovement GetUnitMovement()
    {
        return unitMovement;
    }

    #region Server
    public override void OnStartServer()
    {
        ServerOnUnitSpawned?.Invoke(this);
    }

    public override void OnStopServer()
    {
        ServerOnUnitSpawned?.Invoke(this);
    }

    #endregion


    #region Client

    public override void OnStartClient()
    {
        if (!isClientOnly || !isOwned)
            return;
        AuthorityOnUnitSpawned?.Invoke(this);
    }

    public override void OnStopClient()
    {
        if (!isClientOnly || !isOwned)
            return;
        AuthorityOnUnitDespawned?.Invoke(this);
    }

    [Client]
    public void Select()
    {
        if (!isOwned)
            return;
        onSelected?.Invoke();
    }

    [Client]
    public void Deselect()
    {
        if (!isOwned)
            return;
        onDeselected?.Invoke();
    }

    #endregion
}
