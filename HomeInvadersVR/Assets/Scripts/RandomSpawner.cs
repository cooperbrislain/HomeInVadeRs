using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Destroys this object when it collides with an object.
/// </summary>
public class RandomSpawner : MonoBehaviour
{
	[System.Serializable]
	public class WaveData {
		public GameObject[] spawnPrefabs;
		public float spawnDelay;
        public float spawnIndividualIntervalSecs;
        public int baseSpawnCount;
		public GameObject[] itemPrefabs;
		public Vector3[] itemPositions;
		public int itemCount;
	};
	public WaveData[] waveData;
	public Transform target;		// target that they seek
	public float spawnIntervalSecs;
	public float spawnIntervalSecsChangeRate = 0.95f;
	public List<Vector3> spawnPoints;
	public float spawnCountPerWave = 5;
	public float secondsBetweenWaves = 5;
	public float spawnCountPerWaveChangeRate = 1.2f;
	public int wave;

	public int score;

	public Text text;

	private int _prefabIndex;

	void Awake() {
		StartCoroutine(SpawnUpdate());
	}

	private IEnumerator SpawnUpdate() {
		while (true) {
			int waveDataIndex = Mathf.Min(waveData.Length - 1, wave);
			WaveData currentWaveData = waveData[waveDataIndex];
			float spawnCount = spawnCountPerWave + currentWaveData.baseSpawnCount;

			// spawn items
			for (int i = 0; i < currentWaveData.itemCount; ++i) {
				GameObject itemPrefab = currentWaveData.itemPrefabs[i % currentWaveData.itemPrefabs.Length];
				Vector3 pos = currentWaveData.itemPositions[i % currentWaveData.itemPositions.Length];
				GameObject item = Instantiate(itemPrefab, pos, Quaternion.identity);
				item.transform.SetParent(transform, true);
			}

            yield return new WaitForSeconds(currentWaveData.spawnDelay);

            for (int i = 0; i < spawnCountPerWave; ++i) {
				int slot = Random.Range(0, spawnPoints.Count);
				Vector3 pos = spawnPoints[slot];

				_prefabIndex %= currentWaveData.spawnPrefabs.Length;
				GameObject prefab = currentWaveData.spawnPrefabs[_prefabIndex];
				_prefabIndex++;
				GameObject spawn = Instantiate(prefab, pos, Quaternion.identity);
				spawn.transform.SetParent(transform, true);
				var ai = spawn.GetComponent<AIController>();
				if (ai != null) {
					ai.target = target;
					ai.Destroyed += OnSpawnDestroyed;
				}

				yield return new WaitForSeconds(spawnIntervalSecs);
                yield return new WaitForSeconds(currentWaveData.spawnIndividualIntervalSecs);
            }
            spawnCountPerWave *= spawnCountPerWaveChangeRate;
			spawnIntervalSecs *= spawnIntervalSecsChangeRate;
			_prefabIndex = 0;
			wave++;

			yield return new WaitForSeconds(secondsBetweenWaves);
		}
	}

	private void OnSpawnDestroyed() {
		score++;
		if (text != null) {
			text.text = "Score: " + score;
		}
	}
}
