using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR;

public class AIMovement : Agent
{
    // Start is called before the first frame update
    
    [SerializeField] private Transform goal_A;
    [SerializeField] private Transform goal_B;
    [SerializeField] private Transform player;
    [SerializeField] private float speed = 4f;
    [SerializeField] private int eyes = 16;
    [SerializeField] private float viewDist = 3f;
    [SerializeField] private Transform rig;

    private NavMeshAgent agent;
    private NavMeshPath path;
    private Vector3[] directions;
    private bool activated;
    private bool first = false;
    public bool achieved = false;
    //private bool foundA = false;
    //private bool foundB = false;
    RaycastHit hit;
    Ray shootRay;

    public void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
        directions = new Vector3[eyes];
        float angle = 360 / eyes;
        float angleC = angle;
        for (int i = 0; i < eyes; i++)
        {
            directions[i] = Quaternion.Euler(0, angleC, 0) * Vector3.forward;
            angleC += angle;
        }

    }

    /*
    public void startAI(Transform goalA, Transform goalB, Transform playerr, float speedd, int eyess, float viewDistt)
    {
        agent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
        directions = new Vector3[eyes];
        float angle = 360 / eyes;
        float angleC = angle;
        for (int i = 0; i < eyes; i++)
        {
            directions[i] = Quaternion.Euler(0, angleC, 0) * Vector3.forward;
            angleC += angle;
        }

        goal_A = goalA;
        goal_B = goalB;
        player = playerr;
        speed = speedd;
        eyes = eyess;
        viewDist = viewDistt;
        this.MaxStep = 2000;
    }

    public void setParams(NNModel nn)
    {
        BehaviorParameters bp = gameObject.GetComponent<BehaviorParameters>();
        ActionSpec actSpec = new ActionSpec();
        bp.BrainParameters.VectorObservationSize = 40;
        actSpec.NumContinuousActions = 2;
        bp.BrainParameters.ActionSpec = actSpec;
        bp.InferenceDevice = InferenceDevice.GPU;
        bp.Model = nn;
        bp.BehaviorType = BehaviorType.InferenceOnly;
        
        Debug.Log(bp.InferenceDevice.ToString());
    }*/

    private void limitTime()
    {
        activated = false;
        achieved = true;
    }

    public override void CollectObservations(VectorSensor sensor)
    {

        if (!activated && gameObject.activeSelf )
        {
            activated = true;
            Debug.Log(transform.localPosition);
            Invoke("limitTime", 8);
        }
        //Debug.Log("Nani");
        //wallFound = false;
        for (int i = 0; i < eyes; i++)
        {
            shootRay = new Ray(new Vector3(transform.position.x, goal_A.transform.position.y, transform.position.z), directions[i]);
            if (Physics.Raycast(shootRay, out hit, viewDist))
            {
                if (hit.transform.tag == "Player")
                {
                    sensor.AddObservation(1);
                }
                else
                {
                    sensor.AddObservation(0);
                }

                sensor.AddObservation(hit.distance);
            }
            else
            {
                //sensor.AddObservation(false);
                sensor.AddObservation(0);
                sensor.AddObservation(viewDist);
            }
        }

        // Información Agente
        sensor.AddObservation(transform.position.x);
        sensor.AddObservation(transform.position.z);
        sensor.AddObservation(Vector3.Distance(player.position, transform.position));

        // Información objetivo 
        sensor.AddObservation(goal_A.position.x);
        sensor.AddObservation(goal_A.position.z);
        sensor.AddObservation(Vector3.Distance(goal_A.position, transform.position));

        // Información objetivo B
        sensor.AddObservation(goal_B.position.x);
        sensor.AddObservation(goal_B.position.z);
        sensor.AddObservation(Vector3.Distance(goal_B.position, transform.position));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (agent.pathStatus == NavMeshPathStatus.PathComplete && !achieved)
        {
            float moveX = speed * actions.ContinuousActions[0];
            float moveZ = speed * actions.ContinuousActions[1];

            Vector3 dest = new Vector3(transform.position.x + moveX, transform.position.y, transform.position.z + moveZ);
        
            agent.SetDestination(dest);
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (this.gameObject.activeSelf && other.tag == "Goal")
        {
            if (first)
            {
                first = false;
                /*if (other.gameObject == goal_A)
                {
                    foundA = true;
                }
                else
                {
                    foundB = true;
                }*/
            }
            else
            {
                Debug.Log("Ha llegado a ambos");
                achieved = true;
                first = true;
                //foundA = false;
                //foundB = false;
            }
        }
    }
    
}