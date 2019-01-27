using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VRTK;

public class CeilingTrapPickup : VRTK_InteractableObject
{
    public static CeilingTrapPickup ActivePickup = null;

    public int        Quanity = 1;
    public string     ActivationObjectTag;
    public GameObject CeilingTrapPrefab;

    [System.NonSerialized]
    public GameObject CeilingPlacementObject;

    private bool _placementValid;

    // Use this for initialization
    void Start()
    {
        CeilingPlacementObject = Instantiate(CeilingTrapPrefab);
        CeilingPlacementObject.GetComponent<BoxCollider>().enabled = false; // Disable the placement collider
        TogglePlacementVisible(false);
    }

    public void TogglePlacementVisible(bool visible)
    {
        MeshRenderer[] meshRenderers = CeilingPlacementObject.GetComponentsInChildren<MeshRenderer>(true);
        foreach (MeshRenderer renderer in meshRenderers)
            renderer.enabled = visible;
    }
    public void PlacePreviewAt(RaycastHit hit)
    {
        if (CeilingPlacementObject != null)
        {
            CeilingPlacementObject.transform.position = hit.point;
            _placementValid = true;
        }
    }
    public void DisablePlacement()
    {
        _placementValid = false;
        DisableUsing();
    }

    public override void StartUsing(VRTK_InteractUse usingObject)
    {
        base.StartUsing(usingObject);

        GetComponentInChildren<BoxCollider>().enabled = false;
        usingObject.GetComponent<VRTK_Pointer>().Toggle(true);
        ActivePickup = this;
        GroundTrapPlacementPlane.GetInstance().gameObject.SetActive(true);
        TogglePlacementVisible(true);
    }

    public override void StopUsing(VRTK_InteractUse usingObject)
    {
        DisableUsing();
        usingObject.GetComponent<VRTK_Pointer>().Toggle(false);

        base.StopUsing(usingObject);

        if (_placementValid)
            SpawnTrap();
    }

    void DisableUsing()
    {
        TogglePlacementVisible(false);
        GroundTrapPlacementPlane.GetInstance().gameObject.SetActive(false);
        GetComponentInChildren<BoxCollider>().enabled = true;
        ActivePickup = null;
    }

    void SpawnTrap()
    {
        GameObject newTrap = Instantiate(CeilingTrapPrefab);
        newTrap.transform.position = CeilingPlacementObject.transform.position;

        --Quanity;
        if (Quanity <= 0)
            Destroy(gameObject);

        _placementValid = false;
    }

}