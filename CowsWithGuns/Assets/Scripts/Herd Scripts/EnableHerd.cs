using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableHerd : MonoBehaviour
{
    private List<FlockingComponent> herd = new List<FlockingComponent>();



    void Start()
    {
        herd.AddRange(GetComponentsInChildren<FlockingComponent>());
    }

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			foreach(var obj in herd)
			{
				obj.enabled = true;
			}

			this.enabled = false;
		}
	}
}
