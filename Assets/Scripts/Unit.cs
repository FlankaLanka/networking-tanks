using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class Unit : NetworkBehaviour
{

    [SerializeField] private UnityEvent onSelected;
    [SerializeField] private UnityEvent onDeselected;

    [SerializeField] private UnitMovement unitMovement;
    public UnitMovement GetUnitMovement()
    {
        return unitMovement;
    }

    #region Client

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
