using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeSequenceLogic : MonoBehaviour
{
    public GameObject guards;
    [Space]
    public GameObject policeCarPrefab;
    public float carTime = 3;

    public Vector3 startPos1;
    public Vector3 endPos1;
    public Vector3 curvePoint1;
    public Vector3 startPos2;
    public Vector3 endPos2;
    public Vector3 curvePoint2;
    [Space]
    public GameObject policePrefab;
    public float spawnDelay = 1;
    public float spawnDistance;
    public float spawnAngleMin;
    public float spawnAngleMax;
    [Space]
    public GameObject invisWalls;
    [Space]
    public float bridgeMoveTime = 15;
    public Vector3 bridgeRotation;
    [Space]
    public Transform bridge1;
    public Transform bridge2;
    public GameObject bridgeCollider;

    private Coroutine policeSpawner;
    private bool canBeTriggered = false;



    // Called when the previous objective has been completed
    public void Activate()
	{
        canBeTriggered = true;
        guards.SetActive(true);
	}
    public void BeginSequence()
	{
        if (!canBeTriggered)
		{
            return;
		}
        canBeTriggered = false;

        StartCoroutine(MovePoliceCars());
        // "Get to the bridge"
        ObjectiveManager.instance.CountedTaskComplete(3);
    }


    private IEnumerator MovePoliceCars()
	{
        // Spawn cars
        Transform car1 = Instantiate(policeCarPrefab, startPos1, Quaternion.identity, transform).transform;
        Transform car2 = Instantiate(policeCarPrefab, startPos2, Quaternion.identity, transform).transform;

        float t = 0;
        while (t < carTime)
		{
            // Move car 1
            Vector3 lastPos1 = car1.position;
            car1.localPosition = GetPointOnCurve(t / carTime, startPos1, curvePoint1, endPos1);
            car1.forward = car1.position - lastPos1;
            // Move car 2
            Vector3 lastPos2 = car2.position;
            car2.localPosition = GetPointOnCurve(t / carTime, startPos2, curvePoint2, endPos2);
            car2.forward = car2.position - lastPos2;

            t += Time.deltaTime;
            yield return null;
		}

        yield return new WaitForSeconds(1);

        // Stop sirens
        car1.GetComponent<PoliceCarLogic>().enabled = false;
        car2.GetComponent<PoliceCarLogic>().enabled = false;

        // Spawn police at the sides of the cars
        Instantiate(policePrefab, car1.position + car1.right * 2, Quaternion.identity);
        Instantiate(policePrefab, car1.position + -car1.right * 2, Quaternion.identity);
        Instantiate(policePrefab, car2.position + car2.right * 2, Quaternion.identity);
        Instantiate(policePrefab, car2.position + -car2.right * 2, Quaternion.identity);


        // Start next phase after delay
        yield return new WaitForSeconds(2);
        policeSpawner = StartCoroutine(SpawnPolice());
        StartCoroutine(MoveBridge());
	}

    private Vector3 GetPointOnCurve(float t, in Vector3 p1, in Vector3 p2, in Vector3 p3)
    {
        if (t < 0 || t > 1)
        {
            return Vector3.zero;
        }

        float c = 1.0f - t;

        // The Bernstein polynomials
        float bb0 = c * c;
        float bb1 = 2 * c * t;
        float bb2 = t * t;

        // Return the point
        return p1 * bb0 + p2 * bb1 + p3 * bb2;
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
        while (t < bridgeMoveTime)
		{
            bridge1.rotation = Quaternion.Slerp(start1, end1, t / bridgeMoveTime);
            bridge2.rotation = Quaternion.Slerp(start2, end2, t / bridgeMoveTime);

            t += Time.deltaTime;
            yield return null;
		}

        // Stop spawning police
        StopCoroutine(policeSpawner);
        // Allow the gap to be crossed
        bridgeCollider.SetActive(false);
        invisWalls.SetActive(false);
        // Complete the objective
        ObjectiveManager.instance.CountedTaskComplete(4);
    }
}
