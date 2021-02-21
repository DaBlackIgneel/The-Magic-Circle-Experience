using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementMagicCircle : MagicCircle
{
    [SerializeField]
    ElementType myElement;

    public LinkableData<float> emissionRate = new LinkableData<float>(10);
    public ElementPhase elementPhase = ElementPhase.Liquid;
    public Collider2D magicCollider;
    public bool autoActivate = true;
    public int shouldActivate = 0;

    ParticleMagic currentMagic;
    GameObject magicParent;

    public ElementMagicCircle() : base()
    {
        mcType = MagicCircleType.Element;
    }

    // Start is called before the first frame update
    void Start()
    {
        mcType = MagicCircleType.Element;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateElementColor();
    }

    void FixedUpdate()
    {
        if( isActive )
        {
            if( currentMagic != null )
            {
                currentMagic.emissionRate = emissionRate.Value();
                currentMagic.transform.position = magicParent.transform.position;
                currentMagic.SetPhase( elementPhase );
                if( myElement == ElementType.Fire || myElement == ElementType.Wind )
                {
                    if( magicCollider == null )
                    {
                        magicCollider = currentMagic.GetComponent<Collider2D>();
                    }
                    else
                    {
                        if( elementPhase == ElementPhase.Liquid )
                        {
                            magicCollider.isTrigger = true;
                        }
                        else
                        {
                            magicCollider.isTrigger = false;
                        }
                    }
                }
            }
        }
        if( shouldActivate == 1 )
        {
            Activate();
        }
        else if( shouldActivate > 1 )
        {
            shouldActivate--;
        }
    }

    public override void SetMcType( MagicCircleType newMcType )
    {

    }

    public ParticleMagic GetMagic()
    {
        return currentMagic;
    }

    public bool IsEmitting()
    {
        if( isActive )
        {
            return currentMagic.isEmitting;
        }
        else
        {
            return false;
        }
    }

    public GameObject GetMagicObject()
    {
        return currentMagic.gameObject;
    }

    public GameObject GetMagicParent()
    {
        return magicParent;
    }

    public override void Activate()
    {
        if( isActive )
        {
            Deactivate();
            shouldActivate = 10;
            return;
        }
        if( shouldActivate > 0 )
        {
            shouldActivate = 0;
        }
        isActive = true;
        if( currentMagic != null )
        {
            currentMagic.Deactivate();
        }
        print("emissionRate: " + emissionRate );
        magicParent = new GameObject(myElement.ToString() + " Magic Parent");
        magicParent.transform.position = transform.position;
        GameObject magic = Instantiate(MagicList.elementMagicList[myElement], magicParent.transform) as GameObject;
        magic.name = myElement.ToString() + " Magic PS";
        currentMagic = magic.GetComponent<ParticleMagic>();
        currentMagic.SetElement( myElement );
        ClipperTest ct = magic.GetComponent<ClipperTest>();
        if( ct == null )
        {
            magic.AddComponent<ClipperTest>();
        }
        magicCollider = magic.GetComponent<Collider2D>();
        if( currentMagic != null )
        {
            Debug.Log("Activating the " + myElement.ToString() + " Magic");
            if( autoActivate )
            {
                Debug.Log("There ain't no shape, so we just shooting the magic");
                currentMagic.Activate();
            }
            else
            {
                Debug.Log("Waiting for the Form MC to activate the magic");
            }
        }
        else
        {
            Debug.LogError("YO, you forgot to put the ParticleMagic on " + myElement.ToString());
        }
    }

    public override void Deactivate()
    {
        isActive = false;
        if( currentMagic != null )
        {
            print("Deactivating magic circle " + this);
            currentMagic.Deactivate();
        }
    }

    public void SetElement( ElementType element )
    {
        myElement = element;
    }

    public ElementType GetElement()
    {
        return myElement;
    }

    void UpdateElementColor()
    {
        if( mySpriteRenderer == null )
        {
            mySpriteRenderer = GetComponent<SpriteRenderer>();
        }
        switch( myElement )
        {
            case ElementType.Water:
                mySpriteRenderer.color = Color.blue;
                break;
            case ElementType.Earth:
                mySpriteRenderer.color = Color.yellow;
                break;
            case ElementType.Fire:
                mySpriteRenderer.color = Color.red;
                break;
            case ElementType.Wind:
                mySpriteRenderer.color = Color.grey;
                break;
            default:
                mySpriteRenderer.color = Color.black;
                break;
        }
    }

    public override int GetSubType()
    {
        return (int) myElement;
    }
}
