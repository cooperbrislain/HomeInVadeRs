using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Valuable : Goal {
    public          int    valueRank;
    public          string valuableName;
    public override bool   IsValuable()   { return true; }
}
