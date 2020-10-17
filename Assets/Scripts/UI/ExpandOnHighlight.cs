using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ExpandOnHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject expandableObj;
    public bool follow;
    public Vector3 offset;
    bool highlighted;
    public float timer = 0;
    const float disableTime = .025f;

    void Update()
    {
        DisableTimer();
    }

    void DisableTimer()
    {
        if( timer > 0 )
        {
            timer -= Time.deltaTime;
        }
        else if( timer != float.MinValue )
        {
            DisableExpandableObject();
            timer = float.MinValue;
        }
    }

    public void OnPointerEnter( PointerEventData eventData )
    {
        // Debug.Log("Entered " + gameObject.name);
        highlighted = true;
        if( expandableObj != null )
        {
            EnableExpandableObject();
        }
    }

    public void OnPointerExit( PointerEventData eventData )
    {
        // Debug.Log("Exited " + gameObject.name);
        highlighted = false;
        if( expandableObj != null )
        {
            timer = disableTime;
        }
    }

    void EnableExpandableObject()
    {
        if( expandableObj != null )
        {
            ExpandOnHighlight eoh = (ExpandOnHighlight) expandableObj.GetComponent<ExpandOnHighlight>();
            if( eoh != null )
            {
                eoh.timer = float.MinValue;
            }
            // Debug.Log("Enabling " + gameObject.name);
            expandableObj.SetActive( true );
            if( follow )
            {
                expandableObj.transform.position = transform.position + offset;
            }
        }
    }

    void DisableExpandableObject()
    {
        if( expandableObj != null && !highlighted )
        {
            ExpandOnHighlight eoh = (ExpandOnHighlight) expandableObj.GetComponent<ExpandOnHighlight>();
            if( eoh != null )
            {
                if( !eoh.IsHighlighted() )
                {
                    // Debug.Log("Disabling " + gameObject.name);
                    expandableObj.SetActive( false );
                }
            }
            else
            {
                expandableObj.SetActive( false );
            }
        }
    }

    public bool IsHighlighted()
    {
        return highlighted;
    }
}
