using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeSequenceLogic : MonoBehaviour
{
    public int herdCount = 3;
    [Space]
    public GameObject policePrefab;
    public float spawnDelay = 1;
    public float spawnDistance;
    public float spawnAngleMin;
    public float spawnAngleMax;
    [Space]
    public float sequenceDurration = 15;
    public Vector3 bridgeRotation;
    [Space]
    public Transform bridge1;
    public Transform bridge2;
    public GameObject bridgeCollider;
    public Transform herdTarget;

    private Coroutine policeSpawner;
    private bool canBeTriggered = false;



	void OnTriggerEnter(Collider other)
	{
		if (canBeTriggered && other.CompareTag("Player"))
		{
            // Make the herd stay near the bridge instead of following the player
            FlockingComponent.SetFlockTarget(herdTarget);

            policeSpawner = StartCoroutine(SpawnPolice());
            StartCoroutine(MoveBridge());

            canBeTriggered = false;

            // "Get to the bridge"
            ObjectiveManager.instance.CountedTaskComplete(3);
        }
	}

    private IEnumerator SpawnPolice()
	{
        while (true)
		{
            // Get random direction within angle, multiplied by distance
            Vector3 spawnPoint = Vector3.zero;
            float angle = Random.Range(spawnAngleMin, spawnAngleMax) * Mathf.Deg2Rad;
            spawnPoint.x = Vector3.forward.x * Mathf.Cos(angle) - Vector3.forward.z * Mathf.Sin(angle);
            spawnPoint.z = Vector3.forward.x * Mathf.Sin(angle) + Vector3.forward.z * Mathf.Cos(angle);
            spawnPoint *= spawnDistance;
            // Instantiate prefab at position
            Instantiate(policePrefab, transform.position + spawnPoint, Quaternion.identity, transform.parent);

            yield return new WaitForSeconds(spawnDelay);
        }
	}

    private IEnumerator MoveBridge()
	{
        Quaternion start1 = bridge1.rotation;
        Quaternion start2 = bridge2.rotation;
        Quaternion end1 = start1 * Quaternion.Euler(bridgeRotation);
        Quaternion end2 = start2 * Quaternion.Euler(-bridgeRotation);

        float t = 0;
        while (t < sequenceDurration)
		{
            bridge1.rotation = Quaternion.Slerp(start1, end1, t / sequenceDurration);
            bridge2.rotation = Quaternion.Slerp(start2, end2, t / sequenceDurration);

            t += Time.deltaTime;
            yield return null;
		}

        // Stop spawning police
        StopCoroutine(policeSpawner);
        // Allow the gap to be crossed
        bridgeCollider.SetActive(false);

        // Make the herd follow the player
        FlockingComponent.SetFlockTarget(GameObject.FindGameObjectWithTag("Player").transform);
    }


    public void OnHerdCollected()
	{
        herdCount--;

        if (herdCount <= 0)
		{
            canBeTriggered = true;
        }
	}
}
