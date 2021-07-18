using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 1;

    private MobileControllerScript mobileScript;
    private Rigidbody rb;



    void Awake()
    {
        mobileScript = GameObject.Find("Mobile Controller").GetComponent<MobileControllerScript>();
        rb = GetComponent<Rigidbody>();

        
        if (mobileScript.onMobile)
		{
            //create joystick
            mobileScript.CreateNewJoystick("Left Stick", new Vector2(100, 100));
        }
    }


	void FixedUpdate()
	{
        //move using the rigidbody
        rb.MovePosition(rb.position + GetMovement() * moveSpeed * Time.fixedDeltaTime);
	}

    // Read movement input
    private Vector3 GetMovement()
	{
        if (mobileScript.onMobile)
        {
            return new Vector3
            {
                x = mobileScript.joystickValues["Left Stick"].x,
                y = 0f,
                z = mobileScript.joystickValues["Left Stick"].y
            };
        }
        else
        {
            return new Vector3
            {
                x = Input.GetAxis("Horizontal"),
                y = 0,
                z = Input.GetAxis("Vertical")
            };
        }
    }
}
