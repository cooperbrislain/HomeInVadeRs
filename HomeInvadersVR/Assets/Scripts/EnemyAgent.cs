using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using VRTK;

public class EnemyAgent : MonoBehaviour {
	public float distanceToSeekPlayer = 20;  // distance at which we start seeking the player
	public float distanceToHitPlayer = 0.5f;
    public Goal      goal;
    public string    agentName;
	private Goal _previousGoal;
    NavMeshAgent     agent;

    private Valuable carriedItem;
	private EnemyAgentDelegator _delegator;
	private WaitForSeconds _wait = new WaitForSeconds(1);

	// Use this for initialization
	void Start() {
        agent = GetComponent<NavMeshAgent>();
		_delegator = EnemyAgentDelegator.GetInstance();
		if (_delegator != null) {
			_delegator.AddActiveAgent(this);
		}
	}

	void OnDestroy() {
		if (carriedItem != null) {
			carriedItem.transform.parent = null;
		}
		if (_delegator != null) {
			_delegator.RemoveActiveAgent(this);
		}
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
				if (goal is PlayerGoal)
				{
				}
				else if (goal.IsValuable())
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

	public void CheckForPlayer() {
		if (_delegator != null && _delegator.VRHeadset != null) {
			Vector3 headsetPos = _delegator.VRHeadset.transform.position;
			Vector3 pos = transform.position;
			float distance = (headsetPos - pos).magnitude;

			bool didHit = false;

			if (distance < distanceToHitPlayer) {
				// TODO: how to set position in VR?
				_delegator.VRHeadset.position = _delegator.JailPosition;
				if (_previousGoal != null) {
					goal = _previousGoal;
					_previousGoal = null;
				}
				didHit = true;
			} else if (distance < distanceToSeekPlayer) {
				// check for sight
				Ray ray = new Ray(headsetPos, (pos - headsetPos).normalized);
				RaycastHit hitInfo;
				if (!Physics.Raycast(ray, out hitInfo)) {
					Goal headsetGoal = _delegator.VRHeadset.GetComponent<PlayerGoal>();
					if (headsetGoal == null) {
						headsetGoal = _delegator.VRHeadset.gameObject.AddComponent<PlayerGoal>();
					}
					if (_previousGoal == null) {
						_previousGoal = goal;
					}
					goal = headsetGoal;
					didHit = true;
				}
			}
			if (!didHit && _previousGoal != null) {
				goal = _previousGoal;
				_previousGoal = null;
			}
		}
	}
}
