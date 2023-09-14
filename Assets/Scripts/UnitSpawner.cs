using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private Health health;
    [SerializeField] private Unit unitPrefab;
    [SerializeField] private Transform unitSpawnPoint;
    [SerializeField] private TMP_Text remainingUnitsText;
    [SerializeField] private Image unitProgressImage;
    [SerializeField] private int maxUnitQueue = 5;
    [SerializeField] private float spawnMoveRange = 10f;
    [SerializeField] private float unitSpawnDuration = 5f;

    [SerializeField]
    [SyncVar(hook=nameof(ClientHandleQueuedUnitsUpdated))]
    private int queuedUnits;
    [SerializeField]
    [SyncVar()]
    private float unitTimer;

    private float progressImageVelocity;

    private void Update()
    {
        if(isServer)
        {
            ProduceUnits();
        }

        if(isClient)
        {
            UpdateTimerDisplay();
        }
    }

    #region Server

    public override void OnStartServer()
    {
        health.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
    }

    [Server]
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(this.gameObject);
    }

    [Command]
    private void CmdSpawnUnit()
    {
        if (queuedUnits == maxUnitQueue)
            return;

        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();
        if (player.GetResources() < unitPrefab.GetResourceCost())
            return;

        queuedUnits++;
        player.SetResources(player.GetResources() - unitPrefab.GetResourceCost());
    }

    [Server]
    private void ProduceUnits()
    {
        if (queuedUnits == 0)
            return;
        unitTimer += Time.deltaTime;
        if (unitTimer < unitSpawnDuration)
            return;

        GameObject unitInstance = Instantiate(unitPrefab.gameObject, unitSpawnPoint.position, unitSpawnPoint.rotation);
        NetworkServer.Spawn(unitInstance, connectionToClient);

        Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
        spawnOffset.y = unitSpawnPoint.position.y;
        UnitMovement unitMovement = unitInstance.GetComponent<UnitMovement>();
        unitMovement.ServerMove(unitSpawnPoint.position + spawnOffset);

        queuedUnits--;
        unitTimer = 0f;
    }

    #endregion
    

    #region Client

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || !isOwned)
            return;
        CmdSpawnUnit();
    }

    private void ClientHandleQueuedUnitsUpdated(int oldUnits, int newUnits)
    {
        remainingUnitsText.text = queuedUnits.ToString();
    }

    [ClientCallback]
    private void UpdateTimerDisplay()
    {
        float newProgress = unitTimer / unitSpawnDuration;

        if(newProgress < unitProgressImage.fillAmount)
        {
            unitProgressImage.fillAmount = newProgress;
        }
        else
        {
            unitProgressImage.fillAmount = Mathf.SmoothDamp(unitProgressImage.fillAmount, newProgress, ref progressImageVelocity, 0.1f);
        }
    }

    #endregion
}
