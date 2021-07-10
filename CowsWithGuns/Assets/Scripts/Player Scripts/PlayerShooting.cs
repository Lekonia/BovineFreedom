using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ShootLogic)), DisallowMultipleComponent]
public class PlayerShooting : MonoBehaviour
{
    public Transform firePoint;
    [Space]
    [Tooltip("Fire rate in bullets per second")]
    public float fireRate = 1;
    public float randomSpread = 0;

    private ShootLogic shootLogic;
    // The time the last shot occured at
    private float lastShotTime = 0;



    void Start()
    {
        // Convert to time between firing
        fireRate = 1 / fireRate;
        // Convert from degrees to radians
        randomSpread *= Mathf.Deg2Rad;

        shootLogic = GetComponent<ShootLogic>();
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && lastShotTime + fireRate <= Time.time)
		{
            // Get a direction with a random rotation
            Vector3 dir = firePoint.forward;
            float angle = Random.Range(-randomSpread, randomSpread);
            dir.x = dir.x * Mathf.Cos(angle) - dir.z * Mathf.Sin(angle);
            dir.z = dir.x * Mathf.Sin(angle) + dir.z * Mathf.Cos(angle);
            // Fire
            shootLogic.Fire(firePoint.position, dir);

            lastShotTime = Time.time;
        }
    }
}
