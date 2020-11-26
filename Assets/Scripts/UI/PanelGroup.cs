using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class PanelGroup : MonoBehaviour
{
    public GameObject[] panels;
    public int panelIndex;

    void Awake()
    {
        ShowPanels();
    }

    void ShowPanels()
    {
        for( int i = 0; i < panels.Length; i++ )
        {
            if( i == panelIndex )
            {
                panels[i].SetActive( true );
            }
            else
            {
                panels[i].SetActive( false );
            }
        }
    }

    public void SetPageIndex( int index )
    {
        panelIndex = index >= 0 ? index : 0;
        ShowPanels();
    }

    public int GetPageIndex()
    {
        return panelIndex;
    }

}
