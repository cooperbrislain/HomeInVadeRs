using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Goal : MonoBehaviour {
    public          float activateDistance = 0.25f;
    public abstract bool  IsValuable();
}
