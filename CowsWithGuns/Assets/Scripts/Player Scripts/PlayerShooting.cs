using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerShooting : MonoBehaviour
{
    [Tooltip("Fire rate in bullets per second")]
    public float fireRate = 1;
    public float cowNumMulti = 1;
    public int maxCowNum = 3;

    private MobileControllerScript mobileScript;
    private Plane plane = new Plane(Vector3.up, 0);

    // The time the last shot occured at
    private float lastShotTime = 0;
    private int lastCowIndex = 0;



    void Start()
    {
        mobileScript = GameObject.Find("Mobile Controller").GetComponent<MobileControllerScript>();
        if (mobileScript.onMobile)
        {
            //create joystick
            mobileScript.CreateNewJoystick("Right Stick", new Vector2(-100, 100), new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0));
        }

        // Convert to time between firing
        fireRate = 1 / fireRate;
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && lastShotTime + fireRate <= Time.time)
		{
            //get cows with guns
            var flock = FlockingComponent.cowsWithGuns;
            if (flock.Count <= 0)
                return;

            // Determine how many cows will shoot
            int numFiring = (int)(flock.Count * fireRate * cowNumMulti);
            if (numFiring < 1)
			{
                numFiring = 1;
			}
            else if (numFiring > maxCowNum)
			{
                numFiring = maxCowNum;
			}

            // Get the point the player is aiming at
            Vector3 point = GetAimPoint();

            for (int i = 0; i < numFiring; i++)
			{
                int index = lastCowIndex + i;
                index %= flock.Count;

                flock[index].Shoot(point);
            }

            // Update last index
            lastCowIndex += numFiring + 1;
            lastCowIndex %= flock.Count;

            lastShotTime = Time.time;


            //// Get a direction with a random rotation
            //Vector3 dir = firePoint.forward;
            //float angle = Random.Range(-randomSpread, randomSpread);
            //dir.x = dir.x * Mathf.Cos(angle) - dir.z * Mathf.Sin(angle);
            //dir.z = dir.x * Mathf.Sin(angle) + dir.z * Mathf.Cos(angle);
            //// Fire
            //shootLogic.Fire(firePoint.position, dir);
            //
            //lastShotTime = Time.time;
        }
    }


    private Vector3 GetAimPoint()
    {
        if (mobileScript.onMobile)
        {
            Vector3 dir = new Vector3
            {
                x = mobileScript.joystickValues["Right Stick"].x,
                y = 0f,
                z = mobileScript.joystickValues["Right Stick"].y
            };

            //this should eventualy use a wide raycast to find something the player is aiming at, but for now...
            dir = transform.position + dir * 10;

            return dir;
        }
        else
        {
            // Cast ray from cursor to plane
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            plane.Raycast(ray, out float enter);
            // Get the hit point
            Vector3 point = ray.GetPoint(enter);
            point.y = 0;
            return point;
        }
    }
}
