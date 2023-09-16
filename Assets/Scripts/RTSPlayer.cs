using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private Transform cameraTransform;
    public Transform GetCameraTransform() => cameraTransform;

    [SyncVar(hook=nameof(ClientHandleResourcesUpdated))]
    private int resources = 500;
    public event Action<int> ClientOnResourcesUpdated;

    [SerializeField] private LayerMask buildingBlockLayer = new LayerMask();
    [SerializeField] private Building[] buildings = new Building[0];
    [SerializeField] private float buildingRangeLimit;
    private List<Building> myBuildings = new List<Building>();
    private List<Unit> myUnits = new List<Unit>();
    private Color teamColor = new Color();

    [SyncVar(hook = nameof(AuthorityHandlePartyOwnerStateUpdated))]
    private bool isPartyOwner = false;
    public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;

    [SyncVar(hook=nameof(ClientHandleDisplayNameUpdated))]
    private string displayName;
    public static event Action ClientOnInfoUpdated;

    public List<Building> GetMyBuildings() => myBuildings;
    public List<Unit> GetMyUnits() => myUnits;
    public int GetResources() => resources;
    public Color GetTeamColor() => teamColor;
    public bool GetIsPartyOwner() => isPartyOwner;
    public string GetDisplayName() => displayName;

    #region Server
    
    [Server]
    public void SetDisplayName(string newName)
    {
        displayName = newName;
    }

    [Server]
    public void SetPartyOwner(bool state)
    {
        isPartyOwner = state;
    }

    [Server]
    public void SetResources(int newResources)
    {
        resources = newResources;
    }

    [Server]
    public void SetTeamColor(Color color)
    {
        teamColor = color;
    }

    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned += ServerHandleBuildingDespawned;

        DontDestroyOnLoad(gameObject);
    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned -= ServerHandleBuildingDespawned;
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

    private void ServerHandleBuildingSpawned(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId)
            return;
        myBuildings.Add(building);
    }

    private void ServerHandleBuildingDespawned(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId)
            return;
        myBuildings.Remove(building);
    }

    [Command]
    public void CmdStartGame()
    {
        if (!isPartyOwner)
            return;
        ((RTSNetworkManager)NetworkManager.singleton).StartGame();
    }

    [Command]
    public void CmdTryPlaceBuilding(int buildingId, Vector3 point)
    {
        Building buildingToPlace = null;
        foreach(Building building in buildings)
        {
            if(building.GetId() == buildingId)
            {
                buildingToPlace = building;
                break;
            }
        }

        if (buildingToPlace == null)
            return;

        BoxCollider buildingToPlaceCollider = buildingToPlace.GetComponent<BoxCollider>();
        if (!CanPlaceBuilding(buildingToPlaceCollider, point) || resources < buildingToPlace.GetPrice())
            return;

        GameObject buildingInstance = Instantiate(buildingToPlace.gameObject, point, buildingToPlace.transform.rotation);
        NetworkServer.Spawn(buildingInstance, connectionToClient);

        SetResources(resources - buildingToPlace.GetPrice());
    }
    public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 point)
    {
        //if colliding with another building
        if (Physics.CheckBox(point + buildingCollider.center, buildingCollider.size / 2, Quaternion.identity, buildingBlockLayer))
            return false;

        foreach (Building building in myBuildings)
        {
            if ((point - building.transform.position).sqrMagnitude <= buildingRangeLimit * buildingRangeLimit)
                return true;
        }
        return false;
    }

    #endregion


    #region Client

    public override void OnStartClient()
    {
        if (NetworkServer.active)
            return;
        ((RTSNetworkManager)NetworkManager.singleton).Players.Add(this);
        DontDestroyOnLoad(gameObject);
    }

    public override void OnStartAuthority()
    {
        if (NetworkServer.active)
            return;
        Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
        Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned += AuthorityHandleBuildingDespawned;
    }
    public override void OnStopClient()
    {
        if(!isClientOnly)
        {
            ClientOnInfoUpdated?.Invoke();
            return;
        }
        else
        {
            ((RTSNetworkManager)NetworkManager.singleton).Players.Remove(this);
            ClientOnInfoUpdated?.Invoke();
        }

        if (!isOwned)
            return;
        Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
        Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
    }

    private void AuthorityHandleUnitSpawned(Unit unit)
    {
        myUnits.Add(unit);
    }

    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        myUnits.Remove(unit);
    }

    private void AuthorityHandleBuildingSpawned(Building building)
    {
        myBuildings.Add(building);
    }
    private void AuthorityHandleBuildingDespawned(Building building)
    {
        myBuildings.Remove(building);
    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool oldState, bool newState)
    {
        if (!isOwned)
            return;
        AuthorityOnPartyOwnerStateUpdated?.Invoke(newState);
    }

    private void ClientHandleResourcesUpdated(int oldResources, int newResources)
    {
        ClientOnResourcesUpdated?.Invoke(newResources);
    }

    private void ClientHandleDisplayNameUpdated(string oldDisplayName, string newDisplayName)
    {
        ClientOnInfoUpdated?.Invoke();
    }

    #endregion
}
