using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 1;

    private MobileControllerScript mobileScript;
    private Rigidbody rb;
    private Plane plane = new Plane(Vector3.up, 0);



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
        //move using the rigidbody
        rb.MovePosition(rb.position + GetMovement() * moveSpeed * Time.fixedDeltaTime);
        //rotate by setting the forward direction
        Vector3 direction = GetDirection();
        if (direction != Vector3.zero)
		{
            transform.forward = GetDirection();
        }
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
    // Get the direction the player is facing
    private Vector3 GetDirection()
	{
        if (mobileScript.onMobile)
        {
            return new Vector3
            {
                x = mobileScript.joystickValues["Right Stick"].x,
                y = 0f,
                z = mobileScript.joystickValues["Right Stick"].y
            };
        }
        else
        {
            // Cast ray from cursor to plane
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            plane.Raycast(ray, out float enter);
            // Use the hit point relitive to the player as the direction
            Vector3 point = ray.GetPoint(enter) - transform.position;
            point.y = 0;
            return point;
        }
    }
}
