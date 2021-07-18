using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody), typeof(HealthLogic)), DisallowMultipleComponent]
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
    public float aimDelay = 0.3f;
    [Tooltip("Rate of fire in bullets per second")]
    public float fireRate = 1;

    private Transform player;
    private Rigidbody rb;
    private NavMeshAgent agent;
    private ShootLogic weapon;

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
        // Shoot script exists on child weapon object
        weapon = GetComponentInChildren<ShootLogic>();
        // Add listener for when we die
        GetComponent<HealthLogic>().OnDie.AddListener(Die);

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
                StartCoroutine(FireAtPosition(player.position));

                lastShotTime = Time.time;
            }
        }
        // Too close, flee
        else
		{
            Vector3 dir = rb.position - player.position;
            rb.AddForce(dir.normalized * desiredSpeed * Time.fixedDeltaTime * fleeWeight);
        }

        // Rotate to face the player
        transform.forward = Vector3.Lerp(transform.forward, (player.position - rb.position).normalized, Time.fixedDeltaTime * 10);

        // Update the nav mesh agent
        agent.velocity = rb.velocity;
        agent.nextPosition = rb.position;
    }

    private IEnumerator FireAtPosition(Vector3 pos)
	{
        yield return new WaitForSeconds(aimDelay);
        weapon.Fire(firePoint.position, pos - rb.position);
    }
    

    private void Die()
	{
        // Drop weapon
        weapon.SetWeaponDropState(true);

        Destroy(gameObject);
    }
}
