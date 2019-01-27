using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAgentDelegator : MonoBehaviour {


    private static EnemyAgentDelegator _instance;
    public  static EnemyAgentDelegator GetInstance() { return _instance; }

    public float EnemyUpdateFrequency = 1.0f; // How often to update the enemy actors in seconds

    List<EnemyAgent>          _activeAgents = new List<EnemyAgent>();
    Valuable[]                _valuables;
    SortedList<int, Valuable> _rankedValuables = new SortedList<int, Valuable>();
    float                     _updateTimer;



    public void AddActiveAgent(EnemyAgent agent) {
        Debug.Log("Adding active agent: " + agent.agentName);
        _activeAgents.Add(agent);
    }

	public void RemoveActiveAgent(EnemyAgent agent) {
		Valuable valuable = agent.goal as Valuable;
		if (valuable != null && !_rankedValuables.ContainsKey(valuable.valueRank)) {
			_rankedValuables.Add(valuable.valueRank, valuable);
		}
		_activeAgents.Remove(agent);
	}

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    void Start() {
        InitValuables();
    }

    public void InitValuables() // Public because we might want to re-init during the game
    {
        _rankedValuables.Clear();
        _valuables = Object.FindObjectsOfType<Valuable>();
        foreach(Valuable valuable in _valuables)
            _rankedValuables.Add(valuable.valueRank, valuable);
    }

	void Update() {
        _updateTimer += Time.deltaTime;
        if (_updateTimer > EnemyUpdateFrequency)
        {
            UpdateAgents();
            _updateTimer = 0;
        }
    }

    void UpdateAgents()
    {
        foreach(EnemyAgent agent in _activeAgents)
        {
            // Skip any agents with active goals
            if (agent.goal != null) continue;

            int activeValuables = _rankedValuables.Count;
            Debug.Log(agent.agentName + " needs job. Active valuables: " + activeValuables);
            if (activeValuables > 0)
            {
                int      valuableIndex = activeValuables - 1;
                int      first         = _rankedValuables.Keys[valuableIndex];
                Valuable valuable      = _rankedValuables[first];

                _rankedValuables.RemoveAt(valuableIndex);

                agent.goal = valuable;
                Debug.Log("Assigned enemy " + agent.agentName + " valuable " + valuable.valuableName + " with rank: " + valuable.valueRank);
            }
        }
    }
}
