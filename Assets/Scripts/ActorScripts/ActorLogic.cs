using Obstacle_ns;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ActorLogic : MonoBehaviour
{
    public Vector3 mainWaypoint;
    public Vector3 avoidanceWaypoint;

    Obstacle obstacle;

    RaycastHit hit;
    bool waypointReached;
    private Vector3 ActorArea;
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

    public float reachDist = 3.0f;

    public float avoidanceRadius = 3.0f;

    private Animator animator;
    private CharacterController controller;

    enum OperationMode 
    { 
        WAIT,
        INIT,
        EXEC
    }
    private OperationMode operationMode = OperationMode.WAIT;

    private void Start()
    {
        operationMode = OperationMode.WAIT;
    }

    private void OnEnable()
    {
        operationMode = OperationMode.WAIT;
    }

    private void OnDisable()
    {
        operationMode = OperationMode.WAIT;
    }

    void Setup()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        ActorArea = this.transform.parent.gameObject.transform.position;
        waypointReached = true;
    }

    public void Update()
    {
        switch (operationMode)
        {
            case OperationMode.WAIT:
                if (this.transform.parent.tag == "SyntheticHumanArea") { operationMode = OperationMode.INIT; }
                break;
            case OperationMode.INIT:
                Setup();
                operationMode = OperationMode.EXEC;
                break;
            case OperationMode.EXEC:
                Wander();
                break;
            default:
                operationMode = OperationMode.WAIT;
                break;
        }
    }

    public void Wander()
    {
        ApplyGravity();
        animator.SetFloat("Speed", speed);
        obstacle = CheckForObstacleAhead(50);
        if (waypointReached)
        {
            speed = 0f;
            Wait(timeToWait);
            mainWaypoint = GetRandomWaypoint();
            waypointReached = false;
        }
        else
        {
            if (obstacle.ObstacleExists())
            {
                avoidanceWaypoint = CalculateAvoidanceWaypoint(obstacle.ObstaclePosition(), avoidanceRadius, mainWaypoint);
                MoveTo(avoidanceWaypoint);
            }
            else
            {
                MoveTo(mainWaypoint);
            }
        }

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

    public void Wait(float time)
    {
        while (time > 0)
        {
            time -= Time.deltaTime;
        }
    }

    private Vector3 GetRandomWaypoint()
    {
        float distance;
        float xCord, yCord;
        do
        {
            xCord = Random.Range((ActorArea.x - areaSize), (ActorArea.x + areaSize));
            yCord = Random.Range((ActorArea.z - areaSize), (ActorArea.z + areaSize));
            distance = Mathf.Sqrt(Mathf.Pow((xCord - this.transform.position.x), 2) + Mathf.Pow((yCord - this.transform.position.y), 2));
        } while (distance < 5);
        Vector3 waypoint = new Vector3(xCord, 0, yCord);
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