using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class GroundTrapPlacementPlane : VRTK_InteractableObject {

    VRTK_BasePointerRenderer   activePointerRenderer = null;

    static GroundTrapPlacementPlane        _instance;
    public static GroundTrapPlacementPlane GetInstance() { return _instance; }

    protected override void Awake()
    {
        _instance = this;
        gameObject.SetActive(false);
    }

    public override void StartUsing(VRTK_InteractUse usingObject)
    {
        base.StartUsing(usingObject);

        // Need to make sure we are tracking start/stop for each controller? (or only allow for one controller)
        activePointerRenderer = usingObject.GetComponent<VRTK_BasePointerRenderer>();

        //usingObject.GetComponent<VRTK_Pointer>().targetListPolicy = GetComponent<VRTK_PolicyList>();

        //if (GroundTrapPickup.ActivePickup != null)
        //    GroundTrapPickup.ActivePickup.TogglePlacementVisible(true);
    }

    public override void StopUsing(VRTK_InteractUse usingObject)
    {
        activePointerRenderer = null;

        base.StopUsing(usingObject);

        //usingObject.GetComponent<VRTK_Pointer>().targetListPolicy = null;

        //if (GroundTrapPickup.ActivePickup != null)
        //    GroundTrapPickup.ActivePickup.TogglePlacementVisible(false);
    }


    protected override void Update()
    {
        base.Update();

        if (activePointerRenderer != null && this.IsUsing())
        {
            RaycastHit hit = activePointerRenderer.GetDestinationHit();

            if (GroundTrapPickup.ActivePickup != null)
                GroundTrapPickup.ActivePickup.PlacePreviewAt(hit);
        }
        else if (GroundTrapPickup.ActivePickup != null)
            GroundTrapPickup.ActivePickup.DisablePlacement();
    }
}
