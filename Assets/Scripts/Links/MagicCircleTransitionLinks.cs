using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum PendingOperation
{
    None,
    Activate,
    Deactivate
}

class PendingOperationContainer
{
    public PendingOperation pendingOp;
    public float timeTillOp;

    public PendingOperationContainer( PendingOperation po, float time )
    {
        pendingOp = po;
        timeTillOp = time;
    }
}

public class MagicCircleTransitionLinks : MagicCircleLinks
{
    // These are declared in the parent class
    // public SpellNode source;
    // public SpellNode destination;
    public float delayTime = 0;
    private float timeTillActivate = 0;
    bool wasActivated;

    PendingOperation pendingOperation = PendingOperation.None;
    List<PendingOperationContainer> pendingOpList = new List<PendingOperationContainer>();

    void FixedUpdate()
    {
        if( source != null && destination != null && source.IsMagicCircle() && destination.IsMagicCircle() )
        {
            MagicCircle mcSource = (MagicCircle)source;
            MagicCircle mcDestination = (MagicCircle)destination;
            if( mcSource.isActive != wasActivated )
            {
                wasActivated = mcSource.isActive;
                PendingOperation operation = mcSource.isActive ? PendingOperation.Activate : PendingOperation.Deactivate;
                pendingOpList.Add( new PendingOperationContainer( operation, delayTime ) );
                // timeTillActivate = delayTime;
                // pendingOperation = mcSource.isActive ? PendingOperation.Activate : PendingOperation.Deactivate;
            }
            // Timer to count down until you activate the magic circle
            List<int> removePoc = new List<int>();
            for( int i = 0; i < pendingOpList.Count; i++ )
            {
                if( pendingOpList[i].timeTillOp <= 0 )
                {
                    // Activate the magic
                    if( pendingOpList[i].pendingOp == PendingOperation.Activate )
                    {
                        mcDestination.Activate();
                    }
                    // Only Deactivate the magic if it hasn't already been deactivated
                    else if( mcDestination.isActive &&  pendingOpList[i].pendingOp == PendingOperation.Deactivate )
                    {
                        mcDestination.Deactivate();
                    }
                    removePoc.Add(i);
                }
                // Keep decreasing the time until it reaches 0;
                else
                {
                    pendingOpList[i].timeTillOp -= Time.fixedDeltaTime;
                }
            }
            for( int i = removePoc.Count -1; i >= 0; i-- )
            {
                pendingOpList.RemoveAt(i);
            }

            // if( timeTillActivate <= 0 )
            // {
            //     // Only Activate the magic if it hasn't already been activated
            //     if( !mcDestination.isActive && pendingOperation == PendingOperation.Activate )
            //     {
            //         mcDestination.Activate();
            //     }
            //     // Only Deactivate the magic if it hasn't already been activated
            //     else if( mcDestination.isActive &&  pendingOperation == PendingOperation.Deactivate )
            //     {
            //         mcDestination.Deactivate();
            //     }
            // }
            // // Keep decreasing the time until it reaches 0;
            // else
            // {
            //     timeTillActivate -= Time.fixedDeltaTime;
            // }
        }
        else
        {
            timeTillActivate = 0;
        }
    }

    void Update()
    {
        DrawLink();
    }


    void DrawLink()
    {
        if( source != null && destination != null )
        {
            Debug.DrawLine( source.transform.position - Vector3.up * .25f, destination.transform.position - Vector3.up * .25f, Color.red );
        }
    }

    public override LinkTypes GetLinkType()
    {
        return LinkTypes.Transition;
    }
}
