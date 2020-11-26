using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormMagicCircle : MagicCircle
{
    [SerializeField]
    FormType myForm;
    public LinkableData<ParticleMagic> formableMagic  = new LinkableData<ParticleMagic>(null);
    public LinkableData<float> sizeMultiplier = new LinkableData<float>(1);
    public bool autoLinkToElementMagic = true;

    private MagicControllerTracker magicControllerTracker;

    public FormMagicCircle() : base()
    {
        mcType = MagicCircleType.Form;
        formableMagic = new LinkableData<ParticleMagic>(null);
    }

    // Start is called before the first frame update
    void Start()
    {
        mcType = MagicCircleType.Form;
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        mySpriteRenderer.color = Color.black;
        if( autoLinkToElementMagic && spellParent.initialElement != null )
        {
            MagicCircleDataLinks link = (MagicCircleDataLinks) spellParent.AddLink( LinkTypes.Data );
            link.source = spellParent.initialElement;
            link.destination = this;
            link.selectedProperty = "GetMagic";
            link.selectedLinkableProperty = "formableMagic";
            link.UpdateSourceAndDestination();
            link.link = true;
            spellParent.initialElement.autoActivate = false;
            // formableMagic.SetLinkedValue( emc.GetMagic );
        }
    }

    void FixedUpdate()
    {
        if( isActive )
        {
            if( formableMagic.Value() != null  && magicControllerTracker.IsCurrentFormController( this ) )
            {
                formableMagic.Value().sizeMultiplier = sizeMultiplier.Value();
            }
        }
    }

    public override void SetMcType( MagicCircleType newMcType )
    {

    }

    public override void Activate()
    {
        isActive = true;
        if( formableMagic.Value() != null )
        {
            Debug.Log("Activating Form Magic");

            magicControllerTracker = formableMagic.Value().GetComponent<MagicControllerTracker>();
            if( magicControllerTracker == null )
            {
                magicControllerTracker = formableMagic.Value().gameObject.AddComponent<MagicControllerTracker>();
            }
            magicControllerTracker.SetCurrentFormController( this );

            formableMagic.Value().SetShape( myForm );
            formableMagic.Value().Activate();
        }
        else
        {
            Debug.LogWarning("YOOOOO, there aint no element to form the shape");
        }
    }

    public override void Deactivate()
    {
        isActive = false;
        if( formableMagic.Value() != null )
        {
            Debug.Log("Deactivating magic circle " + this);
            formableMagic.Value().Deactivate();
        }
    }

    public void SetForm( FormType form )
    {
        myForm = form;
    }

    public FormType GetForm()
    {
        return myForm;
    }

    public override int GetSubType()
    {
        return (int)myForm;
    }
}
