using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody), typeof(ShootLogic)), DisallowMultipleComponent]
public class PoliceController : MonoBehaviour
{
    public float desiredSpeed = 5;
    [Space]
    public float seekRadius = 10;
    public float seekWeight = 2.5f;
    [Space]
    public float fireRadius = 7;
    public float stoppingWeight = 1;
    [Space]
    public float fleeRadius = 3;
    public float fleeWeight = 250;
    [Space]
    public Transform firePoint;
    public float chanceToHit = 0.5f;
    [Tooltip("Rate of fire in bullets per second")]
    public float fireRate = 1;

    private Transform player;
    private Rigidbody rb;
    private NavMeshAgent agent;
    private ShootLogic shootLogic;

    // The time of the last shot
    private float lastShotTime = 0;



    void Start()
    {
        // Check that ranges are in the correct order
        if (fireRadius > seekRadius)
            fireRadius = seekRadius;
        if (fleeRadius > fireRadius)
            fleeRadius = fireRadius;

        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        shootLogic = GetComponent<ShootLogic>();

        // Convert to time between firing
        fireRate = 1 / fireRate;

        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.speed = desiredSpeed;
    }

    void FixedUpdate()
    {
        float sqrDist = Vector3.SqrMagnitude(rb.position - player.position);

        // Too far from the player, do nothing
        if (sqrDist > seekRadius * seekRadius)
		{
            // Apply breaking force
            rb.AddForce(-rb.velocity * Time.fixedDeltaTime * stoppingWeight);
		}
        // Not close enough to fire, seek
        else if (sqrDist > fireRadius * fireRadius)
		{
            // Use nav mesh agent to pathfind
            agent.SetDestination(player.position);
            rb.AddForce((agent.desiredVelocity - rb.velocity) * Time.fixedDeltaTime * seekWeight, ForceMode.VelocityChange);
        }
        // Within firing range, stop and fire
        else if (sqrDist > fleeRadius * fleeRadius)
		{
            // Apply breaking force
            rb.AddForce(-rb.velocity * Time.fixedDeltaTime * stoppingWeight);

            // Fire at the player
            if (lastShotTime + fireRate <= Time.time)
			{
                Vector3 dir = (player.position - firePoint.position).normalized;
                // If we should miss the player, rotate the direction
                bool shouldHit = Random.Range(0f, 1f) < chanceToHit;
                if (!shouldHit)
				{
                    // Rotate by random angle to miss
                    float angle = Random.Range(10f, 20f) * Mathf.Deg2Rad;
                    dir.x = dir.x * Mathf.Cos(angle) - dir.z * Mathf.Sin(angle);
                    dir.z = dir.x * Mathf.Sin(angle) + dir.z * Mathf.Cos(angle);
                }
                // Fire
                shootLogic.Fire(firePoint.position, dir);

                lastShotTime = Time.time;
            }
        }
        // Too close, flee
        else
		{
            Vector3 dir = rb.position - player.position;
            rb.AddForce(dir.normalized * desiredSpeed * Time.fixedDeltaTime * fleeWeight);
        }

        // Update the nav mesh agent
        agent.velocity = rb.velocity;
        agent.nextPosition = rb.position;
    }
}
