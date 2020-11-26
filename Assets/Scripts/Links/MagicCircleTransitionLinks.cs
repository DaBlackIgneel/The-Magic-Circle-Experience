using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicCircleTransitionLinks : MagicCircleLinks
{
    // These are declared in the parent class
    // public SpellNode source;
    // public SpellNode destination;
    public float delayTime = 0;
    private float timeTillActivate = 0;
    bool wasActivated;

    enum PendingOperation
    {
        None,
        Activate,
        Deactivate
    }

    PendingOperation pendingOperation = PendingOperation.None;

    void FixedUpdate()
    {
        if( source != null && destination != null && source.IsMagicCircle() && destination.IsMagicCircle() )
        {
            MagicCircle mcSource = (MagicCircle)source;
            MagicCircle mcDestination = (MagicCircle)destination;
            if( mcSource.isActive != wasActivated )
            {
                wasActivated = mcSource.isActive;
                timeTillActivate = delayTime;
                pendingOperation = mcSource.isActive ? PendingOperation.Activate : PendingOperation.Deactivate;
            }
            // Timer to count down until you activate the magic circle
            if( timeTillActivate <= 0 )
            {
                // Only Activate the magic if it hasn't already been activated
                if( !mcDestination.isActive && pendingOperation == PendingOperation.Activate )
                {
                    mcDestination.Activate();
                }
                // Only Deactivate the magic if it hasn't already been activated
                else if( mcDestination.isActive &&  pendingOperation == PendingOperation.Deactivate )
                {
                    mcDestination.Deactivate();
                }
            }
            // Keep decreasing the time until it reaches 0;
            else
            {
                timeTillActivate -= Time.fixedDeltaTime;
            }
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
