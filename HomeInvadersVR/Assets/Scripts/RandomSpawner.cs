using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Destroys this object when it collides with an object.
/// </summary>
public class RandomSpawner : MonoBehaviour
{
	public Transform target;		// target that they seek
	public GameObject spawnPrefab;
	public float spawnIntervalSecs;
	public float spawnIntervalSecsChangeRate = 0.95f;
	public List<Vector3> spawnPoints;
	public float spawnCountPerWave = 5;
	public float secondsBetweenWaves = 5;
	public float spawnCountPerWaveChangeRate = 1.2f;
	public int wave;

	public int score;

	void Awake() {
		StartCoroutine(SpawnUpdate());
	}

	private IEnumerator SpawnUpdate() {
		while (true) {
			yield return new WaitForSeconds(secondsBetweenWaves);

			for (int i = 0; i < spawnCountPerWave; ++i) {
				int slot = Random.Range(0, spawnPoints.Count);
				Vector3 pos = spawnPoints[slot];
				GameObject spawn = Instantiate(spawnPrefab, pos, Quaternion.identity);
				var ai = spawn.GetComponent<AIController>();
				ai.target = target;
				ai.Destroyed += OnSpawnDestroyed;
				yield return new WaitForSeconds(spawnIntervalSecs);
			}
			spawnCountPerWave *= spawnCountPerWaveChangeRate;
			spawnIntervalSecs *= spawnIntervalSecsChangeRate;
			wave++;
		}
	}

	private void OnSpawnDestroyed() {
		score++;
	}
}
