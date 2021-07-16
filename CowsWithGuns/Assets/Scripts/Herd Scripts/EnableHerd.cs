using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnableHerd : MonoBehaviour
{
	public bool shouldDisableObject = false;
	[Tooltip("The objective this task belongs to. -1 belongs to no objective")]
	public int objectiveIndex = -1;
	[Space]
	public UnityEvent OnCollected;

    private List<FlockingComponent> herd = new List<FlockingComponent>();



    void Start()
    {
        herd.AddRange(GetComponentsInChildren<FlockingComponent>());

		foreach (var obj in herd)
		{
			if (shouldDisableObject)
			{
				obj.gameObject.SetActive(false);
			}
			else
			{
				obj.enabled = false;
			}
		}
    }

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			foreach(var obj in herd)
			{
				if (shouldDisableObject)
				{
					obj.gameObject.SetActive(true);
				}
				else
				{
					obj.enabled = true;
				}
			}

			if (objectiveIndex != -1)
			{
				ObjectiveManager.instance.CountedTaskComplete(objectiveIndex);
			}

			OnCollected.Invoke();
			this.enabled = false;
		}
	}
}
