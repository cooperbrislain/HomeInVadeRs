using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class TrapPlacementPlane : VRTK_InteractableObject {

    VRTK_BasePointerRenderer   activePointerRenderer = null;

    public override void StartUsing(VRTK_InteractUse usingObject)
    {
        base.StartUsing(usingObject);

        // Need to make sure we are tracking start/stop for each controller? (or only allow for one controller)
        activePointerRenderer = usingObject.GetComponent<VRTK_BasePointerRenderer>();
        
        usingObject.GetComponent<VRTK_Pointer>().targetListPolicy = GetComponent<VRTK_PolicyList>();

        if (GroundTrapPickup.ActivePickup != null)
            GroundTrapPickup.ActivePickup.TogglePlacementVisible(true);
    }

    public override void StopUsing(VRTK_InteractUse usingObject)
    {
        base.StopUsing(usingObject);

        usingObject.GetComponent<VRTK_Pointer>().targetListPolicy = null;

        if (GroundTrapPickup.ActivePickup != null)
            GroundTrapPickup.ActivePickup.TogglePlacementVisible(false);

        activePointerRenderer = null;
    }

    protected void Start()
    {
    }

    protected override void Update()
    {
        base.Update();

        if (activePointerRenderer != null && activePointerRenderer.IsTracerVisible())
        {
            RaycastHit hit = activePointerRenderer.GetDestinationHit();

            if(hit. GroundTrapPickup.ActivePickup != null)
                GroundTrapPickup.ActivePickup.PlacePreviewAt(hit);
        }
        else if (GroundTrapPickup.ActivePickup != null)
            GroundTrapPickup.ActivePickup.DisablePlacement();
    }
}
