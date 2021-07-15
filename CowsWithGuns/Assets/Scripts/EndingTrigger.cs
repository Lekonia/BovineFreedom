using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingTrigger : MonoBehaviour
{
	public int shopCount = 5;
	public GameObject endingScreen;
	public GameObject barrier;
	public Transform herdTarget;

	private bool canBeTriggered = false;



	void OnTriggerEnter(Collider other)
	{
		if (canBeTriggered && other.CompareTag("Player"))
		{
			// Disable player controls
			other.GetComponent<PlayerController>().enabled = false;
			other.GetComponent<PlayerShooting>().enabled = false;

			endingScreen.SetActive(true);
			FlockingComponent.SetFlockTarget(herdTarget);
		}
	}

	public void OnShopDestroied()
	{
        shopCount--;

        if (shopCount <= 0)
		{
			canBeTriggered = true;
			if (barrier != null)
				barrier.SetActive(false);
		}
    }
}
