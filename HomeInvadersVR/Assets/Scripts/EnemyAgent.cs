using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAgent : MonoBehaviour {

    public Goal      goal;
    public string    agentName;
    NavMeshAgent     agent;

    private Valuable carriedItem;

	// Use this for initialization
	void Start () {
        agent = GetComponent<NavMeshAgent>();
        EnemyAgentDelegator.GetInstance().AddActiveAgent(this);
	}
	
	// Update is called once per frame
	void Update () {
        if (goal)
        {
            if (agent != null)
                agent.SetDestination(goal.transform.position);

            // If we are close enough to the object to pick it up (Only on the X/Z plane)
            Vector3 diff = (transform.position - goal.transform.position);
            diff.y = 0;
            float distanceToGoal2D = diff.magnitude;
            if (distanceToGoal2D < goal.activateDistance)
            {
                if (goal.IsValuable())
                {
                    carriedItem = (Valuable)goal;

                    Debug.Log(agentName + " picked up " + carriedItem.valuableName);
                    // Parent the object under the agent (in front of it) and make it kinematic
                    goal.transform.position = transform.position + transform.forward * 0.5f + transform.up;
                    goal.transform.parent = transform;
                    goal.GetComponent<Rigidbody>().isKinematic = true;
                    
                    goal = AgentReturnGoal.GetInstance();
                }
                else
                {
                    Debug.Log(agentName + " is throwing " + carriedItem.valuableName + " into the truck!");
                    goal = null;
                    carriedItem.transform.parent = null;
                    Rigidbody valuableRB = carriedItem.GetComponent<Rigidbody>();
                    valuableRB.isKinematic = false;
                    valuableRB.AddForceAtPosition(transform.forward * 100, carriedItem.transform.position, ForceMode.Force);
                }

            }
        }
	}
}
