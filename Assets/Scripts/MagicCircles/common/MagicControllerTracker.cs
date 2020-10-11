using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicControllerTracker : MonoBehaviour
{
    MovementMagicCircle movementController;
    FormMagicCircle formController;

    public void SetCurrentMovementController( MovementMagicCircle mmc )
    {
        // if( movementController != null )
        // {
            // movementController.GiveUpPsMoveControl();
            movementController = mmc;
        // }
    }

    public void SetCurrentFormController( FormMagicCircle fmc )
    {
        // if( formController != null )
        // {
            // formController.GiveUpPsFormControl();
            formController = fmc;
        // }
    }

    public bool IsCurrentMoveController( MovementMagicCircle mmc )
    {
        return mmc == movementController;
    }

    public bool IsCurrentFormController( FormMagicCircle fmc )
    {
        return fmc == formController;
    }
}
