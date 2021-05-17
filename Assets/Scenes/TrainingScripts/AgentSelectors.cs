using System.Collections;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.Barracuda;
using UnityEngine.AI;
using UnityEngine.XR;

public class AgentSelectors : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private Transform player;
    [SerializeField] private GameObject possibleAgents;
    [SerializeField] private int agentsNumber = 4;
    [SerializeField] private NNModel nnModel;

    [SerializeField] private Transform goal_A;
    [SerializeField] private Transform goal_B;
    //[SerializeField] public Transform player;
    [SerializeField] private float speed = 4f;
    [SerializeField] private int eyes = 16;
    [SerializeField] private float viewDist = 3f;
    [SerializeField] private Transform rig;

    private bool trying = false;
    private GameObject[] workingAgent;
    private int foundAgents;
    private bool desplazado = false;
    private NavMeshPath path;
    private Vector3[] directions;

    Ray shootRay;
    RaycastHit hit;

    void Start()
    {
        //ScriptableObject playerS = player.GetComponent<ScriptableObject>;
        path = new NavMeshPath();
        workingAgent = new GameObject[agentsNumber];
        player.position = rig.position;
        //ChangeToAI(false);
    }

    void updateDirections()
    {
        directions = new Vector3[eyes];
        float angle = 260 / eyes;
        float angleC = angle;
        for (int i = 0; i < eyes; i++)
        {
            directions[i] = Quaternion.Euler(0, angleC + 50, 0) * new Vector3(player.forward.x, 0f, player.forward.y);
            angleC += angle;
        }
    }

    void tryAgain()
    {
        trying = false;
    }

    // Update is called once per frame
    void Update()
    {
        // bool desplazado = playerS.desplazado //Actualizar si el player tiene que ser recolocado
        
        //------------------Meter aqui lo de desplazarse---------------------
        if (Input.GetKeyDown(KeyCode.R) || Vector2.Distance(new Vector2(player.position.x, player.position.z), new Vector2(rig.position.x, rig.position.z)) > 0.8)
        {
            desplazado = !desplazado;
            updateDirections();
            Debug.Log("Desplazado " + desplazado.ToString());
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            player.position = new Vector3(0, 0.3f, 0);
        }
        if (trying && desplazado)
        {
            int count = 0;
            for (int i = 0; i < foundAgents; i+=1)
            {
                if (workingAgent[i].GetComponent<AIMovement>().achieved)
                {
                    count += 1;
                    Debug.Log("Lo consiguió");
                }
            }
            if (count == foundAgents) {
                Debug.Log("Todos lo consiguieron");
                ChangeToAI(false);
                trying = false;
                workingAgent = new GameObject[agentsNumber];
            }
        }
        else if (desplazado && !trying)
        {
            foundAgents = 0;
            Vector3 dir;
            foreach (Transform group in possibleAgents.transform)
            {
                if (group.gameObject.activeSelf)
                {
                    foreach (Transform agents in group)
                    {
                        dir = agents.transform.position - player.transform.position;
                        
                        //----------------------Buscar si tiene la red neuronal como component-------------------------
                        if (agents.gameObject.activeSelf && agents.TryGetComponent(out AIMovement AI) && foundAgents < agentsNumber && Vector3.Angle(dir, player.transform.forward) > 45 && Vector3.Distance(player.transform.position, agents.transform.position) < 8f)
                        {
                            workingAgent[foundAgents] = agents.gameObject;
                            foundAgents += 1;
                        }
                        if (foundAgents == agentsNumber)
                        {
                            trying = true;
                            ChangeToAI(true);
                            return;
                        }
                    }
                }
            }
            /*for (int i = 0; i < eyes && foundAgents < agentsNumber; i++)
            {
                shootRay = new Ray(new Vector3(player.position.x, 0.5f, player.position.z), directions[i]);
                if (Physics.Raycast(ray: shootRay, out hit, maxDistance: 10f, layerMask: 9))
                {
                    workingAgent[foundAgents] = hit.collider.gameObject;
                    Debug.Log(hit.collider.gameObject.name);
                    foundAgents += 1;
                }
                
            }*/

            if (foundAgents != 0)
            {
                ChangeToAI(true);
                trying = true;
            }
            else
            {
                trying = true;
                Invoke("tryAgain",5f);
            }
        }
        else if (trying && !desplazado)
        {
            trying = false;
            Debug.Log("Ya no desplazadito");
            ChangeToAI(false);
            workingAgent = new GameObject[agentsNumber];
        }
    }

    void ChangeToAI(bool ai)
    {
        for (int i = 0; i < foundAgents; i += 1)
        {
            if (ai) Debug.Log(workingAgent[i].name);
            workingAgent[i].GetComponent<NavMeshNavigator>().enabled = !ai;
            workingAgent[i].GetComponent<FSMcontroller>().enabled = !ai;
            workingAgent[i].GetComponent<AIMovement>().enabled = ai;
            workingAgent[i].GetComponent<AIMovement>().achieved = false;
            workingAgent[i].GetComponent<BehaviorParameters>().enabled = ai;
            workingAgent[i].GetComponent<DecisionRequester>().enabled = ai;
        }
    }

    /*void ChangeToAI(bool ai)
    {
        Vector3[] directions = new Vector3[eyes];
        float angle = 360 / eyes;
        float angleC = angle;
        for (int i = 0; i < eyes; i++)
        {
            directions[i] = Quaternion.Euler(0, angleC, 0) * Vector3.forward;
            angleC += angle;
        }

        for (int i = 0; i < foundAgents; i += 1)
        {
            workingAgent[i].GetComponent<NavMeshNavigator>().enabled = !ai;
        }
        for (int i = 0; i < foundAgents; i += 1)
        {
            if (ai)
            {
                if (!workingAgent[i].TryGetComponent(out DecisionRequester decReq))
                {
                    AIMovement aiM = workingAgent[i].AddComponent<AIMovement>();
                    //aiM.startAI(goal_A, goal_B, player, speed, eyes, viewDist);
                    //aiM.setParams(nnModel);                    
                    workingAgent[i].GetComponent<NavMeshNavigator>().enabled = (false);
                }
                else
                {
                    workingAgent[i].GetComponent<DecisionRequester>().enabled = (true);
                    workingAgent[i].GetComponent<BehaviorParameters>().enabled = (true);
                    workingAgent[i].GetComponent<AIMovement>().enabled = (true);
                    workingAgent[i].GetComponent<NavMeshNavigator>().enabled = (false);
                }
            }
            else
            {
                workingAgent[i].GetComponent<DecisionRequester>().enabled = (false);
                workingAgent[i].GetComponent<BehaviorParameters>().enabled = (false);
                workingAgent[i].GetComponent<AIMovement>().enabled = (false);
                workingAgent[i].GetComponent<NavMeshNavigator>().enabled = (true);
            }
            
        }
    }*/

}
