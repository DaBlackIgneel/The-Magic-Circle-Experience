using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicList : MonoBehaviour
{

    public static Dictionary<ElementType,GameObject> elementMagicList;
    [SerializeField]
    List<ElementType> elementMagicListKey;
    [SerializeField]
    List<GameObject> elementMagicListValue;

    public static GameObject defaultSpellNode;
    [SerializeField]
    GameObject editorDefaultSpellNode;
    // Start is called before the first frame update
    void Start()
    {
        defaultSpellNode = editorDefaultSpellNode;
        elementMagicList = new Dictionary<ElementType,GameObject>();
        if( elementMagicList.Count == 0)
        {
            for(int i = 0; i < elementMagicListKey.Count; i++)
            {
                elementMagicList.Add(elementMagicListKey[i], elementMagicListValue[i]);
            }
        }
    }
}
