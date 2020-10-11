using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicCircleLinks : MonoBehaviour
{
    public MagicCircle source;
    public MagicCircle destination;

    public virtual void SetSource( MagicCircle newSource )
    {
        source = newSource;
    }

    public virtual void SetDestination( MagicCircle newDestination )
    {
        destination = newDestination;
    }
}
