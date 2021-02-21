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
    [SerializeField]
    public TransitionLinkUiData transitionData;
    [SerializeField]
    public DataLinkUiData dataData;

    public RectTransform UiBoard;
    public RightClickMenu rcm;
    public GameObject uiSpellNodePrefab;
    public GameObject uiLinkPrefab;
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
    UILineRender selectedLink;
    ModifyableUITypes mostRecentlySelected;
    Spell currentSpell;
    Vector2 offset;
    bool drag;
    GameObject dragableObj;
    SelectionSpace selectedSpace;
    SelectionMode selectionMode;
    List<UISpellNode> uiSpellNodeList = new List<UISpellNode>();
    List<UILineRender> uiLinkList = new List<UILineRender>();

    void Start()
    {
        McDetailsPanel.SetPageIndex( 0 );
    }

    void Update()
    {
        if( Input.GetMouseButtonDown(0) )
        {
            // if left click is pressed...
            if( selectionMode == SelectionMode.None )
            {
                // Checking if a spell node in the game world is clicked.  This is
                // to allow the user to drag the spell node around in user space
                Vector3 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Collider2D obj =  Physics2D.OverlapPoint( (Vector2) mousePoint );
                if( obj != null )
                {
                    SpellNode sn = obj.GetComponent<SpellNode>();
                    if( sn != null )
                    {
                            // drag = true;
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
       if( selectedSpellNode == null && selectedLink == null )
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
       if( Input.GetMouseButtonDown(1) && (selectedLink == null || (selectedLink != null && !selectedLink.isClicked)) )
       {
           DeselectAll();
       }

       // Stop dragging when you release the mouse
       if( Input.GetMouseButtonUp(0) )
       {
           if( selectionMode == SelectionMode.Drag )
           {
               // drag = false;
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
       else if( selectionMode == SelectionMode.TransitionLink || selectionMode == SelectionMode.DataLink )
       {
           selectedLink.SetPoint( selectedLink.points.Count - 1, Input.mousePosition );
       }
    }

    public void DeselectAll()
    {
        if( selectionMode == SelectionMode.TransitionLink || selectionMode == SelectionMode.DataLink )
        {
            selectedLink.deleteOnNoSource = true;
        }
        selectedSpellNode = null;
        selectedLink = null;
        selectionMode = SelectionMode.None;
    }

    public void UpdateSelectedUISpellNode( UISpellNode uisn )
    {
        if( selectionMode == SelectionMode.None )
        {
            // drag = true;
            selectionMode = SelectionMode.Drag;
            dragableObj = uisn.gameObject;
            offset = (Vector2)(uisn.transform.position - Input.mousePosition);
            selectedSpace = SelectionSpace.UI;
            UpdateSelectedSpellNode( uisn.linkedSpellNode );
            mostRecentlySelected = ModifyableUITypes.Node;
        }
        else if( selectionMode == SelectionMode.DataLink || selectionMode == SelectionMode.TransitionLink )
        {
            Debug.Log("Creating Link with source " + selectedUISN.gameObject.name + " and destination " + uisn.gameObject.name);
            CreateLink( selectedUISN, uisn );
            mostRecentlySelected = ModifyableUITypes.Link;
        }
    }

    public void UpdateSelectedLink( UILineRender uilr )
    {
        if( selectionMode == SelectionMode.None )
        {
            mostRecentlySelected = ModifyableUITypes.Link;
            print("Updating the selected link");
            selectedLink = uilr;
            UpdateDetailsPanel( ((int)MagicCircleType.END) + ((int)selectedLink.linkedLink.GetLinkType()) );
            UpdateLinkText();
        }
    }

    public void UpdateSelectedSpellNode( SpellNode sn )
    {
        if( selectedSpellNode != sn)
        {
            selectedSpellNode = sn;
        }
        UpdateDetailsPanel( (int)selectedSpellNode.GetMcType() );
    }

    void UpdateDetailsPanel( int index )
    {
        if( McDetailsPanel.GetPageIndex() != index )
        {
            McDetailsPanel.SetPageIndex( index );
            Debug.Log("Setting Details Panel index to " + index );
        }
        if( index < (int)MagicCircleType.END )
        {
            if( index == 0 )
                return;

            detailsText.text = ((MagicCircleType) index ).ToString() + " Details";
            switch( (MagicCircleType) index)
            {
                case MagicCircleType.Element:
                {
                    OnElementalUILoad();
                    break;
                }
                case MagicCircleType.Form:
                {
                    OnFormUILoad();
                    break;
                }
                case MagicCircleType.Movement:
                {
                    OnMovementUILoad();
                    break;
                }
                case MagicCircleType.Input:
                {
                    OnInputUILoad();
                    break;
                }
                case MagicCircleType.Logic:
                {
                    OnLogicUILoad();
                    break;
                }
                default:
                    break;
            }
        }
        else
        {
            detailsText.text = ((LinkTypes) (index - (int)MagicCircleType.END) ).ToString() + " Details";
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

    public void UpdateRightClickMenuWithLink( UILineRender uiln )
    {
        if( uiln != null )
        {
            rcm.ActivateIndex( (int) ModifyableUITypes.Link );
        }
        selectedLink = uiln;
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
        mcl.SetSource( source.linkedSpellNode );
        mcl.SetDestination( destination.linkedSpellNode );
        selectionMode = SelectionMode.None;
        selectedLink.linkedLink = mcl;
        selectedLink.mcmm = this;
        Vector3 offsetMe = Input.mousePosition - destination.transform.position;
        print(offsetMe.ToString("F3"));
        selectedLink.SetReference( selectedLink.points.Count-1, destination, offsetMe );
        selectedLink.deleteOnNoSource = true;
        UpdateSelectedLink( selectedLink );
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
            selectedUISN = usn;
        }
    }

    public void CreateSpell()
    {
        GameObject objSpell = new GameObject();
        currentSpell = (Spell) objSpell.AddComponent<Spell>();
        currentSpell.name = "New Generic Spell";
        currentSpell.mcmm = this;
    }

    public void DeleteSelected()
    {
        if( mostRecentlySelected == ModifyableUITypes.Node && selectedUISN != null )
        {
            selectedUISN.Delete();
        }
        else if( mostRecentlySelected == ModifyableUITypes.Link && selectedLink != null )
        {
            selectedLink.Delete();
        }
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

    public void ActivateMagicCircle( bool shouldActivate )
    {
        MagicCircle mc = (MagicCircle)selectedUISN.linkedSpellNode;
        if( mc != null )
        {
            if( shouldActivate )
            {
                mc.Activate();
            }
            else
            {
                mc.Deactivate();
            }
        }
    }

    public void UpdateWithCreatedLink( MagicCircleLinks mcl )
    {
        GameObject obj = (GameObject) Instantiate( uiLinkPrefab, UiBoard );
        UILineRender uiLine = obj.GetComponent<UILineRender>();
        uiLine.linkedLink = mcl;
        uiLine.mcmm = this;
        uiLine.points.Clear();
        Vector3 offset = ( mcl.GetLinkType() == LinkTypes.Transition ? Vector3.up * 5f : Vector3.zero);
        for(int i = 0; i < uiSpellNodeList.Count; i++)
        {
            if( uiSpellNodeList[i].linkedSpellNode == mcl.source )
            {
                uiLine.AddReference( uiSpellNodeList[i], offset);
                break;
            }
        }
        for(int i = 0; i < uiSpellNodeList.Count; i++)
        {
            if( uiSpellNodeList[i].linkedSpellNode == mcl.destination )
            {
                uiLine.AddReference( uiSpellNodeList[i], offset );
                break;
            }
        }
        // uiLine.AddReference( selectedUISN );
        uiLinkList.Add( uiLine );
        uiLine.deleteOnNoSource = true;
    }

    public void SetLinkMode( int link )
    {
        Debug.Log("Setting link mode to " + link );
        switch( (LinkTypes) link )
        {
            case LinkTypes.Transition:
            {
                selectionMode = SelectionMode.TransitionLink;
                GameObject obj = (GameObject) Instantiate( uiLinkPrefab, UiBoard );
                UILineRender uiLine = obj.GetComponent<UILineRender>();
                uiLine.points.Clear();
                uiLine.AddReference( selectedUISN );
                uiLine.AddPoint( Input.mousePosition );
                uiLinkList.Add( uiLine );
                selectedLink = uiLine;
                break;
            }
            case LinkTypes.Data:
            {
                selectionMode = SelectionMode.DataLink;
                GameObject obj = (GameObject) Instantiate( uiLinkPrefab, UiBoard );
                UILineRender uiLine = obj.GetComponent<UILineRender>();
                uiLine.points.Clear();
                uiLine.AddReference( selectedUISN );
                uiLine.AddPoint( Input.mousePosition );
                uiLinkList.Add( uiLine );
                selectedLink = uiLine;
                break;
            }
            default:
            {
                Debug.LogWarning(" SetLinkMode link type not implemented ");
                return;
            }
        }
    }

    public void AutoLinkDataToNode()
    {
        if( selectedUISN != null )
        {
            ElementMagicCircle emc = (ElementMagicCircle) selectedUISN.linkedSpellNode;
            if( emc != null )
            {
                currentSpell.initialMagicCircle = emc;
            }
            else
            {
                Debug.LogWarning("Can't auto link data to node. Element Node not selected");
            }
        }
        else
        {
            Debug.LogWarning("Can't auto link data to node. No node selected");
        }
    }

    public void EnableAutoLinkData( bool shouldEnable )
    {
        currentSpell.autoLinkMagicCircle = shouldEnable;
    }


#region ElementUiCallbacks
    public void OnElementalUILoad()
    {
        ElementMagicCircle emc = (ElementMagicCircle) selectedSpellNode;
        if( emc != null )
        {
            elementalData.SetEmissionRate( emc.emissionRate.Value() );
            elementalData.SetElement( emc.GetElement() );
            elementalData.autoActivate.isOn = emc.autoActivate;
            elementalData.SetFluidState( emc.elementPhase );
        }
    }

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
    public void OnFormUILoad()
    {
        // public UnityEngine.UI.Slider size;
        // public UnityEngine.UI.InputField sizeInput;
        // public Dropdown form;
        FormMagicCircle fmc = (FormMagicCircle) selectedSpellNode;
        if( fmc != null )
        {
            formData.SetSize( fmc.sizeMultiplier.Value() );
            formData.SetForm( fmc.GetForm() );
        }
    }

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

    public void OnFormRotationChange( float value )
    {
        formData.OnRotationChanged();
        FormMagicCircle fmc = (FormMagicCircle) selectedSpellNode;
        if( fmc != null )
        {
            fmc.rotation.SetDefaultValue( formData.GetRotation() );
        }
    }

    public void OnFormSizeEndEdit( string value )
    {
        formData.SetSize( float.Parse( value ) );
    }

    public void OnFormRotationEndEdit( string value )
    {
        formData.SetRotation( float.Parse( value ) );
    }
#endregion

#region MovementUiCallbacks
    public void OnMovementUILoad()
    {
        // public UnityEngine.UI.Slider initialVelocity;
        // public UnityEngine.UI.InputField initialVelocityInput;
        // public UnityEngine.UI.Slider force;
        // public UnityEngine.UI.InputField forceInput;
        // public UnityEngine.UI.Slider angle;
        // public UnityEngine.UI.InputField angleInput;
        // public UnityEngine.UI.Toggle dragMagic;
        // public Dropdown movement;
        MovementMagicCircle mmc = (MovementMagicCircle) selectedSpellNode;
        if( mmc != null )
        {
            UpdateMovementValues( mmc );
            movementData.SetInitialVelocity( mmc.initialVelocity.Value() );
            movementData.SetForce( mmc.force.Value() );
            movementData.SetAngle( mmc.directionAngle.Value() );
            movementData.SetMovement( mmc.GetMovement() );
            movementData.dragMagic.isOn = mmc.dragMagic;
        }
    }
    public void OnMovementChanged( int value )
    {
        MovementMagicCircle mmc = (MovementMagicCircle) selectedSpellNode;
        if( mmc != null )
        {
            mmc.SetMovement( movementData.GetMovement() );
            UpdateMovementValues( mmc );
        }
    }

    private void UpdateMovementValues( MovementMagicCircle mmc )
    {
        switch( mmc.GetMovement() )
        {
            case MovementType.Control:
            {
                movementData.SetMaxForce( 50 );
                movementData.SetMinForce( 0 );
                break;
            }
            case MovementType.Path:
            {
                goto case MovementType.Control;
            }
            case MovementType.Push:
            {
                movementData.SetMaxForce( 5 );
                movementData.SetMinForce( -5 );
                break;
            }
            default:
            {
                goto case MovementType.Push;
            }
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
    public void OnInputUILoad()
    {
        UserInputNode uin = (UserInputNode) selectedSpellNode;
        if( uin != null )
        {
            inputData.inputCharacter.text = uin.GetKeyString() ?? null;
        }
    }
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
    public void OnLogicUILoad()
    {
        LogicNode ln = (LogicNode) selectedSpellNode;
        if( ln != null )
        {
            logicData.logicDataType.value = (int)ln.inputDataType;
            logicData.logicDataType.RefreshShownValue();
            logicData.logicCompareType.value = (int)ln.compareType;
            logicData.logicCompareType.RefreshShownValue();
            if( ln.inputDataType == InputDataType.Vector )
            {
                logicData.logicLeftHandInput1.text = ((Vector2)ln.leftHandValue.Value()) != null ? ((Vector2)ln.leftHandValue.Value()).x.ToString() : null;
                logicData.logicLeftHandInput2.text = ((Vector2)ln.leftHandValue.Value()) != null ? ((Vector2)ln.leftHandValue.Value()).x.ToString() : null;
                logicData.logicRightHandInput1.text = ((Vector2)ln.rightHandValue.Value()) != null ? ((Vector2)ln.rightHandValue.Value()).y.ToString() : null;
                logicData.logicRightHandInput2.text = ((Vector2)ln.rightHandValue.Value()) != null ? ((Vector2)ln.rightHandValue.Value()).y.ToString() : null;
            }
            else
            {
                logicData.logicLeftHandInput1.text = ln.leftHandValue.Value() != null ? ln.leftHandValue.Value().ToString() : null;
                logicData.logicRightHandInput1.text = ln.rightHandValue.Value() != null ? ln.rightHandValue.Value().ToString() : null;
            }
        }
    }
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
        LogicNode ln = (LogicNode) selectedSpellNode;
        if( ln != null )
        {
            ln.inputDataType = (InputDataType)( value );
        }
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

#region TransitionLinkCallbacks
    public void OnTransitionDelayChange( float value )
    {
        transitionData.OnDelayChanged();
        MagicCircleTransitionLinks tl = (MagicCircleTransitionLinks) selectedLink.linkedLink;
        if( tl != null )
        {
            tl.delayTime = transitionData.GetDelay();
        }
    }

    public void OnTransitionDelayEndEdit( string value )
    {
        transitionData.SetDelay( float.Parse( value ) );
    }

    private void UpdateTransitionDelay()
    {
        MagicCircleTransitionLinks tl = (MagicCircleTransitionLinks) selectedLink.linkedLink;
        if( tl != null )
        {
            transitionData.SetDelay( tl.delayTime );
        }
    }

    public void UpdateLinkText()
    {
        if( selectedLink.linkedLink.GetLinkType() == LinkTypes.Transition )
        {
            transitionData.sourceLabel.text = selectedLink.linkedLink.source.gameObject.name;
            transitionData.destinationLabel.text = selectedLink.linkedLink.destination.gameObject.name;
            UpdateTransitionDelay();
        }
        else
        {
            dataData.sourceLabel.text = selectedLink.linkedLink.source.gameObject.name;
            dataData.destinationLabel.text = selectedLink.linkedLink.destination.gameObject.name;
            UpdateDataInverseToggle();
            UpdateDataSourceProperties();
            UpdateDataDestinationProperties();
        }
    }

    public void OnSwapSourceDestination()
    {
        SpellNode tempSource = selectedLink.linkedLink.source;
        selectedLink.linkedLink.SetSource( selectedLink.linkedLink.destination );
        selectedLink.linkedLink.SetDestination( tempSource );
        UpdateLinkText();
    }
#endregion

#region DataLinkCallbacks
    public void OnDataSourcePropertyChanged( int value )
    {
        MagicCircleDataLinks dl = (MagicCircleDataLinks) selectedLink.linkedLink;
        if( dl != null )
        {
            if( value > 0 && value <= dl.availableProperties.Count )
            {
                dl.selectedProperty = dl.availableProperties[value-1];
            }
            dl.link = true;
        }
    }

    public void UpdateDataSourceProperties()
    {
        MagicCircleDataLinks dl = (MagicCircleDataLinks) selectedLink.linkedLink;
        if( dl != null )
        {
            dataData.sourceProperty.ClearOptions();
            List<string> temp = new List<string>();
            temp.Add("----");
            temp.AddRange( dl.availableProperties );
            dataData.sourceProperty.AddOptions( temp );
            dataData.sourceProperty.value = (int)( Mathf.Max( dl.availableProperties.FindIndex( x => x == dl.selectedProperty ) + 1, 0 ) );
            dataData.sourceProperty.RefreshShownValue();
        }
    }

    public void OnDataInverseChanged( bool value )
    {
        MagicCircleDataLinks dl = (MagicCircleDataLinks) selectedLink.linkedLink;
        if( dl != null )
        {
            dl.invert = value;
        }
    }

    public void UpdateDataInverseToggle()
    {
        MagicCircleDataLinks dl = (MagicCircleDataLinks) selectedLink.linkedLink;
        if( dl != null )
        {
            dataData.inverseToggle.isOn = dl.invert;
        }
    }

    public void OnDataDestinationPropertyChanged( int value )
    {
        MagicCircleDataLinks dl = (MagicCircleDataLinks) selectedLink.linkedLink;
        if( dl != null )
        {
            if( dataData.usePropertyToggle.isOn )
            {
                if( value > 0 && value <= dl.availableLinkableProperties.Count  )
                {
                    dl.selectedLinkableProperty = dl.availableLinkableProperties[value-1];
                    dl.selectedActivatableFunction = "";
                }
            }
            else if( dataData.useFunctionToggle.isOn )
            {
                if( value > 0 && value <= dl.activatableFunctions.Count )
                {
                    dl.selectedLinkableProperty = "";
                    dl.selectedActivatableFunction = dl.activatableFunctions[value-1];
                }
            }
            dl.link = true;
        }
    }

    public void UpdateDataDestinationProperties()
    {
        MagicCircleDataLinks dl = (MagicCircleDataLinks) selectedLink.linkedLink;
        if( dl != null )
        {
            dataData.destinationProperty.ClearOptions();
            if( selectedLink.isProperty )
            {
                dataData.usePropertyToggle.isOn = true;
            }
            else
            {
                dataData.useFunctionToggle.isOn = true;
            }
            OnDataUsePropertyChanged( dataData.usePropertyToggle.isOn );
            OnDataUseFunctionChanged( dataData.useFunctionToggle.isOn );
        }
    }

    public void OnDataUsePropertyChanged( bool value)
    {
        MagicCircleDataLinks dl = (MagicCircleDataLinks) selectedLink.linkedLink;
        if( dl != null )
        {
            if( value )
            {
                dataData.destinationProperty.ClearOptions();
                List<string> temp = new List<string>();
                temp.Add("----");
                temp.AddRange( dl.availableLinkableProperties );
                dataData.destinationProperty.AddOptions( temp );
                dataData.destinationProperty.value = (int)( Mathf.Max( dl.availableLinkableProperties.FindIndex( x => x == dl.selectedLinkableProperty ) + 1, 0 ) );
                dataData.destinationProperty.RefreshShownValue();
                selectedLink.isProperty = true;
                OnDataDestinationPropertyChanged( dataData.destinationProperty.value );
            }
        }
    }

    public void OnDataUseFunctionChanged( bool value)
    {
        MagicCircleDataLinks dl = (MagicCircleDataLinks) selectedLink.linkedLink;
        if( dl != null )
        {
            if( value )
            {
                dataData.destinationProperty.ClearOptions();
                List<string> temp = new List<string>();
                temp.Add("----");
                temp.AddRange( dl.activatableFunctions );
                dataData.destinationProperty.AddOptions( temp );
                dataData.destinationProperty.value = (int)( Mathf.Max( dl.activatableFunctions.FindIndex( x => x == dl.selectedActivatableFunction ) + 1, 0 ) );
                dataData.destinationProperty.RefreshShownValue();
                selectedLink.isProperty = false;
                OnDataDestinationPropertyChanged( dataData.destinationProperty.value );
            }
        }
    }
#endregion

}

[Serializable]
public class DataLinkUiData
{
    public UnityEngine.UI.Button swapSourceDestination;
    public UnityEngine.UI.Text sourceLabel;
    public UnityEngine.UI.Text destinationLabel;
    public UnityEngine.UI.Toggle usePropertyToggle;
    public UnityEngine.UI.Toggle useFunctionToggle;
    public UnityEngine.UI.Toggle inverseToggle;
    public Dropdown sourceProperty;
    public Dropdown destinationProperty;

}

[Serializable]
public class TransitionLinkUiData
{
    public UnityEngine.UI.Slider delay;
    public UnityEngine.UI.InputField delayInput;
    public UnityEngine.UI.Button swapSourceDestination;
    public UnityEngine.UI.Text sourceLabel;
    public UnityEngine.UI.Text destinationLabel;

    public void SetDelay( float value )
    {
        delay.value = value < delay.maxValue ? value : delay.maxValue;
        delay.value = delay.value > delay.minValue ? delay.value : delay.minValue;
    }

    public float GetDelay()
    {
        return delay.value;
    }

    public void OnDelayChanged()
    {
        delayInput.text = delay.value.ToString("F2");
    }

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

    public void SetMaxForce( float value )
    {
        force.maxValue = value;
    }

    public void SetMinForce( float value )
    {
        force.minValue = value;
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
    public UnityEngine.UI.Slider rotation;
    public UnityEngine.UI.InputField rotationInput;
    public Dropdown form;

    public void SetSize( float value )
    {
        size.value = value < size.maxValue ? value : size.maxValue;
        size.value = size.value > size.minValue ? size.value : size.minValue;
    }

    public void SetRotation( float value )
    {
        rotation.value = value < rotation.maxValue ? value : rotation.maxValue;
        rotation.value = rotation.value > rotation.minValue ? rotation.value : rotation.minValue;
    }

    public float GetSize()
    {
        return size.value;
    }
    public float GetRotation()
    {
        return rotation.value;
    }

    public void OnSizeChanged()
    {
        sizeInput.text = size.value.ToString("F2");
    }

    public void OnRotationChanged()
    {
        rotationInput.text = rotation.value.ToString("F2");
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
