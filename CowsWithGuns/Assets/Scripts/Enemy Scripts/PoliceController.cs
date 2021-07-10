using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody)), DisallowMultipleComponent]
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


    private Transform player;
    private Rigidbody rb;
    private NavMeshAgent agent;



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

            //fire

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
