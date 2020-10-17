using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell : MonoBehaviour
{
    public List<SpellNode> nodes = new List<SpellNode>(10);
    public List<MagicCircleLinks> links = new List<MagicCircleLinks>(10);
    public MagicCircle baseNode;
    public MagicCircle previousMagicCircle;
    public ElementMagicCircle initialElement;
    public bool autoLinkTransition = true;

    public void Activate()
    {
        if( baseNode != null )
        {
            baseNode.Activate();
        }
    }

    public void Deactivate()
    {
        if( baseNode != null )
        {
            baseNode.Deactivate();
        }
    }

    public void AddNode( SpellNode node )
    {
        node.spellParent = this;
        nodes.Add( node );
        MagicCircle mc = (MagicCircle) node.GetComponent<MagicCircle>();
        if( mc != null )
        {
            if( baseNode == null )
            {
                baseNode = mc;
            }
            if( initialElement == null && mc.GetMcType() == MagicCircleType.Element )
            {
                initialElement = (ElementMagicCircle) mc;
            }

            if( previousMagicCircle != null && autoLinkTransition )
            {
                MagicCircleTransitionLinks link = (MagicCircleTransitionLinks) AddLink( LinkTypes.Transition );
                link.source = previousMagicCircle;
                link.destination = mc;
            }
            previousMagicCircle = mc;
        }
    }

    public SpellNode AddNode( MagicCircleType type, Transform parent = null )
    {
        if( parent == null )
        {
            parent = transform;
        }
        SpellNode node;
        GameObject obj = (GameObject)Instantiate( MagicList.defaultSpellNode, parent );
        obj.name = type.ToString() + " Magic Circle";
        switch( type )
        {
            case MagicCircleType.Element:
            {
                node = obj.AddComponent<ElementMagicCircle>();
                break;
            }
            case MagicCircleType.Form:
            {
                node = obj.AddComponent<FormMagicCircle>();
                break;
            }
            case MagicCircleType.Movement:
            {
                node = obj.AddComponent<MovementMagicCircle>();
                break;
            }
            case MagicCircleType.Input:
            {
                node = obj.AddComponent<UserInputNode>();
                break;
            }
            case MagicCircleType.Logic:
            {
                node = obj.AddComponent<LogicNode>();
                break;
            }
            case MagicCircleType.Math:
            {
                Debug.LogWarning("The Math Type doesn't exist yet");
                return null;
                break;
            }
            default:
            {
                Debug.LogWarning("This type doesn't exist yet");
                return null;
            }
        }

        AddNode( node );
        return node;
    }

    public MagicCircleLinks AddLink( LinkTypes lt)
    {
        MagicCircleLinks mcl;
        switch( lt )
        {
            case LinkTypes.Transition:
            {
                mcl = gameObject.AddComponent<MagicCircleTransitionLinks>();
                break;
            }
            case LinkTypes.Data:
            {
                mcl = gameObject.AddComponent<MagicCircleDataLinks>();
                break;
            }
            default:
            {
                Debug.LogWarning("Yo, there isn't this type of LinkType");
                return null;
            }
        }
        mcl.spellParent = this;
        links.Add( mcl );
        return mcl;
    }

    public void DestroyNode( SpellNode node )
    {
        RemoveNode( node );
        MagicCircle mc = (MagicCircle) node;
        if( mc != null )
        {
            mc.Deactivate();
            // for( int i = 0; i < links.Count; i++ )
            // {
            //     if( links[i].destination == node || links[i].source == node )
            //     {
            //         DestroyLink(links[i]);
            //     }
            // }
        }
        Destroy(node);
    }

    public void DestroyLink( MagicCircleLinks link )
    {
        RemoveLink( link );
        Destroy( link );
    }

    public void RemoveNode( SpellNode node )
    {
        nodes.Remove( node );
    }

    public void RemoveLink( MagicCircleLinks link )
    {
        links.Remove( link );
    }

    public void UpdateParentSpell( Spell spell )
    {
        foreach( SpellNode sn in nodes )
        {
            sn.spellParent = spell;
        }
        foreach( MagicCircleLinks mcl in links )
        {
            mcl.spellParent = spell;
        }
    }
}
