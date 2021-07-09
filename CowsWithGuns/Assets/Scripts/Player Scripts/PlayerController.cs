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
            //create joysticks
            mobileScript.CreateNewJoystick("Left Stick", new Vector2(100, 100));
            mobileScript.CreateNewJoystick("Right Stick", new Vector2(-100, 100), new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0));
        }
    }


	void FixedUpdate()
	{
        Vector3 movement = GetMovement();
        //move using the rigidbody
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        //rotate by setting the forward direction
        if (movement.sqrMagnitude > 0.05f)
            transform.forward = GetMovement();
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
