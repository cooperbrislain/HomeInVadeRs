using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentReturnGoal : Goal {
    private static AgentReturnGoal _instance;
    public  static AgentReturnGoal GetInstance() { return _instance; }

    public override bool IsValuable() { return false; }

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }
}
