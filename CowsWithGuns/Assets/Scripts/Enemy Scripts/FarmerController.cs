using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody)), DisallowMultipleComponent]
public class FarmerController : MonoBehaviour
{
    public float desiredSpeed = 5;
    [Space]
    [Tooltip("The point the farmer is running towards")]
    public Transform seekTarget;
    public float seekWeight = 2.5f;
    [Space]
    public float fleeRadius = 10;
    public float fleeWeight = 250;

    private Transform player;
    private NavMeshAgent navAgent;
    private Rigidbody rb;



    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        navAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        navAgent.SetDestination(seekTarget.position);
        navAgent.updatePosition = false;
        navAgent.updateRotation = false;
        navAgent.speed = desiredSpeed;
    }

    void FixedUpdate()
    {
        // Apply seek force
        rb.AddForce((navAgent.desiredVelocity - rb.velocity) * Time.fixedDeltaTime * seekWeight, ForceMode.VelocityChange);

        // If we are close to the player, flee
        if (Vector3.SqrMagnitude(player.position - rb.position) <= fleeRadius * fleeRadius)
		{
            Vector3 dir = rb.position - player.position;
            rb.AddForce(dir.normalized * desiredSpeed * Time.fixedDeltaTime * fleeWeight);
		}

        // Update the nav mesh agent
        navAgent.velocity = rb.velocity;
        navAgent.nextPosition = rb.position;
        navAgent.SetDestination(seekTarget.position);
    }
}
