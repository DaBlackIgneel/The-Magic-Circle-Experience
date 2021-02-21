using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ClipperLib;

using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

public struct PointReference
{
    public MonoBehaviour mb;
    public int referenceIndex;
    public Vector3 offset;
}

public class UILineRender : Graphic//, IPointerDownHandler
{
    public List<Vector2> points;
    public float lineWidth = 1;
    public MagicCircleLinks linkedLink;
    public MagicCircleMakerMenu mcmm;
    public bool isClicked;
    public bool deleteOnNoSource = false;
    public Vector3 offsetMe;

    // Data Variables
    public bool isProperty = true;

    private List<PointReference> pointRef = new List<PointReference>();
    private float width;
    private float height;
    private float unitWidth;
    private float unitHeight;
    private int clipperPrecision = 1000;
    private Paths myMesh;
    private Path myVertices;

    void Update()
    {
        isClicked = false;
        foreach( PointReference pr in pointRef )
        {
            if( pr.mb != null && pr.referenceIndex < points.Count )
            {
                Vector3 newPosition = pr.mb.transform.position - transform.position + pr.offset;
                if( ((Vector3)points[pr.referenceIndex] - (newPosition)).magnitude > 0.01f )
                {
                    SetAllDirty();
                    points[pr.referenceIndex] = newPosition + offsetMe;
                }
            }
        }
        if( linkedLink != null && ( Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) ) )
        {
            int isClickedInt = 0;
            for(int i = 0; i < myMesh.Count; i++ )
            {
                isClickedInt = Clipper.PointInPolygon( new IntPoint( (Input.mousePosition.x-transform.position.x) * clipperPrecision, (Input.mousePosition.y-transform.position.y) * clipperPrecision ), myMesh[i] );
                if( isClickedInt != 0 )
                {
                    isClicked = true;
                    // if( Input.GetMouseButtonDown(0) )
                    // {
                        OnClicked();
                    // }
                    if( Input.GetMouseButtonDown(1) )
                    {
                        mcmm.UpdateRightClickMenuWithLink( this );
                    }
                    break;
                }
            }
        }
        if( linkedLink != null )
        {
            if( linkedLink.GetLinkType() == LinkTypes.Data )
            {
                color = Color.blue;
            }
            else
            {
                color = Color.green;
            }
        }
        if( deleteOnNoSource )
        {
            if( linkedLink == null || linkedLink.source == null || linkedLink.destination == null )
            {
                Delete();
            }
        }
    }

    public void AddPoint( Vector3 point )
    {
        points.Add( point - transform.position );
    }

    public void AddReference( MonoBehaviour mb, Vector3? offset = null )
    {
        PointReference pr = new PointReference();
        pr.mb = mb;
        pr.referenceIndex = points.Count;
        pr.offset = offset ?? Vector3.zero;
        pointRef.Add( pr );
        AddPoint( mb.transform.position );
    }

    public void SetReference( int index, MonoBehaviour mb, Vector3? offset = null )
    {
        PointReference pr = new PointReference();
        pr.mb = mb;
        pr.referenceIndex = index;
        pr.offset = offset ?? Vector3.zero;
        for( int i = 0; i < pointRef.Count; i++ )
        {
            if( pointRef[i].referenceIndex == index )
            {
                pointRef.Remove( pointRef[i] );
            }
        }
        pointRef.Add( pr );
        SetPoint( index, mb.transform.position + pr.offset );
    }

    public void SetPoint( int index, Vector3 point )
    {
        points[index] = ( point - transform.position );
        SetAllDirty();
    }

    public void OnClicked()
    {
        print("Clicked on a link");
        if( mcmm != null )
        {
            mcmm.UpdateSelectedLink( this );
        }
    }

    protected override void OnPopulateMesh( VertexHelper vh )
    {
        vh.Clear();
        myMesh = new Paths();
        myVertices = new Path();

        width = rectTransform.rect.width;
        height = rectTransform.rect.height;

        unitWidth = 1;//width / (float)gridSize.x;
        unitHeight = 1;//width / (float)gridSize.y;

        for( int i = 0; i < points.Count; i++ )
        {
            Vector2 point = points[i];
            float angle = 0;
            if( i+1 < points.Count )
            {
                angle = Mathf.Rad2Deg * Mathf.Atan2( point.y - points[i+1].y, point.x - points[i+1].x);//Vector3.Angle( points[i+1], point );
            }
            else if( i-1 >= 0 )
            {
                angle = Mathf.Rad2Deg * Mathf.Atan2( points[i-1].y - point.y, points[i-1].x - point.x);//Vector3.Angle( point, points[i-1] );
                print( point.y );
            }
            DrawVerticesForPoint( point, vh, angle );
        }

        for( int i = 0; i < points.Count - 1; i++ )
        {
            int index = i * 2;
            vh.AddTriangle( index + 0, index + 1, index + 3);
            myMesh.Add( new Path(3));
            myMesh[index].Add( myVertices[index + 0] );
            myMesh[index].Add( myVertices[index + 1] );
            myMesh[index].Add( myVertices[index + 3] );
            vh.AddTriangle( index + 3, index + 2, index + 0);
            myMesh.Add( new Path(3));
            myMesh[index+1].Add( myVertices[index + 3] );
            myMesh[index+1].Add( myVertices[index + 2] );
            myMesh[index+1].Add( myVertices[index + 0] );
        }
    }

    void DrawVerticesForPoint( Vector2 point, VertexHelper vh, float angle )
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        vertex.position = Quaternion.Euler(0,0,angle) * (new Vector3( 0, -lineWidth/2 ));
        vertex.position += new Vector3( unitWidth * point.x, unitHeight * point.y);

        vh.AddVert( vertex );
        myVertices.Add( new IntPoint( (int)(vertex.position.x * clipperPrecision), (int)(vertex.position.y * clipperPrecision) ) );

        vertex.position = Quaternion.Euler(0,0,angle) * (new Vector3( 0, lineWidth/2 ));
        vertex.position += new Vector3( unitWidth * point.x, unitHeight * point.y);
        vh.AddVert( vertex );
        myVertices.Add( new IntPoint( (int)(vertex.position.x * clipperPrecision), (int)(vertex.position.y * clipperPrecision) ) );
    }

    public void Delete()
    {
        if( linkedLink != null && linkedLink.GetLinkType() == LinkTypes.Data )
        {
            MagicCircleDataLinks dl = (MagicCircleDataLinks) linkedLink;
            if( isProperty )
            {
                dl.ResetProperty( dl.selectedLinkableProperty );
            }
        }
        Destroy( linkedLink );
        linkedLink = null;
        Destroy( this.gameObject );
    }
}
