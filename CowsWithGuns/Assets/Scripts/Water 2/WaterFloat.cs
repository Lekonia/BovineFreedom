using Ditzelgames;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterFloat : MonoBehaviour
{
    //Public Properties
    public float AirDrag = 1;
    public float WaterDrag = 10;
    public bool AffectDirection = true;
    public Transform[] FloatPoints;
    public bool AttachToSurface = false;

    //Used Components
    protected Rigidbody Rigidbody;
    protected Waves Waves;

    //Water Line
    protected float WaterLine;
    protected Vector3[] WaterLinePoints;

    //Help Vectors
    protected Vector3 centerOffset;
    protected Vector3 smoothVectorRotation;
    protected Vector3 TargetUp;

    public Vector3 Center
    {
        get
        {
            return transform.position + centerOffset;
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        Waves = FindObjectOfType<Waves>();
        Rigidbody = GetComponent<Rigidbody>();
        Rigidbody.useGravity = false;

        //Compute Center
        WaterLinePoints = new Vector3[FloatPoints.Length];

        for (int i = 0; i < FloatPoints.Length; i++)
            WaterLinePoints[i] = FloatPoints[i].position;
        centerOffset = PhysicsHelper.GetCenter(WaterLinePoints) - transform.position;
    }

    // Update is called once per frame
    void FixedUpdate() //Not Update!! FIXED UPDATE!! It is Physics!!
    {
        //Default Water Surface
        var newWaterLine = 0f;
        var pointUnderWater = false;

        //Set WaterLinePoints and WaterLine
        for (int i = 0; i < FloatPoints.Length; i++)
        {
            //Height
            WaterLinePoints[i] = FloatPoints[i].position;
            WaterLinePoints[i].y = Waves.GetHeight(FloatPoints[i].position);
            newWaterLine += WaterLinePoints[i].y / FloatPoints.Length;

            if (WaterLinePoints[i].y > FloatPoints[i].position.y)
                pointUnderWater = true;
        }

        var waterLineDelta = newWaterLine - WaterLine;
        WaterLine = newWaterLine;

        //Gravity
        var gravity = Physics.gravity;
        Rigidbody.drag = AirDrag;

        if (WaterLine > Center.y)
        {
            Rigidbody.drag = WaterDrag;

            //Under Water
            if (AttachToSurface)
            {
                //Attach to Water Surface
                Rigidbody.position = new Vector3(Rigidbody.position.x, WaterLine - centerOffset.y, Rigidbody.position.z);
            }

            else
            {
                //Go Up
                //gravity = -Physics.gravity;
                gravity = AffectDirection ? TargetUp * -Physics.gravity.y : -Physics.gravity; //Testing a new line of code
                transform.Translate(Vector3.up * waterLineDelta * 0.9f);
            }
        }

        Rigidbody.AddForce(gravity * Mathf.Clamp(Mathf.Abs(WaterLine - Center.y), 0, 1));

        //Compute Up Vector
        TargetUp = PhysicsHelper.GetNormal(WaterLinePoints);

        //Rotation
        if (pointUnderWater)
        {
            //Attach to Water Surface
            TargetUp = Vector3.SmoothDamp(transform.up, TargetUp, ref smoothVectorRotation, 0.2f);
            Rigidbody.rotation = Quaternion.FromToRotation(transform.up, TargetUp) * Rigidbody.rotation;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        if (FloatPoints == null)
            return;

        for (int i = 0; i < FloatPoints.Length; i++)
        {
            if (FloatPoints[i] == null)
                continue;

            if (Waves != null)
            {
                //Draw Cube
                Gizmos.color = Color.red;
                Gizmos.DrawCube(WaterLinePoints[i], Vector3.one * 0.3f);
            }

            //Draw Sphere
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(FloatPoints[i].position, 0.1f);
        }

        //Draw Center
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(new Vector3(Center.x, WaterLine, Center.z), Vector3.one * 1f);
            Gizmos.DrawRay(new Vector3(Center.x, WaterLine, Center.z), TargetUp * 1f);
        }
    }
}
