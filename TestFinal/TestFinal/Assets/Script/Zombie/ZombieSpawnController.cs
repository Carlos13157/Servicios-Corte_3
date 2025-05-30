using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ZombieSpawnController : NetworkBehaviour {
    [SerializeField] private NetworkPrefabRef zombiePrefab;
    [SerializeField] private int zombieAmount = 5;
    [SerializeField] private float nextWaveTime = 10f;
    [SerializeField] private float nextZombieSpawnTime = 1f;
    [SerializeField] private GameObject generalSpawnPointObject;

    private List<Vector3> spawnPoints = new();
    private float lastTimeWaveCalled = -999f;

    public static int activeZombies = 0;

    public override void Spawned() {
        base.Spawned();

        foreach (Transform child in generalSpawnPointObject.transform) {
            spawnPoints.Add(child.position);
        }

        if (Object.HasStateAuthority) {
            Debug.Log("This instance will handle zombie spawning.");
        }
    }

    public override void FixedUpdateNetwork() {
        // Solo quien tenga autoridad sobre el estado ejecuta el spawn
        if (!Object.HasStateAuthority)
            return;

        if (Time.time - lastTimeWaveCalled >= nextWaveTime && activeZombies <= zombieAmount) {
            StartCoroutine(SpawnWave());
            lastTimeWaveCalled = Time.time;
        }
    }

    private IEnumerator SpawnWave() {
        var closestSpawnPoints = CalculateClosestSpawnPoints();

        for (int i = 0; i < zombieAmount; i++) {
            var randInt = Random.Range(0, closestSpawnPoints.Count);
            Vector3 spawnPos = closestSpawnPoints[randInt];

            // Instanciar zombie como NetworkObject
            Runner.Spawn(zombiePrefab, spawnPos, Quaternion.identity);

            activeZombies++;
            yield return new WaitForSeconds(nextZombieSpawnTime);
        }
    }

    private List<Vector3> CalculateClosestSpawnPoints() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        Vector3 averagePosition = Vector3.zero;
        foreach (var player in players) {
            averagePosition += player.transform.position;
        }
        averagePosition /= players.Length;

        Dictionary<float, Vector3> pointDictionary = new();
        foreach (Vector3 spawnPoint in spawnPoints) {
            float distance = Vector3.Distance(averagePosition, spawnPoint);
            pointDictionary[distance] = spawnPoint;
        }

        List<float> sortedDistances = new(pointDictionary.Keys);
        sortedDistances.Sort();

        List<Vector3> closestPoints = new();
        foreach (var distance in sortedDistances.GetRange(0, Mathf.Min(5, sortedDistances.Count))) {
            closestPoints.Add(pointDictionary[distance]);
        }

        return closestPoints;
    }
}