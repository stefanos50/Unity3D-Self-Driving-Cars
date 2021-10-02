using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(NNet))]
public class CarController : MonoBehaviour
{
    private float currentSteerAngle;
    private float currentbreakForce =0f;
    private bool isBreaking = false;
    public bool showSensors = true;

    [SerializeField] private float motorForce;
    [SerializeField] private float breakForce;
    [SerializeField] private float maxSteerAngle;

    private Vector3 startPosition, startRotation;
    public NNet network;
    public GeneticManager genetic_manager;

    [Range(-1f, 1f)]
    public float acceleration, turning;
    public float timeSinceStart = 0f;

    //Multipliers for the overall fitness calculation
    [Header("Fitness")]
    public float overallFitness;
    public float distanceMultiplier = 1.4f;
    public float avgSpeedMultiplier = 0.2f;
    public float sensorMultiplier = 0.1f;

    //Neural Network options (number of the hidden layers and neurons)
    [Header("Network Options")]
    public int LAYERS = 1;
    public int NEURONS = 10;

    private Vector3 lastPosition;
    private float totalDistanceTravelled;
    private float avgSpeed;

    //The three car sensors
    private float aSensor, bSensor, cSensor;

    //Variable that keeps track if the trained brain is saved
    public bool saved = false;
    public float time_to_train; 

    //Wheel collider references for each of the 4 wheels of the car
    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;

    //Wheel gameobject transform referneces for each of the 4 wheels of the car
    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheeTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;


    //Create the lines of the sensors of the car visualy with a red color 
    //and destroy them after a realy small amount of time for performance purposes
    private GameObject MakeLine(Vector3[] vertices)
    {
        GameObject o = new GameObject("SensorLineGameObject");
        LineRenderer line = o.AddComponent<LineRenderer>().GetComponent<LineRenderer>();
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        Material mat = new Material(shader) { color = Color.red };
        line.material = mat;
        line.startWidth = 0.3f;
        line.endWidth = 0.3f;
        line.positionCount = vertices.Length;
        line.SetPositions(vertices);
        Destroy(o, 0.01f);
        return o;
    }

    //Init the variables
    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
        network = GetComponent<NNet>();
        genetic_manager = GetComponent<GeneticManager>();
        //TEST
        network.init(LAYERS, NEURONS);
    }

    //Reset the car and neural network
    public void ResetWithNetwork(NNet net)
    {
        network = net;
        Reset();
    }

    //Reset the car by resetting the variable values to default and moving the car to
    //its start position and rotation
    public void Reset()
    {
        network.init(LAYERS, NEURONS);

        timeSinceStart = 0f;
        totalDistanceTravelled = 0f;
        avgSpeed = 0f;
        lastPosition = startPosition;
        overallFitness = 0f;
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
    }

    //If the car enter a wall trigger then it means it went out of the road 
    //so it should die (Reset).
    private void OnTriggerEnter(Collider col)
    {
        Death();
    }

    private void FixedUpdate()
    {
        //Update the wheels movement
        HandleMotor();
        HandleSteering();
        UpdateWheels();

        //Update the sensors
        InputSensors();

        //Keep track of the last position
        lastPosition = transform.position;

        //Set acceleration and turning values based of the neural netowork result output values
        (acceleration, turning) = network.RunNetwork(aSensor, bSensor, cSensor);

        //Move the car based of the acceleration , turning value that the neural netowrk predicted
        MoveCar(acceleration, turning);
        //Increment the time that the car is moving without getting out of the road
        timeSinceStart += Time.deltaTime;

        //Calculate the Overall fitness
        CalculateFitness();

        //Keep track of the real time until the car is trained
        if(saved == false)
        {
            time_to_train = Time.realtimeSinceStartup;
        }
    }

    //Death function is called when the car collide with a wall (get out of the road)
    //and inform the genetic algorithm 
    public void Death()
    {
        genetic_manager.Death(overallFitness, network);
    }

    //Calculation of the overall fitness
    private void CalculateFitness()
    {
        //Total distance is incrementing every time based of the distance between the current position and the last position
        totalDistanceTravelled += Vector3.Distance(transform.position, lastPosition);
        //Average speed is the total distance that the car travelled divided by the time since the car started moving (without crashing a wall)
        avgSpeed = totalDistanceTravelled / timeSinceStart;

        //Overall Fitness based of the multiplier values
        overallFitness = (totalDistanceTravelled * distanceMultiplier) + (avgSpeed * avgSpeedMultiplier) + (((aSensor+bSensor+cSensor)/3)*sensorMultiplier);

        //If the overall fitness is really low for a specific time then there is no reason to keep trying 
        //and its better to start from the start
        if (timeSinceStart > 20 && overallFitness < 40)
        {
            Death();
        }

        //If the overall fitness has a high value then save the brain (save the neural network)
        if(overallFitness >= 1500)
        {
            if(saved == false)
            {
                saved = true;
                network.SaveNetwork();
            }
        }
    }

    private void InputSensors()
    {
        //The three sensors of the car
        // a is the right sensor of the car
        //b is the front sensor of the car
        //c is the left sensor of the car
        Vector3 a = (transform.forward + transform.right);
        Vector3 b = (transform.forward);
        Vector3 c = (transform.forward - transform.right);

        Ray raycast = new Ray(transform.position, a);
        RaycastHit hit;
        if(Physics.Raycast(raycast,out hit))
        {
            aSensor = hit.distance / 20;
            //Debug.DrawLine(raycast.origin, hit.point, Color.red);

            //If the timescale has a big value then dont draw the sensor lines for performance purposes
            //If the showSensor variable is false then dont draw the sensor lines
            if (Time.timeScale <= 7.0f && showSensors == true)
            {
                Vector3[] LinePositions = new[] { raycast.origin, hit.point };
                MakeLine(LinePositions);
            }
        }

        raycast.direction = b;
        if (Physics.Raycast(raycast, out hit))
        {
            bSensor = hit.distance / 20;
            //Debug.DrawLine(raycast.origin, hit.point, Color.red);

            //If the timescale has a big value then dont draw the sensor lines for performance purposes
            //If the showSensor variable is false then dont draw the sensor lines
            if (Time.timeScale <= 7.0f && showSensors == true)
            {
                Vector3[] LinePositions = new[] { raycast.origin, hit.point };
                MakeLine(LinePositions);
            }
        }

        raycast.direction = c;
        if (Physics.Raycast(raycast, out hit))
        {
            cSensor = hit.distance / 20;
            //Debug.DrawLine(raycast.origin, hit.point, Color.red);

            //If the timescale has a big value then dont draw the sensor lines for performance purposes
            //If the showSensor variable is false then dont draw the sensor lines
            if (Time.timeScale <= 7.0f && showSensors == true)
            {
                Vector3[] LinePositions = new[] { raycast.origin, hit.point };
                MakeLine(LinePositions);
            }
        }
    }

    private Vector3 input;
    //Update the car location and rotation based of the acceleration and turning values (vertical , horizontal)
    public void MoveCar(float vertical , float horizontal)
    {
        input = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, vertical * 11.4f), 0.02f);
        input = transform.TransformDirection(input);
        transform.position += input;

        transform.eulerAngles += new Vector3(0,(horizontal * 90)*0.02f,0);
    }

    //Update the motorTorque of the 4 wheel colliders of the car based on the current acceleration and the 
    //preset motorForce
    private void HandleMotor()
    {
        frontLeftWheelCollider.motorTorque = acceleration * motorForce;
        frontRightWheelCollider.motorTorque = acceleration * motorForce;
        rearLeftWheelCollider.motorTorque = acceleration * motorForce;
        rearRightWheelCollider.motorTorque = acceleration * motorForce;
    }

    //Update the steerAngle of the 2 front wheels of the car based on the current turning value and the
    //preset maxSteerAngle value
    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * turning;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    //Update all of the 4 wheels of the car by calling the single wheel function for each wheel
    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheeTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    //Given a single wheel collider and transform update its rotation and position
    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;       
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
        wheelTransform.position += new Vector3(0, 0.1f, 0);
    }

}
