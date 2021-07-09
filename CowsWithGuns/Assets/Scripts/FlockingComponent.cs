using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody)), DisallowMultipleComponent]
public class FlockingComponent : MonoBehaviour
{
	public Transform target;

	public float seperationWeight = 4;
	public float seperationRadius = 1;
	[Space]
	public float cohesionWeight = 1;
	public float cohesionRadius = 4;
	[Space]
	public float seekTargetSpeed = 1;
	public float seekWeight = 1;

	private float radius;

	// List containing all flocking agents
	private static List<FlockingComponent> flock = new List<FlockingComponent>();
	private Rigidbody rb;



    void Start()
    {
		rb = GetComponent<Rigidbody>();

		// Use the larger value for the radius
		radius = seperationRadius > cohesionRadius ? seperationRadius : cohesionRadius;
    }

	void OnEnable()
	{
		flock.Add(this);
	}
	void OnDisable()
	{
		flock.Remove(this);
	}

	void FixedUpdate()
    {
		// Get the local flock
		List<Transform> localFlock = new List<Transform>();
		foreach(var obj in flock)
		{
			if (obj != this && Vector3.SqrMagnitude(rb.position - obj.transform.position) < radius * radius)
			{
				localFlock.Add(obj.transform);
			}
		}

		// Apply the sum of forces
		Vector3 force = Seperation(localFlock) + Cohesion(localFlock) + Seek();
		rb.AddForce(force * Time.fixedDeltaTime);
    }


	private Vector3 Seek()
	{
		// Get the desired direction
		Vector3 desieredVel = target.position - rb.position;
		desieredVel.Normalize();

		// Multiply the heading by the max speed to get desiered velocity
		desieredVel *= seekTargetSpeed;
		// Find the force nessesary to change the velocity
		desieredVel -= rb.velocity;

		// If non-zero, normalise to the base value
		if (desieredVel != Vector3.zero)
			desieredVel = desieredVel.normalized * 100;
		return desieredVel * seekWeight;
	}
	private Vector3 Seperation(in List<Transform> swarm)
	{
		Vector3 force = Vector3.zero;
		if (swarm.Count == 0)
			return force;

		// Count the number of agents used
		int count = 0;

		foreach (Transform obj in swarm)
		{
			// Find the distance between the agents
			float sqrDist = Vector3.SqrMagnitude(rb.position - obj.position);

			// Add the direction from each agent
			if (sqrDist < seperationRadius * seperationRadius)
			{
				// Weigh the direction by the inverse of its distance.
				// This results in the force being stronger the closer they are
				Vector3 dir = rb.position - obj.position;
				dir = dir.normalized / Mathf.Sqrt(sqrDist);

				force += dir;
				count++;
			}
		}

		// If there were no agents, just return zero
		if (count <= 0)
			return Vector3.zero;
		
		// Find the mean
		force /= count;

		// Scale all forces to the same magnitude
		if (force != Vector3.zero)
			force = force.normalized * 100;
		return force * seperationWeight;
		
	}
	private Vector3 Cohesion(in List<Transform> swarm)
	{
		Vector3 force = Vector3.zero;
		if (swarm.Count == 0)
			return force;

		// Count the number of agents used
		float count = 0;

		foreach (Transform obj in swarm)
		{
			// Find the distance between the agents
			float sqrDist = Vector3.SqrMagnitude(rb.position - obj.position);

			if (sqrDist < cohesionRadius * cohesionRadius)
			{
				force += obj.position - rb.position;
				count++;
			}
		}

		// If there were no agents, just return zero
		if (count <= 0)
			return Vector3.zero;

		// Find the mean
		force /= count;
		
		// Scale all forces to the same magnitude
		if (force != Vector3.zero)
			force = force.normalized * 100;
		return force * cohesionWeight;
	}
}
