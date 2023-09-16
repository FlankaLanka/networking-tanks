using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Building building;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private LayerMask floorMask = new LayerMask();

    private Camera mainCamera;
    private BoxCollider buildingCollider;
    private RTSPlayer player;
    private GameObject buildingPreviewInstance;
    private Renderer buildingRendererInstance;

    public void Start()
    {
        mainCamera = Camera.main;
        buildingCollider = building.GetComponent<BoxCollider>();
        iconImage.sprite = building.GetIcon();
        priceText.text = building.GetPrice().ToString();
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
    }

    private void Update()
    {
        UpdateBuildingPreview();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (player.GetResources() < building.GetPrice())
            return;

        buildingPreviewInstance = Instantiate(building.GetBuildingPreview());
        buildingRendererInstance = buildingPreviewInstance.GetComponentInChildren<Renderer>();
        buildingPreviewInstance.SetActive(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (buildingPreviewInstance == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
        {
            player.CmdTryPlaceBuilding(building.GetId(), hit.point);
        }

        Destroy(buildingPreviewInstance);
    }

    public void UpdateBuildingPreview()
    {
        if (buildingPreviewInstance == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
            return;

        buildingPreviewInstance.transform.position = hit.point;
        if(!buildingPreviewInstance.activeSelf)
        {
            buildingPreviewInstance.SetActive(true);
        }

        Color color = player.CanPlaceBuilding(buildingCollider, hit.point) ? Color.green : Color.red;
        buildingRendererInstance.material.SetColor("_BaseColor", color);
    }
}
