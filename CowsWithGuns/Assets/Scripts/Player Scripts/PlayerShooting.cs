using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public float maxDistance = 50;
    
    public float damage = 1;
    [Tooltip("Fire rate in bullets per second")]
    public float fireRate = 1;
    public float randomSpread = 0;

    // The time the last shot occured at
    private float lastShotTime = 0;


    public Material lineMat;
    public Gradient lineColor;

    class Line
	{
        public LineRenderer renderer;
        public float time;
	}
    private Line[] lines;
    private int lineIndex = 0;



    void Start()
    {
        // Convert to time between firing
        fireRate = 1 / fireRate;
        // Convert from degrees to radians
        randomSpread *= Mathf.Deg2Rad;

        // Setup line renderers
        lines = new Line[10];
        for (int i = 0; i < lines.Length; i++)
		{
            lines[i] = new Line();

            GameObject obj = new GameObject("Line Renderer");
            obj.transform.parent = transform;
            lines[i].renderer = obj.AddComponent<LineRenderer>();
            lines[i].renderer.enabled = false;
            lines[i].renderer.material = lineMat;
            lines[i].renderer.colorGradient = lineColor;
            lines[i].renderer.widthMultiplier = 0.1f;
        }
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && lastShotTime + fireRate <= Time.time)
		{
            Fire();
		}

        // Update lines
        foreach (Line line in lines)
		{
            line.time += Time.deltaTime;
            line.renderer.enabled = (line.time <= 0.1f);
		}
    }


    private void Fire()
	{
        Vector3 dir = transform.forward;
        // Rotate dir by random angle
        float angle = Random.Range(-randomSpread, randomSpread);
        dir.x = dir.x * Mathf.Cos(angle) - dir.z * Mathf.Sin(angle);
        dir.z = dir.x * Mathf.Sin(angle) + dir.z * Mathf.Cos(angle);


        Ray ray = new Ray(transform.position, dir);

        //do ray cast


        // Enable line renderer to display shot
        lines[lineIndex].renderer.enabled = true;
        lines[lineIndex].renderer.SetPosition(0, transform.position);
        lines[lineIndex].renderer.SetPosition(1, ray.GetPoint(100));    //change to hit point
        lines[lineIndex].time = 0;

        lineIndex++;
        if (lineIndex >= lines.Length)
            lineIndex = 0;

        lastShotTime = Time.time;
	}
}
