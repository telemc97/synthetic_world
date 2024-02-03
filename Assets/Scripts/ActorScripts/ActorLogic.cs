using Obstacle_ns;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using RosMessageTypes.Geometry;
using System.Data;

public class ActorLogic : MonoBehaviour
{
    public Vector3 mainWaypoint;

    public Vector3 avoidanceWaypoint;
    int timesObstructed = 0;
    public int maxTimesObstructed = 1;
    Obstacle obstacle;

    RaycastHit hit;
    bool waypointReached;
    private Vector3 actorArea;
    public float areaSize = 10.0f;

    //Walking characteristics
    public float speed;
    public float walkingspeed = 2.0f;

    public float accelaration = 6.0f;       //Set in Inspector 
    public float decelaration = 5.0f;       //Set in Inspector 
    public float turnSmoothTime = 1.0f;     //Set in Inspector 
    public float rotationSpeed = 2.0f;
    float turnSmoothVelocity;
    Vector3 GravityVector;

    public float timeToWait = 10.0f;
    float timeLeft;

    public float reachDist = 3.0f;

    public float avoidanceRadius = 3.0f;

    private Animator animator;
    private CharacterController controller;

    enum OperationMode 
    {
        SETUP,
        INIT,
        EXEC,
        WAIT
    }
    private OperationMode operationMode = OperationMode.SETUP;

    private void Start()
    {
        operationMode = OperationMode.SETUP;
    }

    private void OnEnable()
    {
        operationMode = OperationMode.SETUP;
    }

    private void OnDisable()
    {
        operationMode = OperationMode.SETUP;
    }

    void Setup()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        actorArea = this.transform.parent.gameObject.transform.position;
        mainWaypoint = actorArea;
        timeLeft = timeToWait;
        waypointReached = true;
    }

    public void Update()
    {
        switch (operationMode)
        {
            case OperationMode.SETUP:
                if (this.transform.parent.tag == "SyntheticHumanArea") { operationMode = OperationMode.INIT; }
                break;
            case OperationMode.INIT:
                Setup();
                mainWaypoint = GetRandomWaypoint();
                operationMode = OperationMode.EXEC;
                break;
            case OperationMode.EXEC:
                Wander();
                break;
            case OperationMode.WAIT:
                if (!Waiting())
                {
                    operationMode = OperationMode.EXEC;
                }
                break;
            default:
                operationMode = OperationMode.SETUP;
                break;
        }
    }

    public void Wander()
    {
        ApplyGravity();
        SetSpeed(speed);
        obstacle = CheckForObstacleAhead(50);
        if (waypointReached)
        {
            operationMode = OperationMode.WAIT;
            mainWaypoint = GetRandomWaypoint();
            waypointReached = false;
        }
        else
        {
            if (obstacle.ObstacleExists())
            {
                timesObstructed++;
                if (timesObstructed > maxTimesObstructed)
                {
                    operationMode = OperationMode.WAIT;
                    timesObstructed = 0;
                }
                avoidanceWaypoint = CalculateAvoidanceWaypoint(obstacle.ObstaclePosition(), avoidanceRadius, mainWaypoint);
                MoveTo(avoidanceWaypoint);
            }
            else
            {
                MoveTo(mainWaypoint);
            }
        }

    }

    void SetSpeed(float spd)
    {
        speed = spd;
        animator.SetFloat("Speed", spd);
    }

    private Obstacle CheckForObstacleAhead(int dist)
    {
        Obstacle obs = new Obstacle();
        if (Physics.Raycast(this.transform.position, this.transform.TransformDirection(Vector3.forward), out hit, dist))
        {
            if (hit.collider != null)
            {
                obs.SetObstacleData(this.transform.position, hit.transform.position, hit.transform.gameObject.tag);
            }
        }
        return obs;
    }

    public bool Waiting()
    {
        SetSpeed(0f);
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = timeToWait;
            return false;
        }
        return true;
    }

    private Vector3 GetRandomWaypoint()
    {
        RaycastHit raycastHit = new RaycastHit();
        float distance;
        float xCord, yCord;
        Vector3 waypoint = new Vector3();
        string hitTag = null;
        do
        {
            Vector2 newAreaMinCoords = new Vector2();
            Vector2 newAreaMaxCoords = new Vector2();
            //implement finding new waypoint in a subrectangle formed by the intersect of 2 rectangles (center actor area and center this actor)
            Vector2 centerA = new Vector2(this.transform.position.x, this.transform.position.z);
            Vector2 centerB = new Vector2(actorArea.x, actorArea.z);
            //Find xmin and xmax intersection coords
            if (centerA.x - areaSize > centerB.x - areaSize)
            {
                newAreaMinCoords.x = centerA.x - areaSize;
                newAreaMaxCoords.x = centerB.x + areaSize;
            }
            else if (centerB.x - areaSize > centerA.x - areaSize)
            {
                newAreaMinCoords.x = centerB.x - areaSize;
                newAreaMaxCoords.x = centerA.x + areaSize;
            }
            else //centerA = centerB actor position is on the same coordinate as
            {
                newAreaMinCoords.x = centerB.x - areaSize;
                newAreaMaxCoords.x = centerB.x + areaSize;
            }
            //Find ymin and ymax intersection coords
            if (centerA.y - areaSize > centerB.y - areaSize)
            {
                newAreaMinCoords.y = centerA.y - areaSize;
                newAreaMaxCoords.y = centerB.y + areaSize;
            }else if (centerB.y - areaSize > centerA.y - areaSize)
            {
                newAreaMinCoords.y = centerB.y - areaSize;
                newAreaMaxCoords.y = centerA.y + areaSize;
            }
            else //centerA = centerB actor position is on the same coordinate as
            {
                newAreaMinCoords.y = centerB.y - areaSize;
                newAreaMaxCoords.y = centerA.y + areaSize;
            }
            xCord = Random.Range(newAreaMinCoords.x, newAreaMaxCoords.x);
            yCord = Random.Range(newAreaMinCoords.y, newAreaMaxCoords.y);
            distance = Mathf.Sqrt(Mathf.Pow((xCord - this.transform.position.x), 2) + Mathf.Pow((yCord - this.transform.position.y), 2));
            waypoint.x = xCord;
            waypoint.y = 0;
            waypoint.z = yCord;
            if (Physics.Raycast(waypoint, Vector3.down, out raycastHit))
            {
                hitTag = raycastHit.transform.tag;
            }

        } while (distance < 5 && hitTag!="Water" && hitTag != null);
        
        return waypoint;
    }

    private void MoveTo(Vector3 waypoint)
    {
        Vector3 lookPos = waypoint - transform.position;
        lookPos.y = 0;
        Quaternion rotation = new Quaternion();
        if (lookPos != Vector3.zero)
        {
            rotation = Quaternion.LookRotation(lookPos);
        }
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime);
        float targetAngle = transform.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
        Vector3 moveDir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
        float distance = CalculateDistance(transform.position, waypoint);
        if (distance >= reachDist)
        {
            if (speed < walkingspeed) { speed += accelaration * Time.deltaTime; }
            else { speed = walkingspeed; }
            waypointReached = false;
        }
        else
        {
            if (speed > 0) { speed -= decelaration * Time.deltaTime; }
            else { speed = 0; }
            waypointReached = true;
        }

        controller.Move(moveDir.normalized * speed * Time.deltaTime);
    }

    private float CalculateDistance(Vector3 vec0, Vector3 vec1)
    {
        float distance = 0f;
        distance = Mathf.Sqrt(Mathf.Pow((vec1.x - vec0.x), 2) + Mathf.Pow((vec1.z - vec0.z), 2));
        return distance;
    }

    private void ApplyGravity()
    {
        GravityVector = Vector3.zero;
        GravityVector += Physics.gravity;
        controller.Move(GravityVector * Time.deltaTime);
    }

    private Vector3 CalculateAvoidanceWaypoint(Vector3 obstaclePos, float obstacleRadius, Vector3 originalWaypoint)
    {
        //TODO check var names
        //Line between this actor pos and waypoint
        float a = originalWaypoint.y - this.transform.position.y;
        float b = originalWaypoint.x - this.transform.position.x;
        float c = this.transform.position.y * (originalWaypoint.x - this.transform.position.x) - (originalWaypoint.y - this.transform.position.y) * this.transform.position.x;

        //Distance between the actor_pos - waypoint line and obstacle
        float dist = Mathf.Abs(a * obstaclePos.x + b * obstaclePos.y + c) / Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));

        //Closest point of the line actor_pos to obstacle
        Vector2 closestPoint = new Vector2();
        closestPoint.x = (b * (b * obstaclePos.x - a * obstaclePos.y) - a * c) / (Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
        closestPoint.y = (a * (-b * obstaclePos.x + a * obstaclePos.y) - b * c) / (Mathf.Pow(a, 2) + Mathf.Pow(b, 2));

        //Calculate avoidance distance
        float avoidDist = dist - obstacleRadius;

        //Tan of the actor_pos - waypoint line
        float tana = avoidDist / (Mathf.Sqrt(Mathf.Pow((closestPoint.x - this.transform.position.x), 2) + Mathf.Pow((closestPoint.y - this.transform.position.y), 2)));

        //Tan of the actor_pos - new_waypoint line
        float tanb = (((closestPoint.y - obstaclePos.y) / (closestPoint.x - obstaclePos.y)) - tana) / (tana * ((closestPoint.y - obstaclePos.y) / (closestPoint.x - obstaclePos.y))) - 1;

        //x of the actor_pos - new_waypoint line
        float x = (tanb * this.transform.position.x - this.transform.position.y - obstaclePos.x * ((closestPoint.y - obstaclePos.y) / (closestPoint.x - obstaclePos.x)) + obstaclePos.y) 
                / (tanb - ((closestPoint.y - obstaclePos.y) / (closestPoint.x - obstaclePos.x)));
        float y = tanb * x - tanb * this.transform.position.x + this.transform.position.y;
        //New Temporary Vector
        Vector3 newWaypoint = new Vector3(x, 0, y);

        return newWaypoint;    
    }
}