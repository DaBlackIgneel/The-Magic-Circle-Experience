using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicCircle : SpellNode
{
    [SerializeField]
    protected MagicCircleType mcType;

    protected Dictionary<int,MagicCircle> innerMagicCircleList;
    protected SpriteRenderer mySpriteRenderer;
    public MagicCircle mcParent;
    public bool isActive;
    public Vector3 position;

    public MagicCircle(): base()
    {
        innerMagicCircleList = new Dictionary<int,MagicCircle>();
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    // Start is called before the first frame update
    void Start()
    {
        mySpriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // UpdateCirclePositions();
    }

    public virtual void Activate()
    {
        isActive = true;
        print("Activating magic circle " + this);
        for(int i = (int)MagicCircleType.START; i <= (int)MagicCircleType.END; i++ )
        {
            if(innerMagicCircleList.ContainsKey(i))
            {
                innerMagicCircleList[i].Activate();
            }
        }
    }

    public virtual void Deactivate()
    {
        isActive = false;
        print("Deactivating magic circle " + this);
        for(int i = (int)MagicCircleType.START; i <= (int)MagicCircleType.END; i++ )
        {
            if(innerMagicCircleList.ContainsKey(i))
            {
                innerMagicCircleList[i].Deactivate();
            }
        }
    }

    public virtual bool AddMagicCircle( MagicCircle mc )
    {
        if( innerMagicCircleList.ContainsKey((int)mc.GetMcType()) )
        {
            Debug.LogError("This Magic Circle already contains this type.  Can't add new Magic Circle " + mc.mcType);
            return false;
        }
        else
        {
            mc.mcParent = this;
            innerMagicCircleList.Add( (int)mc.GetMcType(), mc );
            UpdateCirclePositions();
            return true;
        }
    }

    public virtual bool RemoveMagicCircle( MagicCircle mc )
    {
        foreach( int currMcType in innerMagicCircleList.Keys )
        {
            Debug.Log( "innerMagicCircleList: " + ((MagicCircleType) currMcType).ToString());
        }
        if( innerMagicCircleList.ContainsKey((int)mc.GetMcType()) )
        {
            mc.Deactivate();
            mc.RemoveAllMagicCircles();
            innerMagicCircleList.Remove( (int)mc.GetMcType() );
            Destroy( mc.gameObject );
            UpdateCirclePositions();
            return true;
        }
        else
        {
            Debug.LogError("This Magic Circle is not on the parent magic circle.  Can't remove it " + mc.mcType);
            return false;
        }
    }

    public virtual bool RemoveAllMagicCircles()
    {
        Deactivate();
        if( innerMagicCircleList.Count == 0 )
        {
            return true;
        }
        var keys = new List<MagicCircle>(innerMagicCircleList.Values);
        foreach( MagicCircle mc in keys )
        {
            mc.RemoveAllMagicCircles();
            innerMagicCircleList.Remove( (int) mc.GetMcType() );
            Destroy( mc.gameObject, 1 );
        }
        return true;
    }

    public virtual bool Contains( int checkType )
    {
        return innerMagicCircleList.ContainsKey( checkType );
    }

    public override MagicCircleType GetMcType()
    {
        return mcType;
    }

    public virtual int GetSubType()
    {
        return 0;
    }

    public MagicCircle GetMagicCircle( int checkType )
    {
        return innerMagicCircleList[ checkType ];
    }

    public virtual void SetMcType( MagicCircleType newMcType )
    {
        mcType = newMcType;
        if( mySpriteRenderer == null )
        {
            mySpriteRenderer = GetComponent<SpriteRenderer>();
        }
        switch( mcType )
        {
            case MagicCircleType.None:
                mySpriteRenderer.color = Color.white;
                break;
            case MagicCircleType.Element:
                mySpriteRenderer.color = Color.green;
                break;
            case MagicCircleType.Form:
                mySpriteRenderer.color = Color.black;
                break;
            case MagicCircleType.Movement:
                mySpriteRenderer.color = Color.grey;
                break;
            default:
                mySpriteRenderer.color = Color.magenta;
                break;
        }
    }

    void UpdateCirclePositions()
    {
        int numOfInner = innerMagicCircleList.Count;
        int currentNum = 0;
        for(int i = (int)MagicCircleType.START; i <= (int)MagicCircleType.END; i++ )
        {
            if(innerMagicCircleList.ContainsKey(i))
            {
                innerMagicCircleList[i].gameObject.transform.position = transform.position +
                    (new Vector3( Mathf.Cos(2 * Mathf.PI / numOfInner * currentNum),
                                 Mathf.Sin(2 * Mathf.PI / numOfInner * currentNum),
                                 0));
                currentNum ++;
            }
        }
    }

    public override bool IsMagicCircle()
    {
        return true;
    }
}
