using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR;

public class lesson2Scr : Agent
{
    // Parámetros in-game
    [SerializeField] private Transform goal_A;
    [SerializeField] private Transform goal_B;
    [SerializeField] private Transform transgoalTransform;
    [SerializeField] private float speed = 2f;
    [SerializeField] private int eyes = 16;
    [SerializeField] private float viewDist = 3f;
    [SerializeField] private bool training = true;
    [SerializeField] private Transform agentss;

    // parámetros globales de conteo
    private float distAI = Mathf.Infinity;
    private float distBI = Mathf.Infinity;
    private float offsetX = 0;
    private float offsetZ = 0;
    private Vector3[] directions;
    private float totalReward = 0f;
    private int times = 0;
    private bool searching = false;
    private Vector3 posini;
    private Vector3 goalP;
    private Vector3 userP;
    private bool updated = true;
    private bool closerToA = false;
    private Vector2 lastDir = new Vector2(0, 0);
    RaycastHit hit;
    Ray shootRay;


    // Variables necesarias de movilidad

    private NavMeshAgent agent;
    private NavMeshAgent agentTarg;
    private NavMeshPath path;

    // Valores de puntuaciones

    private bool wallFound = false;
    private bool first = true;
    private float distA = Mathf.Infinity;
    private float distB = Mathf.Infinity;
    private float initDist = 0;
    public void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
        posini = new Vector3(-42.3f, 0f, 0.2f);
        // We initialize all directions the agent needs to look at respect the forward vector
        directions = new Vector3[eyes];
        float angle = 360 / eyes;
        float angleC = angle;
        for (int i = 0; i < eyes; i++)
        {
            directions[i] = Quaternion.Euler(0, angleC, 0) * Vector3.forward;
            angleC += angle;
        }
        agentTarg = transgoalTransform.gameObject.GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (!updated)
        {
            transgoalTransform.localPosition = goalP;
            transform.localPosition = userP;
            updated = true;
            closerToA = Vector3.Distance(transform.position, goal_A.position) < Vector3.Distance(transform.position, goal_B.position);
            initDist = Vector3.Distance(transgoalTransform.position, transform.position) + 1f;
            shootRay = new Ray(new Vector3(goal_A.position.x, 0.5f, goal_B.position.z), goal_B.position-goal_A.position);
            if (!agent.CalculatePath(goal_A.position, path) || !agent.CalculatePath(goal_B.position, path) || Physics.Raycast(shootRay, out hit, Vector3.Distance(goal_A.position, goal_B.position)))
            {
                EndEpisode();
            }
        }
        agent.speed = agentTarg.speed + 1.5f;
    }

    public override void OnEpisodeBegin()
    {
        //closerToA = !closerToA;
        int activated = 0;
        bool all = false;
        foreach (Transform group in agentss.transform)
        {
            if (!all){

                foreach (Transform agents in group)
                {
                    Vector3 dire = agents.transform.position - transgoalTransform.transform.position;

                    //----------------------Buscar si tiene la red neuronal como component-------------------------
                    
                    if (activated == Mathf.Floor(times/30))
                    {
                        all = true;
                    }

                    if (!all)
                    {
                        //Debug.Log(all);
                        agents.gameObject.SetActive(true);
                        activated += 1;
                    }
                }

            }
            
        }

        distAI = Mathf.Infinity;
        distBI = Mathf.Infinity;
        var rand = new Random();
        first = true;
        searching = true;
        foreach (Transform child in transgoalTransform.transform)
        {
            child.gameObject.SetActive(true);
        }

        // Variable initialization
        wallFound = false;

        // Posicion inicial usuario y meta
        userP = new Vector3(0.0f, 0.0f, 0.0f);
        goalP = new Vector3(0.0f, 0.0f, 0.0f);
        transgoalTransform.localPosition = goalP;
        transform.localPosition = userP;

        transgoalTransform.Rotate(new Vector3(0, Random.Range(0,360), 0));

        Vector3 dir = transform.position - transgoalTransform.position;

        // Vector3.Angle(dir, transgoalTransform.forward) < 45
        // Si, tras colocar las paredes no se puede o hay un muro cerca
        while (Vector3.Distance(userP, goalP) < Mathf.Min(3f+offsetX,3f) || (goalP.z > -1 && userP.z < 3) || (goalP.z < -1 && userP.z > 3) || Vector3.Distance(userP, goalP) > Mathf.Min((4f+offsetX),10f) || !agent.CalculatePath(goal_A.position, path) || Vector3.Angle(dir, transgoalTransform.forward) < Mathf.Min((times/50), 110f) || !agent.CalculatePath(goal_B.position, path) )
        {
            //wallFound = false;

            // Recolocamos

            goalP = new Vector3(Random.Range((20), -(20)), 0.0f, Random.Range((20), -(20)));
            userP = new Vector3(Random.Range((20), -(20)), 0.0f, Random.Range((20), -(20))); 
            //userP = new Vector3(Random.Range(goalP.x+Mathf.Min((3f+offsetX),10f)/2, -goalP.x - Mathf.Min((3f + offsetX), 10f) / 2), 0.0f, Random.Range(goalP.x + Mathf.Min((3f + offsetX), 10f) / 2, -goalP.x - Mathf.Min((3f + offsetX), 10f) / 2));
            dir = userP - goalP;
            transgoalTransform.localPosition = goalP;
            transform.localPosition= new Vector3(userP.x, userP.y, userP.z);
            //Debug.Log(Vector3.Angle(dir, transgoalTransform.forward));

        }
        Vector3 dest = new Vector3(Random.Range((20), -(20)), 0.0f, Random.Range((20), -(20))) + posini;
        if (times > 1000)
        {
            while(!agent.CalculatePath(goal_A.position, path))
            {
                dest = new Vector3(Random.Range((20), -(20)), 0.0f, Random.Range((20), -(20))) + posini;
            }
            //agentTarg.SetDestination(dest);
        }

        //Debug.Log("PostWhile");
        //Debug.Log(transform.localPosition);
        //Debug.Log(transgoalTransform.localPosition);

        //this.transform.localPosition = new Vector3(userP.x, userP.y, userP.z);
        searching = false;
        updated = false;

        totalReward = 0f;
        //Debug.Log("Done");

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (!searching || !training)
        {
            for (int i = 0; i < eyes; i++)
            {
                shootRay = new Ray(new Vector3(transform.position.x, 0.5f, transform.position.z), directions[i]);
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

            distAI = Vector3.Distance(goal_A.position, transform.position);
            distBI = Vector3.Distance(goal_B.position, transform.position);

            if (distAI > distBI) closerToA = false;
            else if (distBI > distAI) closerToA = true;

            // Información Agente
            /*sensor.AddObservation(transform.position.x - transgoalTransform.position.x);
            sensor.AddObservation(transform.position.z - transgoalTransform.position.z);
            sensor.AddObservation(Vector3.Distance(transgoalTransform.position, transform.position));*/

            // Información objetivo A
            if (closerToA)
            {
                if (goal_A.gameObject.activeSelf)
                {
                    sensor.AddObservation(goal_A.position.x - transform.position.x);
                    sensor.AddObservation(goal_A.position.z - transform.position.z);
                    sensor.AddObservation(Vector3.Distance(goal_A.position, transform.position));
                }
                else
                {
                    sensor.AddObservation(goal_B.position.x - transform.position.x);
                    sensor.AddObservation(goal_B.position.z - transform.position.z);
                    sensor.AddObservation(Vector3.Distance(goal_B.position, transform.position));
                    closerToA = false;
                }
            }
            else
            {
                if (goal_B.gameObject.activeSelf)
                {
                    sensor.AddObservation(goal_B.position.x - transform.position.x);
                    sensor.AddObservation(goal_B.position.z - transform.position.z);
                    sensor.AddObservation(Vector3.Distance(goal_B.position, transform.position));
                }
                else
                {
                    sensor.AddObservation(goal_A.position.x - transform.position.x);
                    sensor.AddObservation(goal_A.position.z - transform.position.z);
                    sensor.AddObservation(Vector3.Distance(goal_A.position, transform.position));
                    closerToA = true;
                }
            }

            Vector3 dirA = new Vector3(goal_A.position.x, 0.5f, goal_A.position.z) - new Vector3(transform.position.x, 0.5f, transform.position.z);
            Vector3 dirB = new Vector3(goal_B.position.x, 0.5f, goal_B.position.z) - new Vector3(transform.position.x, 0.5f, transform.position.z);

            shootRay = new Ray(new Vector3(transform.position.x, 0.5f, transform.position.z), dirA);
            if (Physics.Raycast(shootRay, out hit, Mathf.Min(viewDist, distA)))
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
                sensor.AddObservation(0);
                sensor.AddObservation(viewDist);
            }
            
            
            shootRay = new Ray(new Vector3(transform.position.x, 0.5f, transform.position.z), dirB);
            if (Physics.Raycast(shootRay, out hit, Mathf.Min(viewDist, distB)))
            {
                sensor.AddObservation(1);
                sensor.AddObservation(hit.distance);
            }
            else
            {
                sensor.AddObservation(0);
                sensor.AddObservation(viewDist);
            }

            //sensor.AddObservation(goal_A.gameObject.activeSelf);
            //sensor.AddObservation((transform.position - goal_A.position).x);
            //sensor.AddObservation((transform.position - goal_A.position).z);

            // Información objetivo B
            //sensor.AddObservation(goal_B.gameObject.activeSelf);
            //sensor.AddObservation((transform.position - goal_B.position).x);
            //sensor.AddObservation((transform.position - goal_B.position).z);

            //sensor.AddObservation(closerToA);
            //sensor.AddObservation(first);
        }

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!searching && training)
        {
            //Vector2 acts = new Vector2(actions.ContinuousActions[0], actions.ContinuousActions[1]).normalized;
            //float moveX = speed * acts[0] * Time.deltaTime;
            //float moveZ = speed * acts[1] * Time.deltaTime;

            float moveX = speed * actions.ContinuousActions[0] * Time.deltaTime;
            float moveZ = speed * actions.ContinuousActions[1] * Time.deltaTime;

            totalReward = 0f;
            
            /*if (Vector3.Angle(lastDir, new Vector2(moveX, moveZ)) > 90)
            {
                totalReward = -0.2f;
            }*/

            lastDir = new Vector2(moveX, moveZ);

            Vector3 destination = transform.position + new Vector3(moveX, 0, moveZ);
            if (agent.CalculatePath(destination, path))
            {
                //move to target
                //Ray shootRay = new Ray(new Vector3(transform.position.x, 0.6f, transform.position.z), (transform.position-transgoalTransform.position));
                totalReward = 0.0f;
                //bool wall = false;
                Vector3 dirA = new Vector3(goal_A.position.x, goal_A.position.y, goal_A.position.z) - new Vector3(transform.position.x, goal_A.position.y, transform.position.z);
                Vector3 dirB = new Vector3(goal_B.position.x, goal_B.position.y, goal_B.position.z) - new Vector3(transform.position.x, goal_B.position.y, transform.position.z);
                totalReward = 0f;
                Ray shootRay;
                float distancia = Vector3.Distance(transform.position, transgoalTransform.position);

                distAI = Vector3.Distance(goal_A.position, transform.position);
                distBI = Vector3.Distance(goal_B.position, transform.position);

                distA = Vector3.Distance(goal_A.position, destination);
                distB = Vector3.Distance(goal_B.position, destination);

                if (distAI > distBI) closerToA = false;
                else if (distBI > distAI) closerToA = true;



                //float hDB = hit.distance;
                /*if (closerToA && goal_A.gameObject.activeSelf)
                {
                    if (wallA)
                    {
                        Debug.DrawRay(new Vector3(transform.position.x, goal_A.transform.position.y, transform.position.z), dirA.normalized * hDA, Color.red);
                        Debug.Log("upsA");
                    }
                    else
                    {
                        Debug.DrawRay(new Vector3(transform.position.x, goal_A.transform.position.y, transform.position.z), dirA.normalized * distA, Color.green);
                    }
                }
                else
                {
                    if (wallB)
                    {
                        Debug.DrawRay(new Vector3(transform.position.x, goal_B.transform.position.y, transform.position.z), dirB.normalized * hDB, Color.red);
                        Debug.Log("upsB");
                    }
                    else
                    {
                        Debug.DrawRay(new Vector3(transform.position.x, goal_B.transform.position.y, transform.position.z), dirB.normalized * distB, Color.green);
                    }
                }*/


                //float max = Mathf.Max(distA, distB);
                if (first)
                {

                    if (distAI > distBI) closerToA = false;
                    else if (distBI > distAI) closerToA = true;

                    /*for (float i = -1; (i <= 1) && (!wall); i += 1f)
                    {
                        if (closerToA)
                        {
                            shootRay = new Ray(new Vector3(transform.position.x, 0.5f, transform.position.z), Quaternion.Euler(0, i * 5, 0) * dirA);
                            if (Physics.Raycast(shootRay, out hit, 1f))
                            {
                                wall = true;
                            }
                        }
                        else
                        {
                            shootRay = new Ray(new Vector3(transform.position.x, 0.5f, transform.position.z), Quaternion.Euler(0, i * 5, 0) * dirB);
                            if (Physics.Raycast(shootRay, out hit, 1f))
                            {
                                wall = true;
                            }
                        }
                    }*/


                    /*
                    if (distA > distAI && distB > distBI && !wall && (agent.CalculatePath(transform.position + dirA.normalized, path) || agent.CalculatePath(transform.position + dirB.normalized, path)))
                    {
                        SetReward(-1f);
                        EndEpisode();
                        //totalReward = -1f;
                    }
                    */
                    shootRay = new Ray(new Vector3(transform.position.x, 0.5f, transform.position.z), dirA);
                    bool wallA = Physics.Raycast(shootRay, out hit, Mathf.Min(viewDist, distA));
                    //float hDA = hit.distance;
                    shootRay = new Ray(new Vector3(transform.position.x, 0.5f, transform.position.z), dirB);
                    bool wallB = Physics.Raycast(shootRay, out hit, Mathf.Min(viewDist, distB));
                    if (closerToA && distA <= distAI)
                    {
                        //totalReward = Mathf.Min(1f, 1/Mathf.Log(distA/2 + 10f, 10));
                        totalReward = Mathf.Min(1f, 1 / Mathf.Log(distA / 2 + 10f));
                        //totalReward = 0.9f;
                        //Debug.Log("A_Aprox" + ' ' + distAI.ToString() + ' ' + distA.ToString() + ' ' + totalReward.ToString());
                    }
                    else if (!closerToA && distB <= distBI)
                    {
                        //totalReward = Mathf.Min(1f, 1 / Mathf.Log(distB/2 + 10f, 10));
                        totalReward = Mathf.Min(1f, 1 / Mathf.Log(distB / 2 + 10f));
                        //totalReward = 0.9f;
                        //Debug.Log("B_Aprox" + ' ' + distBI.ToString() + ' ' + distB.ToString() + ' ' + totalReward.ToString());
                    }
                    else if (closerToA)
                    {
                        /*if (distA > 2)
                        {
                            totalReward = (Mathf.Max(-Mathf.Log(distA - 1, 10), -0.9f));
                        }*/
                        //totalReward = -Mathf.Min(0.9f, 1 / Mathf.Log(distA + 10, 10));
                        //totalReward = (Mathf.Max(-Mathf.Log((distA) + 1, 10), -0.9f));
                        totalReward = (Mathf.Max(-Mathf.Log((distA) + 1), -0.9f));
                        //totalReward = -0.9f;
                        //SetReward(totalreward);
                        //EndEpisode();
                        //Debug.Log("A_Alej" + ' ' + distAI.ToString() + ' ' + distA.ToString() + ' ' + totalReward.ToString());
                    }
                    else 
                    {
                        /*if (distB > 2)
                        {
                            totalReward = (Mathf.Max(-Mathf.Log(distB - 1, 10), -0.9f));
                        }*/
                        //totalReward = -Mathf.Min(0.9f, 1 / Mathf.Log(distB + 10, 10));
                        //totalReward = (Mathf.Max(-Mathf.Log((distB) + 1, 10), -0.9f));
                        totalReward = (Mathf.Max(-Mathf.Log((distB) + 1), -0.9f));
                        //totalReward = -0.9f;
                        //SetReward(totalreward);
                        //EndEpisode();
                        //Debug.Log("B_Alej" + ' ' + distBI.ToString() + ' ' + distB.ToString() + ' ' + totalReward.ToString());
                    }
                    
                    //else totalReward = 0.2f;*/
                }
                else
                {

                    //float hDA = hit.distance;


                    if (goal_A.gameObject.activeSelf)
                    {
                        shootRay = new Ray(new Vector3(transform.position.x, 0.5f, transform.position.z), dirA);
                        bool wallA = Physics.Raycast(shootRay, out hit, Mathf.Min(viewDist, distA));
                        closerToA = true;
                        //shootRay = new Ray(new Vector3(transform.position.x, goal_A.position.y, transform.position.z), dirA);
                        //wallA = Physics.Raycast(shootRay, out hit, Mathf.Min(viewDist, distA));
                        /*if (!wallA && distA > distAI)
                        {
                            SetReward(-1f);
                            //EndEpisode();
                            //totalReward = -1f;
                        }
                        else*/ 
                        if(distA <= distAI)
                        {
                            //totalReward = Mathf.Min(1f, 1 / Mathf.Log(distA/2 + 10f, 10));
                            totalReward = Mathf.Min(1f, 1 / Mathf.Log(distA / 2 + 10f));
                            //totalReward = 0.9f;
                            //Debug.Log("A_Aprox_2" + ' ' + distAI.ToString() + ' ' + distA.ToString() + ' ' + totalReward.ToString());
                        }
                        else 
                        {
                            /*if (distA > 2)
                            {
                                totalReward = (Mathf.Max(-Mathf.Log(distA - 1, 10), -0.9f));
                            }*/
                            //totalReward = -0.9f;
                            //totalReward = -Mathf.Min(0.9f, 1 / Mathf.Log(distA + 10, 10));
                            //totalReward = (Mathf.Max(-Mathf.Log((distA) + 1, 10), -0.9f));
                            totalReward = (Mathf.Max(-Mathf.Log((distA) + 1), -0.9f));
                            //SetReward(totalreward);
                            //EndEpisode();
                            //Debug.Log("A_Alej_2" + ' ' + distAI.ToString() + ' ' + distA.ToString() + ' ' + totalReward.ToString());
                        }
                        //if (totalReward > 0) totalReward *=1.1f;*/
                    }
                    else
                    {
                        shootRay = new Ray(new Vector3(transform.position.x, 0.5f, transform.position.z), dirB);
                        bool wallB = Physics.Raycast(shootRay, out hit, Mathf.Min(viewDist, distB));
                        closerToA = false;
                        //shootRay = new Ray(new Vector3(transform.position.x, goal_B.position.y, transform.position.z), dirB);
                        //wallB = !Physics.Raycast(shootRay, out hit, Mathf.Min(viewDist, distB));
                        /*if (!wallB && distB > distBI)
                        {
                            SetReward(-1f);
                            //EndEpisode();
                            //totalReward = -1f;
                        }
                        else*/ if (distB <= distBI)
                        {
                            //totalReward = Mathf.Min(1f, 1 / Mathf.Log(distB/2 + 10f, 10));
                            totalReward = Mathf.Min(1f, 1 / Mathf.Log(distB / 2 + 10f));
                            //totalReward = 0.9f;
                            //Debug.Log("B_Aprox_2" + ' ' + distBI.ToString() + ' ' + distB.ToString() + ' ' + totalReward.ToString());
                        }
                        else 
                        {
                            /*if (distB > 2)
                            {
                                totalReward = (Mathf.Max(-Mathf.Log(distB - 1, 10), -0.9f));
                            }*/

                            //totalReward = -Mathf.Min(0.9f, 1 / Mathf.Log(distB + 10, 10));
                            //totalReward = (Mathf.Max(-Mathf.Log((distB) + 1, 10), -0.9f));
                            totalReward = (Mathf.Max(-Mathf.Log((distB) + 1), -0.9f));
                            //totalReward = -0.9f;
                            //SetReward(totalreward);
                            //EndEpisode();
                            //Debug.Log("B_Alej_2" + ' ' + distBI.ToString() + ' ' + distB.ToString() + ' ' + totalReward.ToString());
                        }
                        //if (totalReward > 0) totalReward *= 1.1f;*/
                    }
                }

                //float DPlayer = Vector3.Distance(transform.position, transgoalTransform.position);
                if (totalReward > 0) totalReward = Mathf.Pow(totalReward, 2);
                else totalReward = -Mathf.Pow(totalReward, 2);
                /*if (totalReward > 0)*/ //totalReward = (totalReward)/10;
                /*else if (closerToA)
                {
                    if (wallA)
                    {
                        totalReward /= 5;
                    }
                    else
                    {
                        totalReward /= 2;
                    }
                }
                else
                {
                    if (wallB)
                    {
                        totalReward /= 5;
                    }
                    else
                    {
                        totalReward /= 2;
                    }
                }*/

                //Debug.Log(totalReward);
                SetReward(totalReward);
                transform.position = destination;
                if (distancia > initDist) EndEpisode();
                //Debug.Log(distA.ToString()  + ' ' + distAI.ToString() + "----" + distB.ToString() + ' ' + distBI.ToString() + " = " + totalReward.ToString());
            }
            else
            {
                SetReward(-1f);
                //transform.position = transform.position - new Vector3(moveX, 0, moveZ)*0.5f;
            }
            
        }
        else if (training)
        {
            transgoalTransform.localPosition = new Vector3(0f, 0f, 0f);
            transform.localPosition = new Vector3(0f,0f,0f);
            SetReward(0.0f);
        }
        else
        {
            if (agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                float moveX = speed * actions.ContinuousActions[0];
                float moveZ = speed * actions.ContinuousActions[1];

                Vector3 dest = new Vector3(transform.position.x + moveX, transform.position.y, transform.position.z + moveZ);

                agent.SetDestination(dest);
            }
        }

    }

    private void OntriggerStay(Collider other)
    {
        if (!searching)
        {
            if (other.tag == "Player")
            {
                SetReward(-1f);
                //EndEpisode();
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!searching)
        {
            if (other.gameObject.layer == 9)
            {
                SetReward(-1f);
                EndEpisode();
            }
            if (other.tag == "Untagged")
            {

                SetReward(-0.6f);
                //Debug.Log("Untagged");
                EndEpisode();

            }
            else if (other.tag == "Player")
            {
                SetReward(-1f);
                EndEpisode();
            }
            else if (other.tag == "Goal")
            {
                if (first)
                {
                    Debug.Log("First!");                    
                    first = false;
                    SetReward(1f);
                    other.gameObject.SetActive(false);
;               }
                else
                {
                    SetReward(1f);
                    if (this.MaxStep < 4500)
                    {
                    this.MaxStep += (int)Mathf.Floor(times / 100);
                    }
                    Debug.Log("Ole!!");
                    times += 1;

                    if (offsetX < 8)
                    {
                        if (times < 1000)
                        {
                            offsetX += 0.001f;
                        }
                        else
                        {
                            offsetX += 0.01f;
                        }

                    }

                    if (offsetZ < 8)
                    {
                        if (times < 1000)
                        {
                            offsetZ += 0.001f;
                        }
                        else
                        {
                            offsetZ += 0.01f;
                        }
                    }
                    EndEpisode();
                }

            }
        }

    }

}
