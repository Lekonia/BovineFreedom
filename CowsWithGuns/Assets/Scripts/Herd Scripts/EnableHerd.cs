using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableHerd : MonoBehaviour
{
	public bool shouldDisableObject = false;

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

			this.enabled = false;
		}
	}
}
