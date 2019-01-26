using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Destroys this object when it collides with an object.
/// </summary>
public class RandomSpawner : MonoBehaviour
{
	public GameObject spawnPrefab;
	public float spawnIntervalSecs;
	public Transform target;		// target that they seek
	public List<Vector3> spawnPoints;
	public int spawnCountPerWave = 5;

	public int wave;

	private float _lastSpawnTime;

	void Update() {
		if (_lastSpawnTime + spawnIntervalSecs < Time.time) {
			int slot = Random.Range(0, spawnPoints.Count);
			Vector3 pos = spawnPoints[slot];
			GameObject spawn = Instantiate(spawnPrefab, pos, Quaternion.identity);
			var ai = spawn.GetComponent<AIController>();
			ai.target = target;
			_lastSpawnTime = Time.time;
		}
	}
}
