using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

[Serializable]
public class ActivatableObjects
{
    public List<GameObject> objs;
}

public class RightClickMenu : MonoBehaviour, IPointerDownHandler
{
    public GameObject rightClickMenu;
    public Vector3 offset;
    [SerializeField]
    public List<ActivatableObjects> activationList;
    public int activationIndex;
    RectTransform rightClickMenuTransform;
    Vector3 rightClickedPoint;

    void Update()
    {

        // if( timer > 0 )
        // {
        //     timer -= Time.deltaTime;
        // }
        // else if( timer != float.MinValue )
        // {
        //     DisableExpandableObject();
        //     timer = float.MinValue;
        // }
        if( Input.GetMouseButtonUp(0) )
        {
            if( rightClickMenu != null )
            {
                rightClickMenu.SetActive( false );
                DisableActivationList();
            }
        }
        if( Input.GetMouseButtonDown(1) )
        {
            if( rightClickMenu != null )
            {
                rightClickMenu.SetActive( true );
                if( rightClickMenuTransform == null )
                {
                    rightClickMenuTransform = rightClickMenu.GetComponent<RectTransform>();
                }
                if( rightClickMenuTransform != null )
                {
                    rightClickMenuTransform.position = (Vector3) Input.mousePosition + Vector3.up * rightClickMenuTransform.position.z + offset;
                }
                rightClickedPoint = (Vector3) Input.mousePosition;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if( eventData.button == PointerEventData.InputButton.Right )
        {
            DisableActivationList();
        }
    }

    public void ActivateIndex( int index )
    {
        if( index < activationList.Count )
        {
            foreach( GameObject obj in activationList[index].objs )
            {
                obj.SetActive( true );
            }
        }
        activationIndex = index;
    }

    void DisableActivationList()
    {
        foreach( ActivatableObjects ao in activationList )
        {
            foreach( GameObject obj in ao.objs )
            {
                obj.SetActive( false );
            }
        }
    }

    public Vector3 GetRightClickedPoint()
    {
        return rightClickedPoint;
    }
}
