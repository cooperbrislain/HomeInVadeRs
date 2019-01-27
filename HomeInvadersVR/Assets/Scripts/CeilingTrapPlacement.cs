using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class CeilingTrapPlacement : VRTK_InteractableObject
{

    VRTK_BasePointerRenderer activePointerRenderer = null;


    public override void StartUsing(VRTK_InteractUse usingObject)
    {
        base.StartUsing(usingObject);

        // Need to make sure we are tracking start/stop for each controller? (or only allow for one controller)
        activePointerRenderer = usingObject.GetComponentInChildren<VRTK_StraightPointerRenderer>(true);
    }

    public override void StopUsing(VRTK_InteractUse usingObject)
    {
        activePointerRenderer = null;

        base.StopUsing(usingObject);
    }


    protected override void Update()
    {
        base.Update();

        if (activePointerRenderer != null && this.IsUsing())
        {
            RaycastHit hit = activePointerRenderer.GetDestinationHit();

            if (CeilingTrapPickup.ActivePickup != null)
                CeilingTrapPickup.ActivePickup.PlacePreviewAt(hit);
        }
        else if (CeilingTrapPickup.ActivePickup != null)
            CeilingTrapPickup.ActivePickup.DisablePlacement();
    }
}

