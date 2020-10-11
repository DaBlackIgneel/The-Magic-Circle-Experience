using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementMagicCircle : MagicCircle
{
    [SerializeField]
    MovementType myMovement;
    Rigidbody2D movableRb;
    public LinkableData<GameObject> movableMagic;
    public bool autoLinkToElementMagic = true;

    public LinkableData<float> force = new LinkableData<float>(1);
    public LinkableData<float> initialVelocity = new LinkableData<float>(0);
    public LinkableData<float> directionAngle = new LinkableData<float>(0);
    public LinkableData<Vector3> direction = new LinkableData<Vector3>( Vector3.zero );
    public LinkableData<Vector3> targetPosition = new LinkableData<Vector3>( Vector3.zero );
    public float defaultDirectionAngle;
    public float defaultForce;
    public float defaultInitialVelocity;
    public float maxVelocity = 50;
    public bool chooseMagic = true;
    public bool dragMagic = true;
    public PathCreation.PathCreator myPath;
    public PathCreation.EndOfPathInstruction endOfPathInstruction;

    private MagicControllerTracker magicControllerTracker;
    private MagicControllerTracker parentMagicControllerTracker;
    private float distanceTravelled;
    private Vector3 relativePositionOffset;

    public MovementMagicCircle() : base()
    {
        mcType = MagicCircleType.Movement;
        movableMagic = new LinkableData<GameObject>(null);
    }

    // Start is called before the first frame update
    void Start()
    {
        mcType = MagicCircleType.Movement;
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        mySpriteRenderer.color = Color.green;
        if( autoLinkToElementMagic && mcParent.Contains( (int)MagicCircleType.Element ) )
        {
            Debug.Log("Linked Movement to Element");
            ElementMagicCircle emc = mcParent.GetMagicCircle( (int)MagicCircleType.Element ) as ElementMagicCircle;
            movableMagic.SetLinkedValue( emc.GetMagicObject );
        }
    }

    // Update is called once per frame
    void Update()
    {
        defaultForce = force.Value();
        DrawLink();
        if( myMovement == MovementType.Path && myPath == null )
        {
            myPath = gameObject.AddComponent<PathCreation.PathCreator>();
        }
    }

    void FixedUpdate()
    {
        initialVelocity.SetDefaultValue( defaultInitialVelocity );
        directionAngle.SetDefaultValue( defaultDirectionAngle );
        direction.SetDefaultValue( new Vector3( Mathf.Cos( Mathf.Deg2Rad * directionAngle.Value() ), Mathf.Sin( Mathf.Deg2Rad * directionAngle.Value() ), 0 ) );
        targetPosition.SetDefaultValue( Vector3.Scale( Camera.main.ScreenToWorldPoint(Input.mousePosition), new Vector3(1,1,0) ) );
        if( isActive )
        {
            if( movableMagic.Value() != null && magicControllerTracker.IsCurrentMoveController( this ) )
            {
                ParticleMagic pm;
                if( chooseMagic && ( pm = movableMagic.Value().GetComponent<ParticleMagic>() as ParticleMagic ) != null )
                {
                    // Copy all the movement values to the particle magic controller
                    pm.force = force.Value();
                    pm.initialVelocity = initialVelocity.Value();
                    pm.direction = direction.Value();
                    pm.SetMaxVelocity( maxVelocity );

                    // Set the target direction if the magic is to be controlled
                    if( myMovement == MovementType.Control )
                    {
                        pm.SetTarget( targetPosition.Value() );
                    }
                    // Set the path if the magic is to follow the path
                    else if( myMovement == MovementType.Path )
                    {
                        pm.SetPath( myPath.path, endOfPathInstruction );
                    }
                    // If the magic is stopped or pouring, and you want to drag the magic around,
                    // then make sure the magic follows you
                    else if( dragMagic && ( myMovement == MovementType.Stop || myMovement == MovementType.Pour ) && parentMagicControllerTracker.IsCurrentMoveController( this ) )
                    {
                        pm.transform.parent.position = Vector3.Lerp(pm.transform.parent.position, transform.position + relativePositionOffset, .25f );
                    }
                }
                else
                {
                    if( movableRb != null )
                    {
                        Vector2 velocity = Vector2.zero;
                        switch( myMovement )
                        {
                            case MovementType.Path:
                            {
                                Vector3 targetPos = myPath.path.GetPointAtDistance( distanceTravelled, endOfPathInstruction );
                                distanceTravelled += initialVelocity.Value() * Time.fixedDeltaTime;
                                // print( "targetPos: " + targetPos + " transform: " + movableRb.position );
                                // print(" distance travelled ");
                                float useVelocityConst = movableRb.bodyType == RigidbodyType2D.Dynamic ? 1 : 0;
                                velocity = ( (Vector2)(targetPos - movableRb.transform.position) * Mathf.Abs( initialVelocity.Value() ) -
                                            useVelocityConst * movableRb.velocity * (Mathf.Abs( force.Value() ) +1)/10f );
                                break;
                            }
                            case MovementType.Control:
                            {
                                velocity = ( (Vector2)(targetPosition.Value() - movableRb.transform.position) - movableRb.velocity * (Mathf.Abs(force.Value())+1)/10f );
                                break;
                            }
                            default:
                            {
                                break;
                            }
                        }
                        if( movableRb.bodyType == RigidbodyType2D.Dynamic )
                        {
                            movableRb.AddForce( velocity, ForceMode2D.Impulse );
                        }
                        else
                        {
                            movableRb.MovePosition( (Vector2)movableRb.transform.position + velocity * Time.fixedDeltaTime );
                        }
                    }
                }
            }
            else
            {
                distanceTravelled = 0;
            }
        }
        else
        {
            distanceTravelled = 0;
        }
    }

    public override void SetMcType( MagicCircleType newMcType )
    {

    }

    public override void Activate()
    {
        isActive = true;
        if( movableMagic.Value() != null )
        {
            // print( "We got the current magic: " + formableMagic.Value().gameObject.name );
            Debug.Log("Activating movement");

            if( chooseMagic )
            {
                ParticleMagic pm = movableMagic.Value().GetComponent<ParticleMagic>() as ParticleMagic;
                if( pm != null )
                {
                    pm.SetMovement( myMovement );
                }
            }

            magicControllerTracker = movableMagic.Value().GetComponent<MagicControllerTracker>();
            if( magicControllerTracker == null )
            {
                magicControllerTracker = movableMagic.Value().gameObject.AddComponent<MagicControllerTracker>();
            }
            magicControllerTracker.SetCurrentMovementController( this );

            switch( myMovement )
            {
                case MovementType.Push:
                {
                    Debug.Log("Pushing Movement");
                    if( (movableRb = movableMagic.Value().GetComponent<Rigidbody2D>()) == null )
                    {
                        movableRb = movableMagic.Value().AddComponent<Rigidbody2D>();
                    }
                    movableRb.gravityScale = 0;
                    direction.SetDefaultValue( transform.right );
                    movableRb.AddForce( (Vector2) (initialVelocity.Value() * direction.Value().normalized), ForceMode2D.Impulse );
                    break;
                }
                case MovementType.Control:
                {
                    Debug.Log("Controling Movement");
                    if( (movableRb = movableMagic.Value().GetComponent<Rigidbody2D>()) == null )
                    {
                        movableRb = movableMagic.Value().AddComponent<Rigidbody2D>();
                    }
                    movableRb.gravityScale = 0;
                    break;
                }
                case MovementType.Path:
                {
                    Debug.Log("Using Path Movement");
                    if( (movableRb = movableMagic.Value().GetComponent<Rigidbody2D>()) == null )
                    {
                        movableRb = movableMagic.Value().AddComponent<Rigidbody2D>();
                    }
                    movableRb.gravityScale = 0;
                    break;
                }
                case MovementType.Pour:
                {
                    relativePositionOffset = movableMagic.Value().transform.position - transform.position;
                    if( dragMagic && movableMagic.Value().GetComponent<ParticleMagic>() != null )
                    {
                        parentMagicControllerTracker = movableMagic.Value().transform.parent.gameObject.GetComponent<MagicControllerTracker>();
                        if( parentMagicControllerTracker == null )
                        {
                            parentMagicControllerTracker = movableMagic.Value().transform.parent.gameObject.AddComponent<MagicControllerTracker>();
                        }
                        parentMagicControllerTracker.SetCurrentMovementController( this );
                    }
                    Debug.Log("Pouring Movement");
                    break;
                }
                // TODO: add ability to compress when stopped
                case MovementType.Stop:
                {
                    relativePositionOffset = movableMagic.Value().transform.position - transform.position;
                    Debug.Log("Stopping Movement");
                    break;
                }
                default:
                {
                    Debug.LogWarning( "Movement Type not implemented");
                    break;
                }
            }

        }
        else
        {
            Debug.Log("YOOOOO, there aint no element to move the object");
        }
    }

    public override void Deactivate()
    {
        isActive = false;
        movableRb = null;
        distanceTravelled = 0;
        parentMagicControllerTracker = null;
    }

    public void SetMovement( MovementType movement )
    {
        myMovement = movement;
    }

    public override int GetSubType()
    {
        return (int)(myMovement);
    }

    void DrawLink()
    {
        if( movableMagic.GetSource() != null )
        {
            MagicCircle source = (MagicCircle) movableMagic.GetSource();
            Debug.DrawLine( transform.position, source.transform.position, Color.magenta );
        }
    }
}
