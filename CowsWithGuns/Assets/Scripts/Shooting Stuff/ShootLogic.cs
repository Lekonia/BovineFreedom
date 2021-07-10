using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ShootLogic : MonoBehaviour
{
    public float maxDistance = 50;
    public LayerMask layerMask;
    public float damage = 1;

    [Space]
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
        // Update lines
        foreach (Line line in lines)
        {
            line.time += Time.deltaTime;
            line.renderer.enabled = (line.time <= 0.1f);
        }
    }

    public void Fire(Vector3 firePoint, Vector3 direction)
    {
        // Create and cast ray
        Ray ray = new Ray(firePoint, direction);
        Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance, layerMask);

        // Try to damage whever was hit
        if (hitInfo.transform != null)
		{
            if (hitInfo.transform.TryGetComponent<HealthLogic>(out HealthLogic healthLogic))
			{
                healthLogic.DealDamage(damage);
			}
		}

        // Enable line renderer to display shot
        lines[lineIndex].renderer.enabled = true;
        lines[lineIndex].renderer.SetPosition(0, firePoint);
        // If it hit something, use the contact point, else the max distance
        lines[lineIndex].renderer.SetPosition(1, hitInfo.transform == null ? ray.GetPoint(maxDistance) : hitInfo.point);
        lines[lineIndex].time = 0;

        // Update index
        lineIndex++;
        if (lineIndex >= lines.Length)
            lineIndex = 0;
    }
}
