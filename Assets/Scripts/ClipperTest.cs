#define use_int32
#undef use_lines
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;
using Unity.Collections;
using ClipperLib;

using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
// using Path = List<int>;

public class ClipperTest : MonoBehaviour
{
    public static Dictionary<int,TriggerColliderData> particleTriggerColliders = new Dictionary<int,TriggerColliderData>();
    bool addedTriggerCollider = false;
    int triggerCollider;
    ParticleSystem ps;
    List<int> blarg;
    public static int clipperPrecision = 1000;
    public float particleWidth = 0.5f;
    Paths solution;

    public float timeToCreatePaths = 0;
    public float timeToExecutePaths = 0;
    public float timeToDrawPaths = 0;

    public bool drawBounds = true;
    public float sizeOverLifetimeCutoff = .25f;
    public float maxParticleCollisionEffectDistance = 1;
    public static int maxParticleCollisions = 200;
    public LinkableData<ActivationFunction> ActivateOnCollision;
    public LinkableData<ActivationFunction> ActivateOnTrigger;

    PolygonCollider2D myCollider;
    List<ParticleCollisionEvent> psCollisionEvents;
    List<ParticleCollisionData> collisionPoints;
    ParticleCollisionJob particleCollisionJob = new ParticleCollisionJob();
    ParticleTriggerCollisionJob particleTriggerCollisionJob = new ParticleTriggerCollisionJob();
    List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();
    bool scheduleCollision = false;
    float collisionBatchTime = 0;
    public float maxCollisionBatchTime = .05f;
    ParticleMagic myMagic;

    bool solidMagicCollision = false;
    float solidMagicCollisionResetTime = 0;
    bool nonSolidMagicCollision = false;
    float nonSolidMagicCollisionResetTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        if( ActivateOnCollision == null )
        {
            ActivateOnCollision = new LinkableData<ActivationFunction>( PrintCollision );
        }
        if( ActivateOnTrigger == null )
        {
            ActivateOnTrigger = new LinkableData<ActivationFunction>( PrintTriggerCollision );
        }
        solution = new Paths();
        ps = GetComponent<ParticleSystem>();
        myMagic = GetComponent<ParticleMagic>();
        myCollider = GetComponent<PolygonCollider2D>();
        collisionPoints = new List<ParticleCollisionData>();
        if( myCollider == null )
        {
            myCollider = gameObject.AddComponent<PolygonCollider2D>();
        }
        psCollisionEvents = new List<ParticleCollisionEvent>();
    }

    void FixedUpdate()
    {
        if( collisionPoints.Count > 0 )
        {
            if( collisionBatchTime > maxCollisionBatchTime )
            {
                particleCollisionJob.SetCollisionPoints( collisionPoints );
                collisionPoints.Clear();
                collisionBatchTime = 0;
                scheduleCollision = true;
            }
            else
            {
                collisionBatchTime += Time.fixedDeltaTime;
            }
        }

        if( solidMagicCollisionResetTime > 0 )
        {
            solidMagicCollisionResetTime -= Time.fixedDeltaTime;
        }
        else
        {
            solidMagicCollision = false;
        }

        if( nonSolidMagicCollisionResetTime > 0 )
        {
            nonSolidMagicCollisionResetTime -= Time.fixedDeltaTime;
        }
        else
        {
            nonSolidMagicCollision = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        particleCollisionJob.maxParticleCollisionEffectDistance = maxParticleCollisionEffectDistance;
        particleCollisionJob.deltaTime = Time.deltaTime;
        particleTriggerCollisionJob.deltaTime = Time.deltaTime;
        int currentTime = System.DateTime.Now.Millisecond;
        ParticleSystem.Particle[] myParticles = new ParticleSystem.Particle[ps.main.maxParticles];
        int maxParticles = ps.GetParticles( myParticles );

        Clipper c = new Clipper();
        float sizeMultiplier = 1;
        int realMaxParticles = 0;
        // for( int i = 0; i < maxParticles; i++ )
        // {
        //     var psSOL = ps.sizeOverLifetime;
        //     if( psSOL.enabled )
        //     {
        //         sizeMultiplier = psSOL.size.Evaluate( 1 - (myParticles[i].remainingLifetime / myParticles[i].startLifetime) );
        //     }
        //     if( sizeMultiplier >= sizeOverLifetimeCutoff )
        //     {
        //         realMaxParticles++;
        //     }
        // }
        Paths subj = new Paths();
        int addedPaths = 0;
        for( int i = 0; i < maxParticles; i++ )
        {
            var psSOL = ps.sizeOverLifetime;
            if( psSOL.enabled )
            {
                sizeMultiplier = psSOL.size.Evaluate( 1 - (myParticles[i].remainingLifetime / myParticles[i].startLifetime) );
            }
            if( sizeMultiplier >= sizeOverLifetimeCutoff )
            {
                subj.Add( new Path(4) );
                subj[addedPaths].Add(new IntPoint( (int) ( (myParticles[i].position.x - sizeMultiplier*particleWidth/2f) * clipperPrecision),  (int) ( (myParticles[i].position.y - sizeMultiplier*particleWidth/2f) * clipperPrecision) ) );
                subj[addedPaths].Add(new IntPoint( (int) ( (myParticles[i].position.x + sizeMultiplier*particleWidth/2f) * clipperPrecision),  (int) ( (myParticles[i].position.y - sizeMultiplier*particleWidth/2f) * clipperPrecision) ) );
                subj[addedPaths].Add(new IntPoint( (int) ( (myParticles[i].position.x + sizeMultiplier*particleWidth/2f) * clipperPrecision),  (int) ( (myParticles[i].position.y + sizeMultiplier*particleWidth/2f) * clipperPrecision) ) );
                subj[addedPaths].Add(new IntPoint( (int) ( (myParticles[i].position.x - sizeMultiplier*particleWidth/2f) * clipperPrecision),  (int) ( (myParticles[i].position.y + sizeMultiplier*particleWidth/2f) * clipperPrecision) ) );
                addedPaths++;
            }
        }

        IntRect boundingRect = ClipperBase.GetBounds(subj);
        Paths clipperPath = new Paths( 1 );
        clipperPath.Add( new Path(4) );
        clipperPath[0].Add( new IntPoint( boundingRect.left, boundingRect.bottom ) );
        clipperPath[0].Add( new IntPoint( boundingRect.right, boundingRect.bottom ) );
        clipperPath[0].Add( new IntPoint( boundingRect.right, boundingRect.top ) );
        clipperPath[0].Add( new IntPoint( boundingRect.left, boundingRect.top ) );

        c.AddPaths(subj, PolyType.ptSubject, true);
        c.AddPaths(clipperPath, PolyType.ptClip, true);

        timeToCreatePaths = (System.DateTime.Now.Millisecond - currentTime)/1000f;
        currentTime = System.DateTime.Now.Millisecond;

        c.Execute(ClipType.ctIntersection, solution,
            PolyFillType.pftNonZero, PolyFillType.pftNonZero);

        timeToExecutePaths = (System.DateTime.Now.Millisecond - currentTime)/1000f;
        currentTime = System.DateTime.Now.Millisecond;

        myCollider.pathCount = solution.Count;
        // List<Vector2> colliderPaths = new List<Vector2>();
        for( int i = 0; i < solution.Count; i++ )
        {
            Vector2[] colliderPaths = new Vector2[solution[i].Count];
            for( int j = 0; j < solution[i].Count; j++ )
            {
                Vector2 tempPoints = new Vector2( (float)solution[i][j].X / clipperPrecision - transform.position.x, (float)solution[i][j].Y / clipperPrecision  - transform.position.y);
                colliderPaths[j] = Quaternion.Inverse( transform.rotation ) * ((Vector3)(tempPoints));
                if( drawBounds )
                {
                    Debug.DrawLine(
                        new Vector2( (float)solution[i][j].X / clipperPrecision, (float)solution[i][j].Y / clipperPrecision ),
                        new Vector2( (float)solution[i][(j+1)%solution[i].Count].X / clipperPrecision, (float)solution[i][(j+1)%solution[i].Count].Y / clipperPrecision ),
                        Color.magenta
                        );
                }
            }
            if( i < 100 )
            {
                myCollider.SetPath(i,colliderPaths);
            }
        }
        timeToDrawPaths = (System.DateTime.Now.Millisecond - currentTime)/1000f;
        currentTime = System.DateTime.Now.Millisecond;

        if( myCollider.isTrigger )
        {
            if( !particleTriggerColliders.ContainsKey( myCollider.gameObject.GetInstanceID() ) )
            {
                particleTriggerColliders.Add( myCollider.gameObject.GetInstanceID(), new TriggerColliderData( myCollider, solution, myCollider.bounds ) );
            }
            else
            {
                particleTriggerColliders[myCollider.gameObject.GetInstanceID()].path = solution;
                particleTriggerColliders[myCollider.gameObject.GetInstanceID()].bounds = myCollider.bounds;
            }
        }

    }

    ParticleMagic GetMyMagic()
    {
        return myMagic;
    }

    void AddCollisionPoints( ParticleCollisionData pcd )
    {
        collisionPoints.Add(pcd);
    }

    void OnParticleCollision( GameObject other )
    {
        ParticlePhysicsExtensions.GetCollisionEvents( ps, other, psCollisionEvents );
        ParticleMagic pm = other.gameObject.GetComponent<ParticleMagic>();
        if( pm != null )
        {
            solidMagicCollision = true;
            solidMagicCollisionResetTime = .1f;
            ActivateOnCollision.Value()();

        //     // List<Vector3> collisionPoints = new List<Vector3>( psCollisionEvents.Count );
            for(int i = 0; i < psCollisionEvents.Count; i++ )
            {
                ParticleCollisionData pcd = new ParticleCollisionData();
                pcd.pointOfCollision = psCollisionEvents[i].intersection;
                pcd.velocity = psCollisionEvents[i].velocity;
                if( myCollider.isTrigger )
                {
                    ClipperTest ct = other.gameObject.GetComponent<ClipperTest>();
                    pcd.elementCollision = myMagic.GetElement();
                    ct.AddCollisionPoints( pcd );
                    // print("ADDING TO OTHER CT");
                }
                else
                {
                    pcd.elementCollision = pm.GetElement();
                    collisionPoints.Add( pcd );
                }
                float markerLength = .2f;
                Debug.DrawLine( psCollisionEvents[i].intersection - Vector3.up * markerLength/2, psCollisionEvents[i].intersection + Vector3.up * markerLength/2, Color.blue );
                Debug.DrawLine( psCollisionEvents[i].intersection - Vector3.right * markerLength/2, psCollisionEvents[i].intersection + Vector3.right * markerLength/2, Color.blue );
                // print("ParticleCollision object " + psCollisionEvents[i].intersection );
            }

        }
    }

    void OnTriggerStay2D( Collider2D other )
    {
        ParticleMagic pm = other.gameObject.GetComponent<ParticleMagic>();
        if( pm != null && myCollider.isTrigger )
        {
            nonSolidMagicCollision = true;
            nonSolidMagicCollisionResetTime = .1f;
            ActivateOnTrigger.Value()();
            particleTriggerCollisionJob.gameObjectID = other.gameObject.GetInstanceID();
        }
        // RaycastHit2D[] myContacts = new RaycastHit2D[30];
        // int count = other.Cast( (transform.position - other.transform.position).normalized, myContacts, 0 );
        // print("On trigger 2D enter contact count " + count );
        // float markerLength = .2f;
        // for( int i = 0; i < count; i++ )
        // {
        //     Debug.DrawLine( myContacts[i].point - Vector2.up * markerLength/2, myContacts[i].point + Vector2.up * markerLength/2, Color.blue );
        //     Debug.DrawLine( myContacts[i].point - Vector2.right * markerLength/2, myContacts[i].point + Vector2.right * markerLength/2, Color.blue );
        // }
    }

    public bool GetSolidMagicCollision()
    {
        return solidMagicCollision;
    }

    public bool GetNonSolidMagicCollision()
    {
        return nonSolidMagicCollision;
    }

    void PrintCollision()
    {
        // print("Collider - Colliding with particle collider");
    }

    void PrintTriggerCollision()
    {
        // print("Trigger - Colliding with trigger particle ");
    }

    void OnParticleUpdateJobScheduled()
    {
        if( scheduleCollision )
        {
            particleCollisionJob.myElement = myMagic.GetElement();
            particleCollisionJob.Schedule( ps, 3000 );
            scheduleCollision = false;
        }
        if( nonSolidMagicCollision )
        {
            particleTriggerCollisionJob.myElement = myMagic.GetElement();
            particleTriggerCollisionJob.isTrigger = myCollider.isTrigger;
            particleTriggerCollisionJob.collideWithTrigger = nonSolidMagicCollision;
            particleTriggerCollisionJob.Schedule( ps, 3000 );
        }
    }

    struct ParticleCollisionData
    {
        public Vector3 pointOfCollision;
        public ElementType elementCollision;
        public Vector3 velocity;
    }

    public class TriggerColliderData
    {
        public Collider2D collider;
        public Vector3 center;
        public ElementType element;
        public ParticleMagic particleMagic;
        public Paths path;
        public Bounds bounds;

        public TriggerColliderData( Collider2D newCollider, Paths newPath, Bounds newBounds )
        {
            collider = newCollider;
            center = collider.bounds.center;
            particleMagic = newCollider.GetComponent<ParticleMagic>();
            element = particleMagic.GetElement();
            path = newPath;
            bounds = newBounds;
        }
    }

    struct ParticleCollisionJob : IJobParticleSystemParallelFor
    {
        public float maxParticleCollisionEffectDistance;
        public float deltaTime;
        public ElementType myElement;
        int collisionPointsCount;
        [DeallocateOnJobCompletionAttribute, ReadOnly]
        private NativeArray<float> collisionPointsX;
        [DeallocateOnJobCompletionAttribute, ReadOnly]
        private NativeArray<float> collisionPointsY;
        [DeallocateOnJobCompletionAttribute, ReadOnly]
        private NativeArray<float> collisionPointsZ;
        [DeallocateOnJobCompletionAttribute, ReadOnly]
        private NativeArray<ElementType> elementCollision;
        [DeallocateOnJobCompletionAttribute, ReadOnly]
        private NativeArray<float> collisionVelocitiesX;
        [DeallocateOnJobCompletionAttribute, ReadOnly]
        private NativeArray<float> collisionVelocitiesY;
        [DeallocateOnJobCompletionAttribute, ReadOnly]
        private NativeArray<float> collisionVelocitiesZ;

        public void Execute( ParticleSystemJobData particles, int i )
        {
            // Collider Collisions
            if( collisionPointsCount > 0 )
            {
                int maxParticles = ClipperTest.maxParticleCollisions > collisionPointsCount ? collisionPointsCount : ClipperTest.maxParticleCollisions;
                for( int j = 0; j < maxParticles; j ++ )
                {
                    float windPushBackPercent = 1;
                    float fireBurnUpPercent = .075f;
                    if( myElement == ElementType.Earth )
                    {
                        windPushBackPercent = windPushBackPercent * .75f;
                        fireBurnUpPercent = fireBurnUpPercent * .75f;
                    }
                    if( elementCollision[j] == ElementType.Wind )
                    {
                        PushBackCollision( particles, i, j, maxParticles, windPushBackPercent );
                    }
                    else if( elementCollision[j] == ElementType.Fire )
                    {
                        KillOtherParticle( particles, i, j, maxParticles, fireBurnUpPercent );
                    }
                }
            }

        }

        void PushBackCollision( ParticleSystemJobData particles, int particleIndex, int collisionIndex, int maxParticles, float percent )
        {
            float particleWeight = collisionPointsCount / maxParticles;
            Vector3 collisionPoint = new Vector3( collisionPointsX[collisionIndex], collisionPointsY[collisionIndex], collisionPointsZ[collisionIndex] );
            Vector3 currentPosition = new Vector3( particles.positions.x[particleIndex], particles.positions.y[particleIndex], particles.positions.z[particleIndex] );
            float distance = Vector2.Distance( collisionPoint, currentPosition );
            if( distance < maxParticleCollisionEffectDistance )
            {
                var aliveTimePercent = particles.aliveTimePercent;
                float distanceForceConst = percent * (1 - ( distance / (maxParticleCollisionEffectDistance * particleWeight) ) ) * deltaTime ;

                var velocitiesX = particles.velocities.x;
                var velocitiesY = particles.velocities.y;
                var velocitiesZ = particles.velocities.z;
                if( velocitiesX[particleIndex] < collisionVelocitiesX[collisionIndex] )
                {
                    velocitiesX[particleIndex] += collisionVelocitiesX[collisionIndex] * distanceForceConst * particleWeight;
                }
                if( velocitiesY[particleIndex] < collisionVelocitiesY[collisionIndex] )
                {
                    velocitiesY[particleIndex] += collisionVelocitiesY[collisionIndex] * distanceForceConst * particleWeight;
                }
                if( velocitiesZ[particleIndex] < collisionVelocitiesZ[collisionIndex] )
                {
                    velocitiesZ[particleIndex] += collisionVelocitiesZ[collisionIndex] * distanceForceConst * particleWeight;
                }
            }
        }

        void KillOtherParticle( ParticleSystemJobData particles, int particleIndex, int collisionIndex, int maxParticles, float percent )
        {
            float particleWeight = collisionPointsCount / maxParticles;
            Vector3 collisionPoint = new Vector3( collisionPointsX[collisionIndex], collisionPointsY[collisionIndex], collisionPointsZ[collisionIndex] );
            Vector3 currentPosition = new Vector3( particles.positions.x[particleIndex], particles.positions.y[particleIndex], particles.positions.z[particleIndex] );
            float distance = Vector2.Distance( collisionPoint, currentPosition );
            if( distance < maxParticleCollisionEffectDistance )
            {
                var aliveTimePercent = particles.aliveTimePercent;
                float distanceDeathConst = percent * (1 - ( distance / (maxParticleCollisionEffectDistance * particleWeight) ) ) * deltaTime ;
                aliveTimePercent[particleIndex] += percent * particleWeight;
            }
        }

        public bool SetCollisionPoints( List<ParticleCollisionData> points )
        {
            float[] pointsx = new float[points.Count];
            float[] pointsy = new float[points.Count];
            float[] pointsz = new float[points.Count];
            ElementType[] elementCol = new ElementType[points.Count];
            float[] velocitiesx = new float[points.Count];
            float[] velocitiesy = new float[points.Count];
            float[] velocitiesz = new float[points.Count];

            collisionPointsCount = points.Count;
            for( int i = 0; i < points.Count; i++ )
            {
                pointsx[i] = points[i].pointOfCollision.x;
                pointsy[i] = points[i].pointOfCollision.y;
                pointsz[i] = points[i].pointOfCollision.z;
                elementCol[i] = points[i].elementCollision;
                velocitiesx[i] = points[i].velocity.x;
                velocitiesy[i] = points[i].velocity.y;
                velocitiesz[i] = points[i].velocity.z;
            }
            collisionPointsX = new NativeArray<float>( pointsx, Unity.Collections.Allocator.TempJob );
            collisionPointsY = new NativeArray<float>( pointsy, Unity.Collections.Allocator.TempJob );
            collisionPointsZ = new NativeArray<float>( pointsz, Unity.Collections.Allocator.TempJob );
            elementCollision = new NativeArray<ElementType>( elementCol, Unity.Collections.Allocator.TempJob);
            collisionVelocitiesX = new NativeArray<float>( velocitiesx, Unity.Collections.Allocator.TempJob );
            collisionVelocitiesY = new NativeArray<float>( velocitiesy, Unity.Collections.Allocator.TempJob );
            collisionVelocitiesZ = new NativeArray<float>( velocitiesz, Unity.Collections.Allocator.TempJob );
            return true;
        }
    }

    struct ParticleTriggerCollisionJob : IJobParticleSystemParallelFor
    {
        public float maxParticleCollisionEffectDistance;
        public float deltaTime;
        public ElementType myElement;
        public bool isTrigger;
        public bool collideWithTrigger;
        public int gameObjectID;

        public void Execute( ParticleSystemJobData particles, int i )
        {
            // If the particle is a trigger Collisions
            if( isTrigger )
            {
                // If the particle is colliding with a trigger
                if( collideWithTrigger && particleTriggerColliders.ContainsKey(gameObjectID) )
                {
                    Vector3 point = new Vector3( particles.positions.x[i], particles.positions.y[i], particles.positions.z[i] );
                    int hit = 0;
                    Vector3 extends = particleTriggerColliders[gameObjectID].bounds.extents;
                    Vector3 center = particleTriggerColliders[gameObjectID].bounds.center;
                    // IntRect bounds = particleTriggerColliders[gameObjectID].bounds;

                    // make sure the particle is within the bounds of the other trigger;
                        // Within left bounds ------------   Within right bounds
                    if( point.x > center.x - extends.x && point.x < center.x + extends.x
                        // Within bottom bounds ------------   Within top bounds
                        && point.y > center.y - extends.y && point.y < center.y + extends.y )
                    {
                        // Check to see if the particle is colliding with the trigger
                        for( int pathsIndex = 0; pathsIndex < particleTriggerColliders[gameObjectID].path.Count; pathsIndex++ )
                        {
                            hit = Clipper.PointInPolygon( new IntPoint( point.x * clipperPrecision, point.y * clipperPrecision ) , particleTriggerColliders[gameObjectID].path[0] );
                            if( hit != 0 )
                            break;
                        }
                        // If we've collided with the trigger then, apply the relevant action
                        if( hit != 0 )
                        {
                            //TODO: Add check for the amount one of particles we are colliding with
                            // If the particle is colliding with fire
                            if( particleTriggerColliders[gameObjectID].element == ElementType.Fire )
                            {
                                KillOtherParticle( particles, i, 100 );
                                PushBackCollision( particles, i, 1 );
                            }
                            // If the particle is colliding with Wind
                            else if( particleTriggerColliders[gameObjectID].element == ElementType.Wind )
                            {
                                PushBackCollision( particles, i, 1 );
                            }
                        }
                    }
                }
            }
        }

        void PushBackCollision( ParticleSystemJobData particles, int particleIndex, float percent )
        {
            ParticleMagic pm = particleTriggerColliders[gameObjectID].particleMagic;
            Vector3 velocity = pm.initialVelocity * pm.direction;

            var velocitiesX = particles.velocities.x;
            var velocitiesY = particles.velocities.y;
            var velocitiesZ = particles.velocities.z;

            // Set the x,y,and z velocities for the push back
            if( Mathf.Abs(velocitiesX[particleIndex]) < Mathf.Abs(velocity.x) || Mathf.Sign(velocitiesX[particleIndex]) != Mathf.Sign(velocity.x) )
            {
                velocitiesX[particleIndex] += velocity.x * deltaTime * percent;
            }
            if( Mathf.Abs(velocitiesY[particleIndex]) < Mathf.Abs(velocity.y) || Mathf.Sign(velocitiesY[particleIndex]) != Mathf.Sign(velocity.y) )
            {
                velocitiesY[particleIndex] += velocity.y * deltaTime * percent;
            }
            if( Mathf.Abs(velocitiesZ[particleIndex]) < Mathf.Abs(velocity.z) || Mathf.Sign(velocitiesZ[particleIndex]) != Mathf.Sign(velocity.z) )
            {
                velocitiesZ[particleIndex] += velocity.z * deltaTime * percent;
            }
        }

        void KillOtherParticle( ParticleSystemJobData particles, int particleIndex, float percent )
        {
            var aliveTimePercent = particles.aliveTimePercent;
            aliveTimePercent[particleIndex] += percent * deltaTime;
        }
    }
}
