using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour
{

    bool drag = false;
    Vector3 offset;
    float size = 0.2f;
    Vector3 toXY;

    void Start()
    {
        toXY = new Vector3( 1, 1, 0);
    }

    void Update()
    {
        if( Input.GetMouseButtonDown(0) )
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if( Vector3.Distance( Vector3.Scale(mousePosition, toXY), Vector3.Scale(transform.position, toXY) ) < size )
            {
                if( !drag )
                {
                    drag = true;
                    offset = Vector3.Scale(mousePosition - transform.position, toXY);
                }
            }
        }
        if( drag )
        {
            transform.position = Vector3.Scale(Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset, toXY);
        }
        if( Input.GetMouseButtonUp(0) )
        {
            drag = false;
        }
    }

}
