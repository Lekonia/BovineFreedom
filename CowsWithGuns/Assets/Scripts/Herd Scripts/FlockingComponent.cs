using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody), typeof(NavMeshAgent), typeof(HealthLogic)), DisallowMultipleComponent]
public class FlockingComponent : MonoBehaviour
{
	public Transform target;
	[Space]
	public float seperationWeight = 4;
	public float seperationRadius = 1;
	public float seperationDistanceMult = 1;
	public float seperationDistancePower = 2;
	[Space]
	public float cohesionWeight = 1;
	public float cohesionRadius = 4;
	[Space]
	public float seekTargetSpeed = 1;
	public float seekWeight = 1;
	public float seekStoppingDistance = 2;
	[Space]
	public float pathfindDistance = 20;
	[Space]
	public Transform firePoint;
	public LayerMask layerMask;

	private float radius;

	// List containing all flocking agents
	private static List<FlockingComponent> flock = new List<FlockingComponent>();
	private Rigidbody rb;
	private NavMeshAgent agent;

	private ShootLogic weapon;
	[HideInInspector]
	public bool hasWeapon = false;
	public static List<FlockingComponent> cowsWithGuns = new List<FlockingComponent>();



	// Set the target transform for all agents in the flock
	public static void SetFlockTarget(Transform newTarget)
	{
		foreach (FlockingComponent obj in flock)
		{
			obj.target = newTarget;
		}
	}


    void Start()
    {
		rb = GetComponent<Rigidbody>();
		agent = GetComponent<NavMeshAgent>();
		agent.updatePosition = false;
		agent.updateRotation = false;
		// Check if we already have a gun
		if (TryGetComponent<ShootLogic>(out ShootLogic shootLogic))
		{
			transform.localScale = Vector3.one;

			weapon = shootLogic;
			weapon.SetWeaponDropState(false);
			weapon.layerMask = layerMask;
			hasWeapon = true;
		}
		// Add listener for when we die
		GetComponent<HealthLogic>().OnDie.AddListener(Die);

		// Use the larger value for the radius
		radius = seperationRadius > cohesionRadius ? seperationRadius : cohesionRadius;
    }

	void OnEnable()
	{
		flock.Add(this);
		if (hasWeapon)
		{
			cowsWithGuns.Add(this);
		}
	}
	void OnDisable()
	{
		flock.Remove(this);
		if (hasWeapon)
		{
			cowsWithGuns.Remove(this);
		}
	}

	void FixedUpdate()
    {
		// If we are too far from the target, start pathfinding since we may be stuck
		float sqrDist = Vector3.SqrMagnitude(transform.position - target.position);
		if (sqrDist > pathfindDistance * pathfindDistance)
		{
			agent.SetDestination(target.position);
			rb.velocity = agent.desiredVelocity;
		}
		else
		{
			// Get the local flock
			List<Transform> localFlock = new List<Transform>();
			foreach (var obj in flock)
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

		// Rotate to face the direction of velocity
		Vector3 dir = Vector3.Lerp(transform.forward, rb.velocity.normalized, Time.fixedDeltaTime * 5);
		dir.y = 0;
		transform.forward = (dir == Vector3.zero) ? transform.forward : dir;

		// Update agent
		agent.velocity = rb.velocity;
		agent.nextPosition = rb.position;
    }

	void OnTriggerEnter(Collider other)
	{
		// If we dont have a weapon and have touched one, take it
		if (!hasWeapon && other.CompareTag("Weapon"))
		{
			cowsWithGuns.Add(this);
			transform.localScale = Vector3.one;

			other.transform.parent = transform;
			other.transform.localPosition = Vector3.zero;
			weapon = other.GetComponent<ShootLogic>();
			weapon.SetWeaponDropState(false);
			hasWeapon = true;

			weapon.layerMask = layerMask;
		}
	}


	public void Shoot(Vector3 aimPoint)
	{
		if (weapon != null)
		{
			Vector3 dir = aimPoint - firePoint.position;
			dir.y = 0;
			weapon.Fire(firePoint.position, dir);
		}
	}

	private void Die()
	{
		// If we have a weapon, drop it
		if (weapon != null)
		{
			cowsWithGuns.Remove(this);
			weapon.SetWeaponDropState(true);
			hasWeapon = false;
		}

		Destroy(gameObject);
	}

	private Vector3 Seek()
	{
		// Get the desired direction
		Vector3 desieredVel = target.position - rb.position;
		float sqrDist = Vector3.SqrMagnitude(desieredVel);
		desieredVel.Normalize();

		// Multiply the heading by the max speed to get desiered velocity
		desieredVel *= seekTargetSpeed;
		// Find the force nessesary to change the velocity
		desieredVel -= rb.velocity;

		// If non-zero, normalise to the base value
		if (desieredVel != Vector3.zero)
			desieredVel = desieredVel.normalized * 100;

		// Make force counteract current velocity to slow down as we approach the target, making this arrival and not seek
		if (sqrDist < seekStoppingDistance * seekStoppingDistance)
		{
			desieredVel *= (Mathf.Sqrt(sqrDist) / seekStoppingDistance);
			desieredVel -= rb.velocity * 35;	//magic number to counteract base value
		}

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
				Vector3 dir = rb.position - obj.position;
				// Weigh the direction by a factor of the distance
				float distanceFactor = seperationRadius / Mathf.Sqrt(sqrDist) - 1;
				distanceFactor = Mathf.Pow(distanceFactor, seperationDistancePower) * seperationDistanceMult;
				dir = dir.normalized * distanceFactor;

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
