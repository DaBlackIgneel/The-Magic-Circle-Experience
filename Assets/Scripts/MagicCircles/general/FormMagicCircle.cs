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
    public LinkableData<float> rotation = new LinkableData<float>(0);

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
        if( spellParent.autoLinkMagicCircle && spellParent.initialMagicCircle != null )
        {
            MagicCircleDataLinks link = (MagicCircleDataLinks) spellParent.AddLink( LinkTypes.Data );
            link.source = spellParent.initialMagicCircle;
            link.destination = this;
            link.selectedProperty = "GetMagic";
            link.selectedLinkableProperty = "formableMagic";
            link.UpdateSourceAndDestination();
            link.link = true;
            spellParent.initialMagicCircle.autoActivate = false;
            spellParent.mcmm.UpdateWithCreatedLink( link );
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
                formableMagic.Value().SetShape( myForm );
                formableMagic.Value().rotation = Vector3.forward * rotation.Value();
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
            if( !formableMagic.Value().IsActive() )
            {
                formableMagic.Value().Activate();
            }
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
