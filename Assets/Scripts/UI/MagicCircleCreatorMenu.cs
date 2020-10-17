using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class MagicCircleCreatorMenu : MonoBehaviour
{
    public GameObject mcTypeDropdown;
    public MagicCircle rootMagicCircle;
    public GameObject defaultMagicCircle;

    public MagicCircle selectedMagicCircle;
    public MagicCircle linkableSourceMagicCircle;
    public Dropdown mainTypeDropdown;
    public Dropdown subTypeDropdown;
    public UnityEngine.UI.Slider value1Slider;
    public Dropdown linkablePropertiesSink;
    public Dropdown linkablePropertiesSource;

    MagicCircleType mcType;
    int subType;

    bool drag;
    float emissionRate = 0;
    float force = 10;
    float sizeMultiplier = 1;
    Spell spell;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if( Input.GetMouseButtonDown(0) )
        {
            // if left click is pressed...
            Vector3 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D obj =  Physics2D.OverlapPoint( (Vector2) mousePoint );
            if( obj != null )
            {
                MagicCircle mc = obj.GetComponent<MagicCircle>();
                if( mc != null )
                    drag = true;
                if( mc != null && selectedMagicCircle != mc)
                {
                    selectedMagicCircle = mc;
                    if( selectedMagicCircle.GetMcType() == MagicCircleType.None )
                    {
                        rootMagicCircle = selectedMagicCircle;
                    }
                    UpdateSelectSensitiveDropdowns();
                }
            }
       }
       if( Input.GetMouseButtonDown(2) )
       {
           if( selectedMagicCircle != null )
           {
               Vector3 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
               Collider2D obj =  Physics2D.OverlapPoint( (Vector2) mousePoint );
               if( obj != null )
               {
                   MagicCircle mc = obj.GetComponent<MagicCircle>();
                   if( mc != null && selectedMagicCircle != mc)
                   {
                       linkableSourceMagicCircle = mc;
                       print( linkablePropertiesSink.captionText.text );
                       string[] linkableMethods = LinkableFinder.FindPropertiesToLinkTo( linkablePropertiesSink.captionText.text, selectedMagicCircle, linkableSourceMagicCircle );

                       linkablePropertiesSource.options.Clear();
                       for(int i = 0; i < linkableMethods.Length; i++ )
                       {
                           linkablePropertiesSource.options.Add( new Dropdown.OptionData( linkableMethods[i] ) );
                       }
                       linkablePropertiesSource.RefreshShownValue();
                   }
               }
           }
       }
       if( Input.GetMouseButtonUp(1) )
       {
           selectedMagicCircle = null;
           linkablePropertiesSink.options.Clear();
           linkablePropertiesSource.options.Clear();
       }
       if( Input.GetMouseButtonUp(0) )
       {
           drag = false;
       }
       if( drag )
       {
           Vector3 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
           selectedMagicCircle.transform.position = new Vector3( mousePoint.x, mousePoint.y, selectedMagicCircle.transform.position.z );
       }
    }

    public void UpdateSelectSensitiveDropdowns()
    {
        // Update Linkable Properties Sink
        string[] linkProperties = LinkableFinder.FindAllLinkableProperty( selectedMagicCircle );
        linkablePropertiesSink.options.Clear();
        for(int i = 0; i < linkProperties.Length; i++ )
        {
            print("new prop: " + linkProperties[i]);
            linkablePropertiesSink.options.Add( new Dropdown.OptionData( linkProperties[i] ) );
        }
        linkablePropertiesSink.RefreshShownValue();

        if( selectedMagicCircle != null )
        {
            // Update Main types Dropdown
            mainTypeDropdown.value = (int) selectedMagicCircle.GetMcType();

            // Update sub types Dropdown
            subTypeDropdown.value = selectedMagicCircle.GetSubType();
        }
    }

    public void OnLinkProperties()
    {
        if( linkablePropertiesSink.captionText.text.Length > 0 && linkablePropertiesSource.captionText.text.Length > 0
            && linkableSourceMagicCircle != null && selectedMagicCircle != null)
        {
            LinkableFinder.LinkField
                (
                linkablePropertiesSink.captionText.text,
                selectedMagicCircle,
                linkablePropertiesSource.captionText.text,
                linkableSourceMagicCircle
                );
        }
    }

    public void OnEmissionRateChanged(float newValue)
    {
        if( mcType == MagicCircleType.Element )
        {
            emissionRate = newValue;
            if( selectedMagicCircle != null )
            {
                ((ElementMagicCircle) selectedMagicCircle ).emissionRate.SetDefaultValue( emissionRate );
                print("Setting emission rate to " + emissionRate);
            }
        }
        else if( mcType == MagicCircleType.Movement )
        {
            force = newValue/255f * 25;
            if( selectedMagicCircle != null )
            {
                ((MovementMagicCircle) selectedMagicCircle ).force.SetDefaultValue( force );
                print("Setting force to " + force);
            }
        }
        else if( mcType == MagicCircleType.Form )
        {
            sizeMultiplier = newValue/255f * 5;
            if( selectedMagicCircle != null )
            {
                ((FormMagicCircle) selectedMagicCircle ).sizeMultiplier.SetDefaultValue( sizeMultiplier );
                print("Setting sizeMultiplier to " + sizeMultiplier);
            }
        }
    }

    public void OnMcTypeChanged(int newValue)
    {
        mcType = newValue <= (int)MagicCircleType.END ? (MagicCircleType)newValue : MagicCircleType.END;
        if( !drag )
        {
            selectedMagicCircle = null;
        }

        if( selectedMagicCircle != null )
        {
            mcType = selectedMagicCircle.GetMcType();
            if( newValue != (int)mcType )
            {
                mainTypeDropdown.value = (int)mcType;
            }
        }
        print( "mcType = " + mcType );
        if( subTypeDropdown != null )
        {
            switch( mcType )
            {
                case MagicCircleType.Element:
                    subTypeDropdown.options.Clear();
                    for(int i = (int)ElementType.START; i <= (int)ElementType.END; i++ )
                    {
                        subTypeDropdown.options.Add( new Dropdown.OptionData( ( ( ElementType ) i ).ToString() ) );
                    }
                    break;
                case MagicCircleType.Form:
                    subTypeDropdown.options.Clear();
                    for(int i = (int)FormType.START; i <= (int)FormType.END; i++ )
                    {
                        subTypeDropdown.options.Add( new Dropdown.OptionData( ( ( FormType ) i ).ToString() ) );
                    }
                    break;
                case MagicCircleType.Movement:
                    subTypeDropdown.options.Clear();
                    for(int i = (int)MovementType.START; i <= (int)MovementType.END; i++ )
                    {
                        subTypeDropdown.options.Add( new Dropdown.OptionData( ( ( MovementType ) i ).ToString() ) );
                    }
                    break;
                default:
                    subTypeDropdown.options.Clear();
                    break;
            }
            mainTypeDropdown.RefreshShownValue();
            subTypeDropdown.RefreshShownValue();
        }

    }

    public void OnSubTypeChanged(int newValue)
    {
        subType = newValue;
        print( "subType = " + subType );
        if( selectedMagicCircle != null )
        {
            switch( mcType )
            {
                case MagicCircleType.Element:
                {
                    ((ElementMagicCircle)selectedMagicCircle).SetElement( (ElementType) subType );
                    break;
                }
                case MagicCircleType.Form:
                {
                    ((FormMagicCircle)selectedMagicCircle).SetForm( (FormType) subType );
                    break;
                }
                case MagicCircleType.Movement:
                {
                    ((MovementMagicCircle)selectedMagicCircle).SetMovement( (MovementType) subType );
                    break;
                }
                default:
                    Debug.LogWarning(" Creator Menu Sub Type not implemented ");
                    break;
            }
        }
        subTypeDropdown.RefreshShownValue();
    }

    public void CreateSpell()
    {
        GameObject objSpell = new GameObject();
        spell = (Spell) objSpell.AddComponent<Spell>();
        spell.name = "New Generic Spell";
    }

    public void OnCreateMC()
    {
        if( spell == null )
        {
            CreateSpell();
        }
        if( rootMagicCircle == null || !rootMagicCircle.Contains( (int)mcType ) )
        {
            if( mcType == MagicCircleType.None )
            {
                rootMagicCircle = null;
            }
            OnEmissionRateChanged( value1Slider.value );
            Transform pos = rootMagicCircle == null ? defaultMagicCircle.transform : rootMagicCircle.transform;
            // GameObject newObj = Instantiate( defaultMagicCircle, pos ) as GameObject;
            // newObj.name = mcType.ToString() + " Magic Circle";
            SpellNode newNode = spell.AddNode( mcType );
            newNode.name = mcType.ToString() + " Magic Circle";
            MagicCircle newMc;
            switch( mcType )
            {
                case MagicCircleType.Element:
                    ElementMagicCircle tempMC = newNode as ElementMagicCircle;
                    // tempMC.SetMcType( mcType );
                    tempMC.SetElement( (ElementType) subType );
                    tempMC.emissionRate.SetDefaultValue( emissionRate );
                    newMc = tempMC as MagicCircle;
                    break;
                case MagicCircleType.Form:
                    FormMagicCircle tempFormMC = newNode as FormMagicCircle;
                    // tempMC.SetMcType( mcType );
                    // tempMC.SetElement( (ElementType) subType );
                    tempFormMC.SetForm( (FormType) subType );
                    tempFormMC.sizeMultiplier.SetDefaultValue( sizeMultiplier );
                    newMc = tempFormMC as MagicCircle;
                    break;
                case MagicCircleType.Movement:
                    MovementMagicCircle tempMovementMC = newNode as MovementMagicCircle;
                    // tempMC.SetMcType( mcType );
                    // tempMC.SetElement( (ElementType) subType );
                    tempMovementMC.SetMovement( (MovementType) subType );
                    tempMovementMC.force.SetDefaultValue( force );
                    newMc = tempMovementMC as MagicCircle;
                    break;
                default:
                    newMc = newNode as MagicCircle;
                    newMc.SetMcType( mcType );
                    break;
            }
            if( rootMagicCircle == null )
            {
                rootMagicCircle = newMc;
            }
            else
            {
                // rootMagicCircle.AddMagicCircle( newMc );
            }
            selectedMagicCircle = newMc;
        }
    }

    public void OnRemoveMc()
    {
        if( selectedMagicCircle != null )
        {
            if( selectedMagicCircle.mcParent == null )
            {
                selectedMagicCircle.RemoveAllMagicCircles();
                Destroy( selectedMagicCircle.gameObject );
            }
            else
            {
                Debug.Log("Removing Magic Circle");
                selectedMagicCircle.mcParent.RemoveMagicCircle( selectedMagicCircle );
            }
        }
    }

    public void Activate()
    {
        // if( rootMagicCircle != null )
        // {
        //     rootMagicCircle.Activate();
        // }
        if( spell != null )
        {
            spell.Activate();
        }
    }

    public void Deactivate()
    {
        // if( rootMagicCircle != null )
        // {
        //     rootMagicCircle.Deactivate();
        // }
        if( spell != null )
        {
            spell.Deactivate();
        }
    }

}
