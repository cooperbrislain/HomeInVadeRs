using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Goal for targeting a player
/// </summary>
public class PlayerGoal : Goal {
	public override bool IsValuable() { return false; }
}
