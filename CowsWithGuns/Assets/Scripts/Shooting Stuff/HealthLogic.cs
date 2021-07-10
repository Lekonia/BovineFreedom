using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class HealthLogic : MonoBehaviour
{
    public float maxHealth = 3;

    public UnityEvent OnDie;
    public UnityEvent OnHit;

    private float currentHealth;


    void Start()
    {
        currentHealth = maxHealth;
    }

	public void DealDamage(float damage)
	{
        currentHealth -= damage;
        OnHit.Invoke();

        if (currentHealth <= 0)
		{
            OnDie.Invoke();
		}
	}
}
