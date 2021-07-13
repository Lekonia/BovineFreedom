using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnableHerd : MonoBehaviour
{
	public bool shouldDisableObject = false;
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

			OnCollected.Invoke();
			this.enabled = false;
		}
	}
}
