using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoliceCarLogic : MonoBehaviour
{
    public Light sirenLight;
    public Color color1;
    public Color color2;
    public float delay = 1;

    private float t = 0;
    private bool usingColor1 = true;



	private void OnDisable()
	{
        sirenLight.enabled = false;
	}

	void Update()
    {
        if (t >= delay)
		{
            sirenLight.color = usingColor1 ? color2 : color1;
            usingColor1 = !usingColor1;
            t = 0;
		}

        t += Time.deltaTime;
    }
}
