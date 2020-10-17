using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellNode : MonoBehaviour
{
    public Spell spellParent;

    public virtual MagicCircleType GetMcType()
    {
        return MagicCircleType.None;
    }

    public virtual bool IsMagicCircle()
    {
        return false;
    }
}
