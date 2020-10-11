using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;
using Unity;
using Unity.Collections;
using System;

public class ParticleMagic : MonoBehaviour
{
    public static float MINLIFETIME = 0.1f;
    public static int particleLayer = 1;
    int currentLayer;
    ParticleSystem ps;
    ParticleSystem.Particle[] solidStopParticle;
    int solidStopParticleCount;
    Vector3 initialSolidPosition;

    public bool immortalParticleOn = false;
    ImmortalParticleJob immortalJob = new ImmortalParticleJob();
    ShapeParticleJob shapeJob = new ShapeParticleJob();
    MovementParticleJob movementJob = new MovementParticleJob();
    KeepSolidShapeJob keepSolidShapeJob = new KeepSolidShapeJob();

    bool destroyWhenDead = false;
    MovementType psMovementType = MovementType.Pour;
    ParticleSystem.MinMaxCurve noisyNoise;
    float quietNoise = 0.2f;
    bool noiseStartEnabled = true;

    public float force = 0;
    public Vector3 direction = Vector3.right;
    public float sizeMultiplier = 1;

    public float emissionRate = 10;
    public float limitedEmissionRate = 10;
    public float emissionAreaConst = 100;
    public float emisisonMinConst = 20;
    public Vector3 position = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 rotation = new Vector3(0.0f, 0.0f, 90.0f);
    public Vector3 scale = new Vector3(1.0f, 1.0f, 0.0f);

    public float maxVelocity = 10;
    public float initialVelocity = 3;
    public float maxVelocityConst;

    ElementPhase phase = ElementPhase.Liquid;
    ElementType myElement;
    Rigidbody2D rb;

    public bool isEmitting;
    public float area;
    public float perimeter;

    public ShapeArea currentShape;
    public Mesh currentMesh;
    Mesh oldMesh;

    Mesh triangleMesh;
    Mesh circleMesh;
    Mesh rectangleMesh;

    Vector3 initialForce;
    PathCreation.VertexPath myPath;
    PathCreation.EndOfPathInstruction myEndOfPathInstruction;
    float pathDistance = 0;

    Mesh CreateTriangleMesh()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[3]
        {
            new Vector3( .5f, -1/3f, 0),
            new Vector3( -.5f, -1/3f, 0),
            new Vector3( 0, 2/3f, 0)
        };
        int[] triangles = new int[3]
        {
            0,
            1,
            2
        };
        Vector2[] uv = new Vector2[3]
        {
            new Vector2( 0, 0 ),
            new Vector2( 1, 0 ),
            new Vector2( .5f, 1 )
        };


        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.name = "Triangle";
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    Mesh CreateRectangleMesh()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3( -1, 1, 0) * .5f,
            new Vector3( 1, 1, 0) * .5f,
            new Vector3( 1, -1, 0) * .5f,
            new Vector3( -1, -1, 0) * .5f
        };
        int[] triangles = new int[6]
        {
            0,
            1,
            3,
            1,
            2,
            3
        };
        Vector2[] uv = new Vector2[4]
        {
            new Vector2( 0, 1 ),
            new Vector2( 1, 1 ),
            new Vector2( 1, 0 ),
            new Vector2( 0, 0 )
        };


        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.name = "Rectangle";
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    Mesh CreateCircleMesh( int detail )
    {
        Mesh mesh = new Mesh();
        int maxVertices = 4 + detail;
        Vector3[] vertices = new Vector3[maxVertices];
        Vector2[] uv = new Vector2[maxVertices];
        int[] triangles = new int[(maxVertices-1)*3];
        // Vector3[] normals = new Vector3[maxVertices];

        int triangleIndex = 0;
        vertices[0] = new Vector3( 0, 0, 0 );
        for( int i = 1; i < maxVertices; i++ )
        {
            vertices[i] = new Vector3( Mathf.Cos(Mathf.PI * 2 *(i-1)/ (maxVertices-1)), Mathf.Sin(Mathf.PI * 2 * (i-1) / (maxVertices-1)), 0 ) * 0.5f;
            uv[i] = (Vector2) Vector3.Scale(vertices[i] + Vector3.one, new Vector3( 0.5f, 0.5f, 0));
            // uv[i] = new Vector2(0,0);
            // normals[i] = new Vector3(0,0,1);
            if( i > 1 )
            {
                triangles[triangleIndex] = i;
                triangles[triangleIndex+1] = i - 1;
                triangles[triangleIndex+2] = 0;
                triangleIndex += 3;
                if( i+1 == maxVertices )
                {
                    triangles[triangleIndex] = 1;
                    triangles[triangleIndex+1] = i;
                    triangles[triangleIndex+2] = 0;
                }
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        // mesh.normals = normals;
        mesh.RecalculateNormals();
        // mesh.RecalculateBounds();
        mesh.name = "Circle";
        return mesh;
    }

    void Start()
    {
        currentLayer = particleLayer;
        gameObject.layer = LayerMask.NameToLayer("ParticleLayer" + particleLayer);
        particleLayer = ((particleLayer + 1) % 20) + 1;
        rb = GetComponent<Rigidbody2D>();
        if( rb == null )
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
        }
        ps = GetComponent<ParticleSystem>();
        var emit = ps.emission;
        emit.rateOverTime = 0;
        var psNoise = ps.noise;
        noisyNoise = psNoise.strength;
        noiseStartEnabled = psNoise.enabled;
        var psCollision = ps.collision;
        psCollision.collidesWith = (int.MaxValue ^ (1 << LayerMask.NameToLayer("ParticleLayer" + currentLayer)));
        print("my layer: " + LayerMask.NameToLayer("ParticleLayer" + currentLayer));
        Debug.Log("Is noise enabled on start?" + psNoise.enabled);
        if( currentShape == null )
        {
            SetShape( FormType.Ball );
        }

        var psFol = ps.forceOverLifetime;
        initialForce = new Vector3();
        initialForce.x = psFol.x.Evaluate(0);
        initialForce.y = psFol.y.Evaluate(0);
        initialForce.z = psFol.z.Evaluate(0);


        var psVol = ps.velocityOverLifetime;
        psVol.enabled = false;

        var psLvol = ps.limitVelocityOverLifetime;
        psLvol.enabled = false;

        var psMain = ps.main;
        psMain.simulationSpace = ParticleSystemSimulationSpace.World;

        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        mr.sharedMaterial = new Material(Shader.Find("Standard"));
        mr.enabled = false;

        mf.mesh = currentShape.GetMesh();

    }

    public void SetMaxVelocity( float vel )
    {
        if( phase == ElementPhase.Liquid )
        {
            maxVelocity = vel;
        }
    }

    public void SetElement( ElementType newElement )
    {
        myElement = newElement;
    }

    public ElementType GetElement()
    {
        return myElement;
    }

    public void SetPhase( ElementPhase newPhase )
    {
        if( newPhase == ElementPhase.Solid )
        {
            maxVelocity = 0;
            if( phase == ElementPhase.Liquid && psMovementType == MovementType.Stop )
            {
                solidStopParticle = new ParticleSystem.Particle[ps.main.maxParticles];
                solidStopParticleCount = ps.GetParticles(solidStopParticle);
                initialSolidPosition = transform.position;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }
        else if( newPhase == ElementPhase.Liquid )
        {
            solidStopParticle = null;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        phase = newPhase;
    }

    public void SetPath( PathCreation.VertexPath newPath, PathCreation.EndOfPathInstruction eopi )
    {
        myPath = newPath;
        myEndOfPathInstruction = eopi;
    }

    public void Activate()
    {
        Debug.Log("Activating particle magic");
        GetPs();
        ps.Play();
        destroyWhenDead = true;
    }

    public void Deactivate()
    {
        Debug.Log("Deactivating particle magic");
        immortalParticleOn = false;
        ps.Stop();
    }

    void GetPs()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void FixedUpdate()
    {
        if( ps != null && !ps.IsAlive() && destroyWhenDead )
        {
            // Destroy(gameObject, 1);
            Destroy(transform.parent.gameObject,1);
        }
        else if( ps == null )
        {
            Debug.Log("REEEEEEEEEEEEEEEEEEEEEEEEEEEEe");
        }

        // Update emiting state;
        if( ps != null )
        {
            isEmitting = ( ps.particleCount != ps.main.maxParticles || immortalParticleOn == false ) && ps.isEmitting;
        }

        if( psMovementType == MovementType.Path )
        {
            movementJob.SetTarget( myPath.GetPointAtDistance( pathDistance, myEndOfPathInstruction ) );
            print("Vertex point: " + myPath.GetPointAtDistance( pathDistance, myEndOfPathInstruction ) );
            pathDistance += initialVelocity * Time.fixedDeltaTime;
            print("PathDistance: " + pathDistance);
        }

    }

    void Update()
    {
        // Calculate the area of the shape after being scaled
        area = currentShape.totalArea * scale.x * scale.y * sizeMultiplier * sizeMultiplier;

        GetPerimeter();

        // Limit the emission rate based on area
        if( psMovementType != MovementType.Stop )
        {
            limitedEmissionRate = Mathf.Min( emissionRate, emissionAreaConst * area + perimeter * emisisonMinConst );
        }
        else
        {
            limitedEmissionRate = Mathf.Min( emissionRate, emissionAreaConst * area + perimeter * emisisonMinConst );
        }

        // Set the emission rate to the limited emission rate
        var emit = ps.emission;
        emit.rateOverTime = GetEmissionRate();

        // Set the max amount of particles based on area or emission duration
        var psMain = ps.main;
        if( psMovementType == MovementType.Stop )
        {
            psMain.maxParticles = Mathf.Max( (int)Mathf.Ceil(area * 20), ps.particleCount );
        }
        else
        {
            // emit particles for at least 10 seconds
            psMain.maxParticles = (int)Mathf.Max( GetEmissionRate() * psMain.startLifetime.Evaluate(0), psMain.maxParticles );
        }

        // Set the velocity over lifetime
        var psVol = ps.velocityOverLifetime;
        // if( psVol.enabled )
        // {
        //     psVol.x = new ParticleSystem.MinMaxCurve(direction.normalized.x * force);
        //     psVol.y = new ParticleSystem.MinMaxCurve(direction.normalized.y * force);
        //     psVol.z = new ParticleSystem.MinMaxCurve(0);
        // }

        var psLvol = ps.limitVelocityOverLifetime;
        // if( psLvol.enabled )
        // {
        //     psLvol.limitX = new ParticleSystem.MinMaxCurve(direction.normalized.x * force);
        //     psLvol.limitY = new ParticleSystem.MinMaxCurve(direction.normalized.y * force);
        //     psLvol.limitZ = new ParticleSystem.MinMaxCurve(0);
        // }

        var psFol = ps.forceOverLifetime;
        if( psFol.enabled )
        {
            // direction = Quaternion.Euler( rotation.x, rotation.y, rotation.z ) * transform.right;
            psFol.x = new ParticleSystem.MinMaxCurve(direction.normalized.x * force + initialForce.x);
            psFol.y = new ParticleSystem.MinMaxCurve(direction.normalized.y * force + initialForce.y);
            psFol.z = new ParticleSystem.MinMaxCurve(0);
        }

    }

    public float GetArea()
    {
        return area;
    }

    public float GetPerimeter()
    {
        return perimeter = currentShape.CalculateScaledPerimeter( scale );
    }

    public float GetEmissionRate()
    {
        // return (int) ( (scale.x + scale.y) * 25 + (scale.x * scale.y) * 75/9f );
        return limitedEmissionRate;
    }

    void OnDrawGizmos()
    {
        if (currentShape != null)
        {
            // Draws a blue line from this transform to the target
            Gizmos.color = Color.blue;
            foreach( Vector2 edge in currentShape.boundaryEdges )
            {
                Gizmos.DrawLine(
                    Vector3.Scale( currentShape.GetMesh().vertices[(int)edge.x], scale ) + transform.position + position,
                    Vector3.Scale( currentShape.GetMesh().vertices[(int)edge.y], scale ) + transform.position + position
                    );
            }
        }
    }

    public void SetShape( FormType newForm )
    {
        Debug.Log("Setting particle magic shape to " + newForm.ToString());
        if( currentShape == null )
        {
            currentShape = new ShapeArea();
        }
        switch( newForm )
        {
            case FormType.Triangle:
            {
                currentShape.SetMesh( CreateTriangleMesh() );
                break;
            }
            case FormType.Ball:
            {
                currentShape.SetMesh( CreateCircleMesh( 10 ) );
                break;
            }
            case FormType.Rectangle:
            {
                currentShape.SetMesh( CreateRectangleMesh() );
                break;
            }
            case FormType.Custom:
                // shapeType = ParticleSystemShapeType.Sprite;
                break;
            default:
                break;
        }
    }

    public void SetMovement( MovementType movement )
    {
        psMovementType = movement;
        Debug.Log("Setting particle magic shape to " + movement.ToString());
        switch( psMovementType )
        {
            case MovementType.Pour:
            {
                var psVol = ps.velocityOverLifetime;
                psVol.enabled = false;

                var psLvol = ps.limitVelocityOverLifetime;
                psLvol.enabled = false;

                var psFol = ps.forceOverLifetime;
                psFol.enabled = true;

                var psNoise = ps.noise;
                psNoise.enabled = noiseStartEnabled;
                psNoise.strength = noisyNoise;
                Debug.Log("Noise is :" + noiseStartEnabled );

                var psMain = ps.main;
                psMain.simulationSpace = ParticleSystemSimulationSpace.World;

                // Don't kill the particles
                immortalParticleOn = false;
                break;
            }

            case MovementType.Stop:
            {
                // psMovementType = MovementType.Stop;
                initialVelocity = 0;
                maxVelocity = force * 10f + 10f;
                force = 0;

                var psVol = ps.velocityOverLifetime;
                psVol.enabled = false;

                var psLvol = ps.limitVelocityOverLifetime;
                psLvol.enabled = false;

                var psFol = ps.forceOverLifetime;
                psFol.enabled = false;

                var psNoise = ps.noise;
                if( psMovementType == MovementType.Control || psMovementType == MovementType.Path )
                {
                    psNoise.enabled = false;
                }
                else
                {
                    psNoise.enabled = true;
                    psNoise.strength = new ParticleSystem.MinMaxCurve(.75f);
                }

                var psMain = ps.main;
                psMain.simulationSpace = ParticleSystemSimulationSpace.World;

                // Don't kill the particles
                immortalParticleOn = true;
                break;
            }
            default:
            {
                goto case MovementType.Stop;
            }

        }
    }

    public void SetTarget( Vector3 targetPos )
    {
        movementJob.SetTarget( targetPos );
    }

    public void EnableParticleImmortality( bool enable )
    {
        immortalParticleOn = enable;
    }

    void OnParticleUpdateJobScheduled()
    {
        if (ps != null)
        {
            // Keep the particle alive for the duration of the magic
            immortalJob.immortalityOn = immortalParticleOn;
            immortalJob.SetParticleSystem(ps);
            immortalJob.Schedule(ps);

            // Set the shape of the particle emission
            shapeJob.SetShape( currentShape );
            shapeJob.SetOffset( transform.position + position );
            shapeJob.SetRotation( Quaternion.Euler( rotation.x, rotation.y, rotation.z ) );
            shapeJob.SetScale( scale * sizeMultiplier );
            shapeJob.SetParticleSystem(ps);

            // Make the particle keep the shape of the particle system if it was set to stop
            if( psMovementType == MovementType.Stop || psMovementType == MovementType.Push )
            {
                shapeJob.FollowStartPosition( true, force );
                shapeJob.maxDistanceTillTp = 10f;
            }
            // Follow normal particle movement
            else
            {
                shapeJob.FollowStartPosition( false, force );
            }
            shapeJob.ScheduleBatch(ps, 100);

            // If solid don't move
            if( phase == ElementPhase.Solid && psMovementType == MovementType.Stop )
            {
                keepSolidShapeJob.SetParticlePosition( solidStopParticle, solidStopParticleCount );
                keepSolidShapeJob.SetOffset( transform.position + position - initialSolidPosition );
                keepSolidShapeJob.Schedule( ps );
            }

            // Set the particle movement
            movementJob.SetInitialVelocity( initialVelocity );
            movementJob.SetMaxVelocity( maxVelocity );
            movementJob.SetDirection( direction );
            movementJob.SetParticleSystem(ps);

            // Follow the target when being controlled either manually or by path
            movementJob.shouldFollowTarget = psMovementType == MovementType.Control || psMovementType == MovementType.Path;
            if( movementJob.shouldFollowTarget )
            {
                movementJob.force = force;
                movementJob.hasInitialVelocity = false;
            }
            movementJob.Schedule(ps);
        }

    }

    struct KeepSolidShapeJob : IJobParticleSystem
    {
        [DeallocateOnJobCompletionAttribute]
        private NativeArray<float> particlePositionsX;
        [DeallocateOnJobCompletionAttribute]
        private NativeArray<float> particlePositionsY;
        [DeallocateOnJobCompletionAttribute]
        private NativeArray<float> particlePositionsZ;

        private float offsetX;
        private float offsetY;
        private float offsetZ;

        public void Execute(ParticleSystemJobData particles)
        {
            var positionsX = particles.positions.x;
            var positionsY = particles.positions.y;
            var positionsZ = particles.positions.z;
            for( int i = 0; i < particles.count && i < particlePositionsX.Length; i++)
            {
                positionsX[i] = particlePositionsX[i] + offsetX;
                positionsY[i] = particlePositionsY[i] + offsetY;
                positionsZ[i] = particlePositionsZ[i] + offsetZ;
            }
        }

        public void SetParticlePosition( ParticleSystem.Particle[] particles, int particleCount )
        {
            float[] tmpx = new float[particleCount];
            float[] tmpy = new float[particleCount];
            float[] tmpz = new float[particleCount];
            for(int i = 0; i < particleCount; i++ )
            {
                tmpx[i] = particles[i].position.x;
                tmpy[i] = particles[i].position.y;
                tmpz[i] = particles[i].position.z;
            }
            particlePositionsX = new NativeArray<float>( tmpx, Unity.Collections.Allocator.TempJob );
            particlePositionsY = new NativeArray<float>( tmpy, Unity.Collections.Allocator.TempJob );
            particlePositionsZ = new NativeArray<float>( tmpz, Unity.Collections.Allocator.TempJob );
        }

        public void SetOffset( Vector3 offset )
        {
            offsetX = offset.x;
            offsetY = offset.y;
            offsetZ = offset.z;
        }
    }

    struct ImmortalParticleJob : IJobParticleSystem
    {
        public bool immortalityOn;
        float totalLifeTime;

        public void Execute(ParticleSystemJobData particles)
        {
            if( immortalityOn )
            {
                var aliveTimePercent = particles.aliveTimePercent;
                for( int i = 0; i < particles.count; i++)
                {
                    if( aliveTimePercent[i] / 100f * totalLifeTime > ParticleMagic.MINLIFETIME )
                    {
                        aliveTimePercent[i] = ParticleMagic.MINLIFETIME / totalLifeTime * 100f + 1;
                    }
                }
            }
        }

        public void SetParticleSystem( ParticleSystem ps )
        {
            var psMain = ps.main;
            totalLifeTime = psMain.startLifetime.Evaluate(0);
        }
    }

    struct MovementParticleJob : IJobParticleSystem
    {
        public bool hasInitialVelocity;
        public bool hasMaximumVelocity;
        public bool shouldFollowTarget;
        private float initialVelocity;
        private float maxVelocity;
        private float totalLifeTime;

        private float directionX;
        private float directionY;
        private float directionZ;

        private float targetX;
        private float targetY;
        private float targetZ;

        public float force;

        public void Execute(ParticleSystemJobData particles)
        {
            if( hasInitialVelocity || hasMaximumVelocity )
            {
                var velocitiesX = particles.velocities.x;
                var velocitiesY = particles.velocities.y;
                var velocitiesZ = particles.velocities.z;
                for( int i = 0; i < particles.count; i++)
                {
                    if( hasInitialVelocity && particles.aliveTimePercent[i] * totalLifeTime / 100f < ParticleMagic.MINLIFETIME )
                    {
                        velocitiesX[i] = directionX * initialVelocity;
                        velocitiesY[i] = directionY * initialVelocity;
                        velocitiesZ[i] = directionZ * initialVelocity;
                    }
                    else if( shouldFollowTarget )
                    {
                        float randomSeed = particles.randomSeeds[i] / (float) uint.MaxValue + .5f;
                        velocitiesX[i] += ((targetX - particles.positions.x[i]) * randomSeed - velocitiesX[i] * force/1000f );
                        velocitiesY[i] += ((targetY - particles.positions.y[i]) * randomSeed - velocitiesY[i] * force/1000f );
                        velocitiesZ[i] += ((targetZ - particles.positions.z[i]) * randomSeed - velocitiesZ[i] * force/1000f );
                    }
                    Vector3 velocity = new Vector3( velocitiesX[i], velocitiesY[i], velocitiesZ[i] );
                    if( hasMaximumVelocity && velocity.magnitude > maxVelocity )
                    {
                        velocity = velocity.normalized;
                        velocitiesX[i] = velocity.x * maxVelocity;
                        velocitiesY[i] = velocity.y * maxVelocity;
                        velocitiesZ[i] = velocity.z * maxVelocity;
                    }
                }
            }
        }

        public void SetInitialVelocity( float initialVel )
        {
            hasInitialVelocity = true;
            initialVelocity = initialVel;
        }

        public void SetMaxVelocity( float maxVel )
        {
            hasMaximumVelocity = true;
            maxVelocity = maxVel;
        }

        public void SetDirection( Vector3 direction )
        {
            Vector3 normalizedDirection = direction.normalized;
            directionX = normalizedDirection.x;
            directionY = normalizedDirection.y;
            directionZ = normalizedDirection.z;
        }

        public void SetTarget( Vector3 target )
        {
            targetX = target.x;
            targetY = target.y;
            targetZ = target.z;
        }

        public void SetParticleSystem( ParticleSystem ps )
        {
            var psMain = ps.main;
            totalLifeTime = psMain.startLifetime.Evaluate(0);
        }
    }

    struct ShapeParticleJob : IJobParticleSystemParallelForBatch
    {
        [DeallocateOnJobCompletionAttribute, ReadOnly]
        private NativeArray<float> areaArray;
        [DeallocateOnJobCompletionAttribute, ReadOnly]
        private NativeArray<int> triangles;
        [DeallocateOnJobCompletionAttribute, ReadOnly]
        private NativeArray<float> verticesX;
        [DeallocateOnJobCompletionAttribute, ReadOnly]
        private NativeArray<float> verticesY;
        [DeallocateOnJobCompletionAttribute, ReadOnly]
        private NativeArray<float> verticesZ;

        private float offsetX;
        private float offsetY;
        private float offsetZ;

        private float quaterionX;
        private float quaterionY;
        private float quaterionZ;
        private float quaterionW;

        private float scaleX;
        private float scaleY;
        private float scaleZ;

        private float totalArea;

        private float totalLifeTime;

        public bool shouldFollowStartPosition;
        private float followSpeed;
        public float maxDistanceTillTp;

        public void Execute(ParticleSystemJobData particles, int startIndex, int count )
        {
            if( areaArray.IsCreated && areaArray.Length > 0 )
            {
                var positionsX = particles.positions.x;
                var positionsY = particles.positions.y;
                var positionsZ = particles.positions.z;

                // scaleX = scaleX != default( float ) ? scaleX : 1;
                // scaleY = scaleY != default( float ) ? scaleY : 1;
                // scaleZ = scaleZ != default( float ) ? scaleZ : 1;
                for(int i = startIndex; i < startIndex + count; i++ )
                {
                    System.Random rand = new System.Random( (int)particles.randomSeeds[i] );
                    if( particles.aliveTimePercent[i] * totalLifeTime / 100f < ParticleMagic.MINLIFETIME || shouldFollowStartPosition )
                    {
                        float randomSeed = particles.randomSeeds[i] / (float) uint.MaxValue;
                        float targetArea = totalArea * (float) rand.NextDouble();//randomSeed;
                        float currentAreaCount = 0;
                        int triangleIndex = 0;
                        for( ; triangleIndex < areaArray.Length; triangleIndex++ )
                        {
                            currentAreaCount += areaArray[triangleIndex];
                            if( currentAreaCount >= targetArea )
                            {
                                break;
                            }
                        }

                        // Get the triangle vertexes
                        Vector3 vertexA = new Vector3( verticesX[triangles[triangleIndex*3]], verticesY[triangles[triangleIndex*3]], verticesZ[triangles[triangleIndex*3]] );
                        Vector3 vertexB = new Vector3( verticesX[triangles[triangleIndex*3+1]], verticesY[triangles[triangleIndex*3+1]], verticesZ[triangles[triangleIndex*3+1]] );
                        Vector3 vertexC = new Vector3( verticesX[triangles[triangleIndex*3+2]], verticesY[triangles[triangleIndex*3+2]], verticesZ[triangles[triangleIndex*3+2]] );

                        // Scale the triangle
                        Vector3 scale = new Vector3( scaleX, scaleY, scaleZ );
                        vertexA = Vector3.Scale( vertexA, scale);
                        vertexB = Vector3.Scale( vertexB, scale);
                        vertexC = Vector3.Scale( vertexC, scale);

                        // Rotate the triangle
                        Quaternion rotation = new Quaternion( quaterionX, quaterionY, quaterionZ, quaterionW );
                        vertexA = rotation * vertexA;
                        vertexB = rotation * vertexB;
                        vertexC = rotation * vertexC;

                        // Place the particle at a random point on the triangle
                        float randomSeed2 = (float) rand.NextDouble();
                        float startPositionsX = offsetX + (1 - Mathf.Sqrt(randomSeed)) * vertexA.x + (Mathf.Sqrt(randomSeed) * (1 - randomSeed2)) * vertexB.x + (Mathf.Sqrt(randomSeed) * randomSeed2) * vertexC.x;
                        float startPositionsY = offsetY + (1 - Mathf.Sqrt(randomSeed)) * vertexA.y + (Mathf.Sqrt(randomSeed) * (1 - randomSeed2)) * vertexB.y + (Mathf.Sqrt(randomSeed) * randomSeed2) * vertexC.y;
                        float startPositionsZ = offsetZ + (1 - Mathf.Sqrt(randomSeed)) * vertexA.z + (Mathf.Sqrt(randomSeed) * (1 - randomSeed2)) * vertexB.z + (Mathf.Sqrt(randomSeed) * randomSeed2) * vertexC.z;

                        if( particles.aliveTimePercent[i] * totalLifeTime / 100f < ParticleMagic.MINLIFETIME )
                        {
                            positionsX[i] =  startPositionsX;
                            positionsY[i] =  startPositionsY;
                            positionsZ[i] =  startPositionsZ;
                        }

                        // Follow the start position of the shape
                        else if ( shouldFollowStartPosition )
                        {
                            if( maxDistanceTillTp != default(float) && (new Vector3((startPositionsX - positionsX[i]), (startPositionsY - positionsY[i]), (startPositionsZ - positionsZ[i]) )).magnitude > maxDistanceTillTp )
                            {
                                positionsX[i] =  startPositionsX;
                                positionsY[i] =  startPositionsY;
                                positionsZ[i] =  startPositionsZ;
                            }
                            else
                            {
                                var velocitiesX = particles.velocities.x;
                                var velocitiesY = particles.velocities.y;
                                var velocitiesZ = particles.velocities.z;
                                velocitiesX[i] = (startPositionsX - positionsX[i])*followSpeed*10;
                                velocitiesY[i] = (startPositionsY - positionsY[i])*followSpeed*10;
                                velocitiesZ[i] = (startPositionsZ - positionsZ[i])*followSpeed*10;
                            }
                        }
                    }
                }
            }
            // areaArray.Dispose();
        }

        public void FollowStartPosition( bool follow, float fs )
        {
            shouldFollowStartPosition = follow;
            followSpeed = fs;
        }

        public void SetShape( ShapeArea shape )
        {
            totalArea = shape.totalArea;
            areaArray = new NativeArray<float>( shape.triangleAreas, Unity.Collections.Allocator.TempJob );
            triangles = new NativeArray<int>( shape.GetMesh().triangles, Unity.Collections.Allocator.TempJob );
            float[] pointsX = new float[shape.GetMesh().vertices.Length];
            float[] pointsY = new float[shape.GetMesh().vertices.Length];
            float[] pointsZ = new float[shape.GetMesh().vertices.Length];
            for( int i = 0; i < shape.GetMesh().vertices.Length; i++ )
            {
                pointsX[i] = shape.GetMesh().vertices[i].x;
                pointsY[i] = shape.GetMesh().vertices[i].y;
                pointsZ[i] = shape.GetMesh().vertices[i].z;
            }
            verticesX = new NativeArray<float>( pointsX, Unity.Collections.Allocator.TempJob );
            verticesY = new NativeArray<float>( pointsY, Unity.Collections.Allocator.TempJob );
            verticesZ = new NativeArray<float>( pointsZ, Unity.Collections.Allocator.TempJob );
        }

        public void SetOffset( Vector3 offset )
        {
            offsetX = offset.x;
            offsetY = offset.y;
            offsetZ = offset.z;
        }

        public void SetRotation( Quaternion rotation )
        {
            quaterionX = rotation.x;
            quaterionY = rotation.y;
            quaterionZ = rotation.z;
            quaterionW = rotation.w;
        }

        public void SetScale( Vector3 scale )
        {
            scaleX = scale.x;
            scaleY = scale.y;
            scaleZ = scale.z;
        }

        public void SetTransform( Transform transform )
        {
            SetOffset( transform.position );
            SetRotation( transform.rotation );
            SetScale( transform.localScale );
        }

        public void SetParticleSystem( ParticleSystem ps )
        {
            var psMain = ps.main;
            totalLifeTime = psMain.startLifetime.Evaluate(0);
        }
    }
}

public class ShapeArea
{
    Mesh mesh;
    public float[] triangleAreas;
    public float totalArea;

    private float perimeter;
    private Vector3 prevScale;
    // A dictionary of edges, with the key being the index of the vertices,
    // and the value being the number of occurances of the edges
    public Dictionary<Vector2, int> allEdges;
    public List<Vector2> boundaryEdges;

    public Mesh GetMesh()
    {
        return mesh;
    }

    public void SetMesh( Mesh newMesh )
    {
        if( newMesh != mesh )
        {
            mesh = newMesh;
            CalculateLocalMeshArea( mesh );
            UpdateEdgesList( mesh );
        }
    }

    private void CalculateLocalMeshArea( Mesh aMesh )
    {
        triangleAreas = new float[aMesh.triangles.Length/3];
        totalArea = 0;
        for(int i = 0; i < aMesh.triangles.Length; i+=3 )
        {
            Vector3 AB = (aMesh.vertices[aMesh.triangles[i+1]] - aMesh.vertices[aMesh.triangles[i]]);
            Vector3 AC = (aMesh.vertices[aMesh.triangles[i+2]] - aMesh.vertices[aMesh.triangles[i]]);
            triangleAreas[i/3] = Vector3.Cross(AB, AC).magnitude/2;
            totalArea += triangleAreas[i/3];
        }
    }

    public static float[] CalculateMeshArea( Mesh aMesh )
    {
        float[] triangleArea = new float[aMesh.triangles.Length/3];
        for( int i = 0; i < aMesh.triangles.Length; i+=3 )
        {
            Vector3 AB = (aMesh.vertices[aMesh.triangles[i+1]] - aMesh.vertices[aMesh.triangles[i]]);
            Vector3 AC = (aMesh.vertices[aMesh.triangles[i+2]] - aMesh.vertices[aMesh.triangles[i]]);
            triangleArea[i/3] = Vector3.Cross(AB, AC).magnitude/2;
        }
        return triangleArea;
    }

    private void UpdateEdgesList( Mesh aMesh )
    {
        allEdges = new Dictionary<Vector2, int>();
        for( int i = 0; i < aMesh.triangles.Length; i+=3 )
        {
            AddEdgesToList( ref allEdges,new Vector2( aMesh.triangles[i], aMesh.triangles[i+1] ) );
            AddEdgesToList( ref allEdges, new Vector2( aMesh.triangles[i+1], aMesh.triangles[i+2] ) );
            AddEdgesToList( ref allEdges, new Vector2( aMesh.triangles[i+2], aMesh.triangles[i] ) );
        }
        boundaryEdges = new List<Vector2>();
        foreach( Vector2 key in allEdges.Keys )
        {
            if( allEdges[key] == 1 )
            {
                boundaryEdges.Add( key );
            }
        }
    }

    public float CalculateScaledPerimeter( Vector3 scale )
    {
        if( scale == prevScale )
        {
            return perimeter;
        }
        perimeter = 0;
        prevScale = scale;
        foreach( Vector2 key in boundaryEdges )
        {
            if( allEdges[key] == 1 )
            {
                perimeter += Vector3.Distance( Vector3.Scale( mesh.vertices[(int)key.x], scale) , Vector3.Scale( mesh.vertices[(int)key.y], scale) );
            }
        }
        return perimeter;
    }

    public static float CalculateMeshPerimeter( Mesh aMesh, Vector3 scale )
    {
        Dictionary<Vector2, int> edges = new Dictionary<Vector2, int>();
        float trianglePerimeter = 0;
        for( int i = 0; i < aMesh.triangles.Length; i+=3 )
        {
            AddEdgesToList( ref edges,new Vector2( aMesh.triangles[i], aMesh.triangles[i+1] ) );
            AddEdgesToList( ref edges, new Vector2( aMesh.triangles[i+1], aMesh.triangles[i+2] ) );
            AddEdgesToList( ref edges, new Vector2( aMesh.triangles[i+2], aMesh.triangles[i] ) );
        }
        List<Vector2> boundaryEdges = new List<Vector2>();
        foreach( Vector2 key in edges.Keys )
        {
            if( edges[key] == 1 )
            {
                boundaryEdges.Add( key );
                trianglePerimeter += Vector2.Distance( Vector3.Scale( aMesh.vertices[(int)key.x], scale ), Vector3.Scale( aMesh.vertices[(int)key.y], scale ) );
            }
        }
        return trianglePerimeter;
    }

    private static void AddEdgesToList( ref Dictionary<Vector2, int> edgeList, Vector2 edge )
    {
        Vector2 swappedEdge = new Vector2( edge.y, edge.x );
        if( edgeList.ContainsKey( edge ) )
        {
            edgeList[edge] += 1;
        }
        else if( edgeList.ContainsKey( swappedEdge ) )
        {
            edgeList[swappedEdge] += 1;
        }
        else
        {
            edgeList.Add( edge, 1 );
        }
    }
}
