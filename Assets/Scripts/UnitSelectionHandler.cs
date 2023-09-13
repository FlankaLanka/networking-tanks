using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionHandler : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask = new LayerMask();
    private Camera mainCamera;
    public List<Unit> selectedUnits { get; } = new List<Unit>();
    
    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            /*
            foreach (Unit selectedUnit in selectedUnits)
            {
                selectedUnit.Deselect();
            }
            selectedUnits.Clear();
            */
        }
        else if(Mouse.current.leftButton.wasReleasedThisFrame)
        {
            RemoveCurrentSelections();
            ClearSelectionArea();
        }

    }

    private void RemoveCurrentSelections()
    {
        foreach(Unit selectedUnit in selectedUnits)
        {
            selectedUnit.Deselect();
        }
        selectedUnits.Clear();
    }    

    private void ClearSelectionArea()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            return;

        if (!hit.collider.TryGetComponent<Unit>(out Unit unit) || !unit.isOwned)
            return;

        selectedUnits.Add(unit);
        foreach(Unit selectedUnit in selectedUnits)
        {
            selectedUnit.Select();
        }
    }
}
