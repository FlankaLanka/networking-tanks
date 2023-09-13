using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class UnitSelectionHandler : MonoBehaviour
{
    [SerializeField] private RectTransform unitSelectionArea;
    private Vector2 startPosition;
    private RTSPlayer player;

    [SerializeField] private LayerMask layerMask = new LayerMask();
    private Camera mainCamera;
    public List<Unit> selectedUnits { get; } = new List<Unit>();

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if(player == null)
        {
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartSelectionArea();
        }
        else if(Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ClearSelectionArea();
        }
        else if(Mouse.current.leftButton.isPressed)
        {
            UpdateSelectionArea();
        }
    }

    private void StartSelectionArea()
    {
        if(!Keyboard.current.leftShiftKey.isPressed)
        {
            foreach (Unit selectedUnit in selectedUnits)
            {
                selectedUnit.Deselect();
            }
            selectedUnits.Clear();
        }

        unitSelectionArea.gameObject.SetActive(true);
        startPosition = Mouse.current.position.ReadValue();
        UpdateSelectionArea();
    }

    private void UpdateSelectionArea()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        float areaWidth = mousePosition.x - startPosition.x;
        float areaHeight = mousePosition.y - startPosition.y;

        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        unitSelectionArea.anchoredPosition = startPosition + new Vector2(areaWidth / 2, areaHeight / 2);
    }

    private void ClearSelectionArea()
    {
        unitSelectionArea.gameObject.SetActive(false);

        //click on unit to select 1
        if(unitSelectionArea.sizeDelta.magnitude == 0)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
                return;

            if (!hit.collider.TryGetComponent<Unit>(out Unit unit) || !unit.isOwned)
                return;

            selectedUnits.Add(unit);
            foreach (Unit selectedUnit in selectedUnits)
            {
                selectedUnit.Select();
            }

            return;
        }

        //drag to select multiple units
        Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);
        Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

        foreach(Unit unit in player.GetMyUnits())
        {
            if (selectedUnits.Contains(unit))
                continue;

            Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);
            if(screenPosition.x > min.x && screenPosition.x < max.x && screenPosition.y > min.y && screenPosition.y < max.y)
            {
                selectedUnits.Add(unit);
                unit.Select();
            }
        }
    }
}
