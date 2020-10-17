using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ModifyableUITypes
{
    Node,
    Link
}

public class MagicCircleMakerMenu : MonoBehaviour
{
    [SerializeField]
    public ElementalSpellNodeUiData elementalData;
    [SerializeField]
    public FormSpellNodeUiData formData;
    [SerializeField]
    public MovementSpellNodeUiData movementData;
    [SerializeField]
    public InputSpellNodeUiData inputData;
    [SerializeField]
    public LogicSpellNodeUiData logicData;

    public RectTransform UiBoard;
    public RightClickMenu rcm;
    public GameObject uiSpellNodePrefab;
    public PanelGroup McDetailsPanel;
    public Text detailsText;

    public enum SelectionSpace
    {
        UI,
        World
    }

    public enum SelectionMode
    {
        None,
        Drag,
        TransitionLink,
        DataLink
    }

    SpellNode selectedSpellNode;
    UISpellNode selectedUISN;
    Spell currentSpell;
    Vector2 offset;
    bool drag;
    GameObject dragableObj;
    SelectionSpace selectedSpace;
    SelectionMode selectionMode;
    List<UISpellNode> uiSpellNodeList = new List<UISpellNode>();

    void Update()
    {
        if( Input.GetMouseButtonDown(0) )
        {
            // if left click is pressed...
            if( selectionMode == SelectionMode.None )
            {
                Vector3 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Collider2D obj =  Physics2D.OverlapPoint( (Vector2) mousePoint );
                if( obj != null )
                {
                    SpellNode sn = obj.GetComponent<SpellNode>();
                    if( sn != null )
                    {
                            drag = true;
                            selectionMode = SelectionMode.Drag;
                            dragableObj = sn.gameObject;
                            offset = (Vector2)(sn.transform.position - mousePoint);
                            selectedSpace = SelectionSpace.World;
                            UpdateSelectedSpellNode( sn );
                    }
                }
            }
            else if( selectionMode == SelectionMode.TransitionLink )
            {

            }
       }
       if( selectedSpellNode == null )
       {
           UpdateDetailsPanel( 0 );
       }
       // if( Input.GetMouseButtonDown(2) )
       // {
       //     if( selectedMagicCircle != null )
       //     {
       //         Vector3 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
       //         Collider2D obj =  Physics2D.OverlapPoint( (Vector2) mousePoint );
       //         if( obj != null )
       //         {
       //             MagicCircle mc = obj.GetComponent<MagicCircle>();
       //             if( mc != null && selectedMagicCircle != mc)
       //             {
       //                 linkableSourceMagicCircle = mc;
       //                 print( linkablePropertiesSink.captionText.text );
       //                 string[] linkableMethods = LinkableFinder.FindPropertiesToLinkTo( linkablePropertiesSink.captionText.text, selectedMagicCircle, linkableSourceMagicCircle );
       //
       //                 linkablePropertiesSource.options.Clear();
       //                 for(int i = 0; i < linkableMethods.Length; i++ )
       //                 {
       //                     linkablePropertiesSource.options.Add( new Dropdown.OptionData( linkableMethods[i] ) );
       //                 }
       //                 linkablePropertiesSource.RefreshShownValue();
       //             }
       //         }
       //     }
       // }

       // Unselect something when you right click
       if( Input.GetMouseButtonUp(1) )
       {
           selectedSpellNode = null;
           selectionMode = SelectionMode.None;
       }

       // Stop dragging when you release the mouse
       if( Input.GetMouseButtonUp(0) )
       {
           if( selectionMode == SelectionMode.Drag )
           {
               drag = false;
               selectionMode = SelectionMode.None;
           }
       }

       // Drag selected item
       if( selectionMode == SelectionMode.Drag )
       {
           Vector3 mousePoint;
           if( selectedSpace == SelectionSpace.World )
           {
               mousePoint = Camera.main.ScreenToWorldPoint( Input.mousePosition );
           }
           else
           {
               mousePoint = Input.mousePosition;
           }
           dragableObj.transform.position = new Vector3( mousePoint.x, mousePoint.y, dragableObj.transform.position.z ) + (Vector3)offset;
       }
    }

    public void UpdateSelectedUISpellNode( UISpellNode uisn )
    {
        if( selectionMode == SelectionMode.None )
        {
            drag = true;
            selectionMode = SelectionMode.Drag;
            dragableObj = uisn.gameObject;
            offset = (Vector2)(uisn.transform.position - Input.mousePosition);
            selectedSpace = SelectionSpace.UI;
            UpdateSelectedSpellNode( uisn.linkedSpellNode );
        }
        else if( selectionMode == SelectionMode.DataLink || selectionMode == SelectionMode.TransitionLink )
        {
            Debug.Log("Creating Link with source " + selectedUISN.gameObject.name + " and destination " + uisn.gameObject.name);
            CreateLink( selectedUISN, uisn );
        }
    }

    public void UpdateSelectedSpellNode( SpellNode sn )
    {
        UpdateDetailsPanel( (int)sn.GetMcType() );
        if( selectedSpellNode != sn)
        {
            selectedSpellNode = sn;
        }
    }

    void UpdateDetailsPanel( int index )
    {
        McDetailsPanel.SetPageIndex( index );
        int maxMagicCircleTypes = 7;
        if( index < maxMagicCircleTypes )
        {
            detailsText.text = ((MagicCircleType) index ).ToString() + " Details";
        }
        else
        {
            detailsText.text = ((LinkTypes) (index - maxMagicCircleTypes) ).ToString() + " Details";
        }
    }


    public void UpdateRightClickMenuWithNode( UISpellNode uisn )
    {
        if( uisn != null )
        {
            rcm.ActivateIndex( (int) ModifyableUITypes.Node );
        }
        selectedUISN = uisn;
    }

    public void CreateLink( UISpellNode source, UISpellNode destination )
    {
        MagicCircleLinks mcl;
        if( selectionMode == SelectionMode.DataLink )
        {
            mcl = currentSpell.AddLink( LinkTypes.Data );
        }
        else if( selectionMode == SelectionMode.TransitionLink )
        {
            if( source.linkedSpellNode.IsMagicCircle() && destination.linkedSpellNode.IsMagicCircle() )
            {
                mcl = currentSpell.AddLink( LinkTypes.Transition );
            }
            else
            {
                Debug.LogWarning( "Can't a transition link with a non magic circle ");
                return;
            }
        }
        else
        {
            Debug.LogWarning( "Can't Create this link type " + selectionMode);
            return;
        }
        mcl.source = source.linkedSpellNode;
        mcl.destination = destination.linkedSpellNode;
        selectionMode = SelectionMode.None;

    }

    public void CreateSpellNode( int mct )
    {
        if( currentSpell == null )
        {
            CreateSpell();
        }

        selectedSpellNode = currentSpell.AddNode( (MagicCircleType) mct );
        CreateUISpellNode( selectedSpellNode );
        UpdateDetailsPanel( mct );
    }

    public void CreateUISpellNode( SpellNode sn )
    {
        GameObject obj = (GameObject) Instantiate( uiSpellNodePrefab, UiBoard );
        obj.transform.position = rcm.GetRightClickedPoint();
        obj.name = "UI" + sn.GetMcType().ToString() + "SpellNode";
        UISpellNode usn = (UISpellNode) obj.GetComponent<UISpellNode>();
        if( usn != null )
        {
            usn.linkedSpellNode = sn;
            usn.SetUIMenu( this );
            uiSpellNodeList.Add( usn );
        }
    }

    public void CreateSpell()
    {
        GameObject objSpell = new GameObject();
        currentSpell = (Spell) objSpell.AddComponent<Spell>();
        currentSpell.name = "New Generic Spell";
    }

    // public void RemoveSelectedUISpellNode()
    // {
    //     RemoveUISpellNode( selectedUISN );
    // }

    // public void RemoveUISpellNode( UISpellNode uisn )
    // {
    //     uiSpellNodeList.Remove( uisn );
    //     if( uisn.linkedSpellNode != null )
    //     {
    //         Destroy( uisn.linkedSpellNode );
    //     }
    //     Destroy( uisn );
    // }

    public void OnSpellAdded( MagicCircleType mct )
    {

    }

    public void SetLinkMode( int link )
    {
        Debug.Log("Setting link mode to " + link );
        switch( (LinkTypes) link )
        {
            case LinkTypes.Transition:
            {
                selectionMode = SelectionMode.TransitionLink;
                break;
            }
            case LinkTypes.Data:
            {
                selectionMode = SelectionMode.DataLink;
                break;
            }
            default:
            {
                Debug.LogWarning(" SetLinkMode link type not implemented ");
                return;
            }
        }
    }


#region ElementUiCallbacks
    public void OnElementalChanged( int value )
    {
        ElementMagicCircle emc = (ElementMagicCircle) selectedSpellNode;
        if( emc != null )
        {
            emc.SetElement( elementalData.GetElement() );
        }
    }

    public void OnElementalEmissionRateChanged( float value )
    {
        elementalData.OnEmissionRateChanged( value );
        ElementMagicCircle emc = (ElementMagicCircle) selectedSpellNode;
        if( emc != null )
        {
            emc.emissionRate.SetDefaultValue( elementalData.GetEmissionRate() );
        }
    }

    public void OnElementalAutoActivateChanged( bool value )
    {
        ElementMagicCircle emc = (ElementMagicCircle) selectedSpellNode;
        if( emc != null )
        {
            emc.autoActivate = elementalData.IsAutoActive();
        }
    }

    public void OnElementalFluidStateChanged( int value )
    {
        ElementMagicCircle emc = (ElementMagicCircle) selectedSpellNode;
        if( emc != null )
        {
            emc.elementPhase = elementalData.GetFluidState();
        }
    }

    public void OnElementalEndEdit( string value )
    {
        elementalData.SetEmissionRate( float.Parse( value ) );
    }
#endregion

#region FormUiCallbacks
    public void OnFormChanged( int value )
    {
        FormMagicCircle fmc = (FormMagicCircle) selectedSpellNode;
        if( fmc != null )
        {
            fmc.SetForm( formData.GetForm() );
        }
    }

    public void OnFormSizeChange( float value )
    {
        formData.OnSizeChanged();
        FormMagicCircle fmc = (FormMagicCircle) selectedSpellNode;
        if( fmc != null )
        {
            fmc.sizeMultiplier.SetDefaultValue( formData.GetSize() );
        }
    }

    public void OnFormAutoLinkChanged( bool value )
    {
        FormMagicCircle fmc = (FormMagicCircle) selectedSpellNode;
        if( fmc != null )
        {
            fmc.autoLinkToElementMagic = formData.IsAutoLink();
        }
    }

    public void OnFormEndEdit( string value )
    {
        formData.SetSize( float.Parse( value ) );
    }
#endregion

#region MovementUiCallbacks
    public void OnMovementChanged( int value )
    {
        MovementMagicCircle mmc = (MovementMagicCircle) selectedSpellNode;
        if( mmc != null )
        {
            mmc.SetMovement( movementData.GetMovement() );
        }
    }

    public void OnMovementInitialVelocityChange( float value )
    {
        movementData.OnInitialVelocityChanged();
        MovementMagicCircle mmc = (MovementMagicCircle) selectedSpellNode;
        if( mmc != null )
        {
            mmc.initialVelocity.SetDefaultValue( movementData.GetInitialVelocity() );
        }
    }

    public void OnMovementInitialVelocityEndEdit( string value )
    {
        movementData.SetInitialVelocity( float.Parse( value ) );
    }

    public void OnMovementForceChange( float value )
    {
        movementData.OnForceChanged();
        MovementMagicCircle mmc = (MovementMagicCircle) selectedSpellNode;
        if( mmc != null )
        {
            mmc.force.SetDefaultValue( movementData.GetForce() );
        }
    }

    public void OnMovementForceEndEdit( string value )
    {
        movementData.SetForce( float.Parse( value ) );
    }

    public void OnMovementAngleChange( float value )
    {
        movementData.OnAngleChanged();
        MovementMagicCircle mmc = (MovementMagicCircle) selectedSpellNode;
        if( mmc != null )
        {
            mmc.directionAngle.SetDefaultValue( movementData.GetAngle() );
        }
    }

    public void OnMovementAngleEndEdit( string value )
    {
        movementData.SetAngle( float.Parse( value ) );
    }

    public void OnMovementAutoLinkChanged( bool value )
    {
        MovementMagicCircle mmc = (MovementMagicCircle) selectedSpellNode;
        if( mmc != null )
        {
            mmc.autoLinkToElementMagic = movementData.IsAutoLink();
        }
    }

    public void OnMovementDragMagicChanged( bool value )
    {
        MovementMagicCircle mmc = (MovementMagicCircle) selectedSpellNode;
        if( mmc != null )
        {
            mmc.dragMagic = movementData.ShouldDragMagic();
        }
    }
#endregion

#region UserInputUiCallbacks
    public void OnInputEndEdit( string value )
    {
        UserInputNode uin = (UserInputNode) selectedSpellNode;
        if( uin != null )
        {
            uin.inputCharacter = value;
            inputData.OnInputCharacterChanged( uin.GetKeyString() );
        }
    }
#endregion

#region LogicUiCallbacks
    public void OnLogicLeft1EndEdit( string value )
    {
        LogicNode ln = (LogicNode) selectedSpellNode;
        if( ln != null )
        {
            switch( logicData.GetLogicDataType() )
            {
                case InputDataType.Int:
                {
                    ln.leftHandValue.SetDefaultValue( int.Parse( value ) );
                    break;
                }
                case InputDataType.Float:
                {
                    ln.leftHandValue.SetDefaultValue( float.Parse( value ) );
                    break;
                }
                case InputDataType.Vector:
                {
                    float defaultValue = 0;
                    float.TryParse( logicData.GetLeftHandData2(), out defaultValue );
                    ln.leftHandValue.SetDefaultValue( new Vector2( Convert.ToSingle( value ), defaultValue ) );
                    break;
                }
                case InputDataType.Bool:
                {
                    ln.leftHandValue.SetDefaultValue( value.ToLower() == "true" );
                    break;
                }
                default:
                {
                    ln.leftHandValue.SetDefaultValue( value );
                    break;
                }
            }
        }
    }

    public void OnLogicLeft2EndEdit( string value )
    {
        LogicNode ln = (LogicNode) selectedSpellNode;
        if( ln != null )
        {
            switch( logicData.GetLogicDataType() )
            {
                case InputDataType.Vector:
                {
                    ln.leftHandValue.SetDefaultValue( new Vector2( float.Parse( logicData.GetLeftHandData1() ), float.Parse( value ) ) );
                    break;
                }
                default:
                {
                    // ln.leftHandValue.SetDefaultValue( logicData.GetLeftHandData1() );
                    break;
                }
            }
        }
    }

    public void OnLogicRight1EndEdit( string value )
    {
        LogicNode ln = (LogicNode) selectedSpellNode;
        if( ln != null )
        {
            switch( logicData.GetLogicDataType() )
            {
                case InputDataType.Int:
                {
                    ln.rightHandValue.SetDefaultValue( int.Parse( value ) );
                    break;
                }
                case InputDataType.Float:
                {
                    ln.rightHandValue.SetDefaultValue( float.Parse( value ) );
                    break;
                }
                case InputDataType.Vector:
                {
                    ln.rightHandValue.SetDefaultValue( new Vector2( float.Parse( value ), float.Parse( logicData.GetRightHandData2() ) ) );
                    break;
                }
                case InputDataType.Bool:
                {
                    ln.rightHandValue.SetDefaultValue( value.ToLower() == "true" );
                    break;
                }
                default:
                {
                    ln.rightHandValue.SetDefaultValue( value );
                    break;
                }
            }
        }
    }

    public void OnLogicRight2EndEdit( string value )
    {
        LogicNode ln = (LogicNode) selectedSpellNode;
        if( ln != null )
        {
            switch( logicData.GetLogicDataType() )
            {
                case InputDataType.Vector:
                {
                    ln.rightHandValue.SetDefaultValue( new Vector2( float.Parse( logicData.GetRightHandData1() ), float.Parse( value ) ) );
                    break;
                }
                default:
                {
                    // ln.rightHandValue.SetDefaultValue( logicData.GetRightHandData1() );
                    break;
                }
            }
        }
    }

    public void OnLogicInputTypeChanged(int value)
    {
        logicData.logicLeftHandInput2.gameObject.SetActive( logicData.GetLogicDataType() == InputDataType.Vector );
        logicData.logicRightHandInput2.gameObject.SetActive( logicData.GetLogicDataType() == InputDataType.Vector );
    }

    public void OnLogicCompareTypeChanged(int value)
    {
        LogicNode ln = (LogicNode) selectedSpellNode;
        if( ln != null )
        {
            ln.compareType = (LogicType) value;
        }
    }
#endregion

}

[Serializable]
public class LogicSpellNodeUiData
{
    public Dropdown logicDataType;
    public Dropdown logicCompareType;
    public UnityEngine.UI.InputField logicLeftHandInput1;
    public UnityEngine.UI.InputField logicLeftHandInput2;
    public UnityEngine.UI.InputField logicRightHandInput1;
    public UnityEngine.UI.InputField logicRightHandInput2;

    public void OnInputCharacterChanged( string value )
    {
        // inputCharacter.text = value;
    }

    public string GetLeftHandData1()
    {
        return logicLeftHandInput1.text;
    }

    public string GetLeftHandData2()
    {
        return logicLeftHandInput2.text;
    }

    public string GetRightHandData1()
    {
        return logicRightHandInput1.text;
    }

    public string GetRightHandData2()
    {
        return logicRightHandInput2.text;
    }

    public InputDataType GetLogicDataType()
    {
        return (InputDataType) logicDataType.value;
    }

    public InputDataType GetLogicCompareType()
    {
        return (InputDataType) logicCompareType.value;
    }
}

[Serializable]
public class InputSpellNodeUiData
{
    public UnityEngine.UI.InputField inputCharacter;

    public void OnInputCharacterChanged( string value )
    {
        inputCharacter.text = value;
    }
}

[Serializable]
public class MovementSpellNodeUiData
{
    public UnityEngine.UI.Slider initialVelocity;
    public UnityEngine.UI.InputField initialVelocityInput;
    public UnityEngine.UI.Slider force;
    public UnityEngine.UI.InputField forceInput;
    public UnityEngine.UI.Slider angle;
    public UnityEngine.UI.InputField angleInput;
    public UnityEngine.UI.Toggle autoLink;
    public UnityEngine.UI.Toggle dragMagic;
    public Dropdown movement;

    public void SetInitialVelocity( float value )
    {
        initialVelocity.value = value < initialVelocity.maxValue ? value : initialVelocity.maxValue;
        initialVelocity.value = initialVelocity.value > initialVelocity.minValue ? initialVelocity.value : initialVelocity.minValue;
    }

    public float GetInitialVelocity()
    {
        return initialVelocity.value;
    }

    public void OnInitialVelocityChanged()
    {
        initialVelocityInput.text = initialVelocity.value.ToString("F2");
    }

    public void SetForce( float value )
    {
        force.value = value < force.maxValue ? value : force.maxValue;
        force.value = force.value > force.minValue ? force.value : force.minValue;
    }

    public float GetForce()
    {
        return force.value;
    }

    public void OnForceChanged()
    {
        forceInput.text = force.value.ToString("F2");
    }

    public void SetAngle( float value )
    {
        angle.value = value < angle.maxValue ? value : angle.maxValue;
        angle.value = angle.value > angle.minValue ? angle.value : angle.minValue;
    }

    public float GetAngle()
    {
        return angle.value;
    }

    public void OnAngleChanged()
    {
        angleInput.text = angle.value.ToString("F1");
    }

    public bool IsAutoLink()
    {
        return autoLink.isOn;
    }

    public bool ShouldDragMagic()
    {
        return dragMagic.isOn;
    }

    public void SetMovement( MovementType movementType )
    {
        movement.value = (int) movementType;
        movement.RefreshShownValue();
    }

    public MovementType GetMovement()
    {
        return (MovementType) movement.value;
    }
}

[Serializable]
public class FormSpellNodeUiData
{
    public UnityEngine.UI.Slider size;
    public UnityEngine.UI.InputField sizeInput;
    public UnityEngine.UI.Toggle autoLink;
    public Dropdown form;

    public void SetSize( float value )
    {
        size.value = value < size.maxValue ? value : size.maxValue;
        size.value = size.value > size.minValue ? size.value : size.minValue;
    }

    public float GetSize()
    {
        return size.value;
    }

    public void OnSizeChanged()
    {
        sizeInput.text = size.value.ToString("F2");
    }

    public bool IsAutoLink()
    {
        return autoLink.isOn;
    }

    public void SetForm( FormType formType )
    {
        form.value = (int) formType;
        form.RefreshShownValue();
    }

    public FormType GetForm()
    {
        return (FormType) form.value;
    }
}

[Serializable]
public class ElementalSpellNodeUiData
{
    public UnityEngine.UI.Slider emissionRate;
    public UnityEngine.UI.InputField emissionValue;
    public UnityEngine.UI.Toggle autoActivate;
    public Dropdown fluidState;
    public Dropdown element;

    public void SetEmissionRate( float value )
    {
        emissionRate.value = value < emissionRate.maxValue ? value : emissionRate.maxValue;
        emissionRate.value = emissionRate.value > emissionRate.minValue ? emissionRate.value : emissionRate.minValue;
    }

    public void OnEmissionRateChanged( float value )
    {
        emissionValue.text = value.ToString("F1");
    }

    public float GetEmissionRate()
    {
        return emissionRate.value;
    }

    public bool IsAutoActive()
    {
        return autoActivate.isOn;
    }

    public ElementPhase GetFluidState()
    {
        return (ElementPhase) fluidState.value;
    }

    public void SetFluidState( ElementPhase ep )
    {
        fluidState.value = (int) ep;
        fluidState.RefreshShownValue();
    }

    public void SetMaxEmissionRate( float maxRate )
    {
        emissionRate.maxValue = maxRate;
    }

    public void SetElement( ElementType elementType )
    {
        element.value = (int) elementType;
        element.RefreshShownValue();
    }

    public ElementType GetElement()
    {
        return (ElementType) element.value;
    }
}
