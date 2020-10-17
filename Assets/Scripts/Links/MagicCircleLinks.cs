using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicCircleLinks : MonoBehaviour
{
    public SpellNode source;
    public SpellNode destination;
    public Spell spellParent;

    public virtual void SetSource( SpellNode newSource )
    {
        source = newSource;
    }

    public virtual void SetDestination( SpellNode newDestination )
    {
        destination = newDestination;
    }
}
