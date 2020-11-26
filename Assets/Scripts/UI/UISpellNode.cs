using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UISpellNode : MonoBehaviour, IPointerDownHandler
{
    public SpellNode linkedSpellNode;
    public bool removeOnSpellNodeGone;

    UnityEngine.UI.Image sr;
    MagicCircleMakerMenu mcmm;

    void Start()
    {
        sr = GetComponent<UnityEngine.UI.Image>();

        if( sr != null )
        {
            if( linkedSpellNode != null )
            {
                switch( linkedSpellNode.GetMcType() )
                {
                    case MagicCircleType.Element:
                    {
                        sr.color = Color.red;
                        break;
                    }
                    case MagicCircleType.Form:
                    {
                        sr.color = Color.black;
                        break;
                    }
                    case MagicCircleType.Movement:
                    {
                        sr.color = Color.green;
                        break;
                    }
                    case MagicCircleType.Input:
                    {
                        sr.color = Color.cyan;
                        break;
                    }
                    case MagicCircleType.Logic:
                    {
                        sr.color = Color.magenta;
                        break;
                    }
                    case MagicCircleType.Math:
                    {
                        sr.color = Color.grey;
                        break;
                    }
                    default:
                    {
                        sr.color = Color.white;
                        break;
                    }
                }
            }
        }
    }

    void Update()
    {
        // if( removeOnSpellNodeGone && linkedSpellNode == null && mcmm != null )
        // {
        //     mcmm.RemoveUISpellNode( this );
        // }
    }

    public void SetUIMenu( MagicCircleMakerMenu myMcmm )
    {
        mcmm = myMcmm;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if( eventData.button == PointerEventData.InputButton.Left )
        {
            if( mcmm != null )
            {
                mcmm.UpdateSelectedUISpellNode( this );
            }
        }
        else if( eventData.button == PointerEventData.InputButton.Right )
        {
            if( mcmm != null )
            {
                mcmm.UpdateRightClickMenuWithNode( this );
            }
        }
    }

    public void Delete()
    {
        if( linkedSpellNode.IsMagicCircle() )
        {
            MagicCircle mc = (MagicCircle) linkedSpellNode;
            mc.Deactivate();
        }
        Destroy( linkedSpellNode.gameObject );
        Destroy( this.gameObject );
    }
}
