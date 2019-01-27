using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Goal : MonoBehaviour {
    [System.NonSerialized]
    public          float activateDistance = 1.0f;
    public abstract bool  IsValuable();
}
