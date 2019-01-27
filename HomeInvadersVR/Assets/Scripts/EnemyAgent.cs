using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAgent : MonoBehaviour {

    public Valuable  goal;
    public string    agentName;
    NavMeshAgent     agent;

	// Use this for initialization
	void Start () {
        agent = GetComponent<NavMeshAgent>();
        EnemyAgentDelegator.GetInstance().AddActiveAgent(this);
	}

	void OnDestroy() {
		EnemyAgentDelegator.GetInstance().RemoveActiveAgent(this);
	}
	
	// Update is called once per frame
	void Update () {
        if (agent != null && goal != null)
            agent.SetDestination(goal.transform.position);
	}
}
