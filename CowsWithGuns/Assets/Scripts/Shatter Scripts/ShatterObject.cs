using System;
using System.Collections.Generic;
using UnityEngine;

public class ShatterObject : MonoBehaviour
{
    [Space]
    [SerializeField] int TriangleCount;
    [Space]
    [SerializeField] float ExplosionForce; 
    [SerializeField] float ExplosionRadius;
    [SerializeField] Vector3 ExplosionPointOffset;
    [Space]
    [SerializeField] GameObject DisableObject;
    [SerializeField] float DisableDelay;

    RendererVariables Meshs;
    GameObject ShatterContainer, ExplosionPoint;
    bool run;
    float elapsed;
    int a = 0;

    private void Awake()
    {
        run = false;

        Meshs._Filter = new List<MeshFilter>();
        Meshs._Renderer = new List<MeshRenderer>();
        Meshs._Transform = new List<Transform>();

        ExplosionPoint = new GameObject();
        ExplosionPoint.transform.position = transform.position + ExplosionPointOffset;
        ExplosionPoint.transform.SetParent(transform);
        ExplosionPoint.gameObject.name = gameObject.name + "ExplosionPoint";

        //Create Duplicate
        InstantiateBrokenObject();

        CreateContainer();
    }
    private void FixedUpdate()
    {
        if(run)
        {
            elapsed += Time.fixedDeltaTime;
            if(elapsed >= DisableDelay)
            {
                elapsed = 0f;
                run = false;
                DisableObject.SetActive(false);
            }
        }
    }
    private void InstantiateBrokenObject()
    {
        //add meshes
        gameObject.TryGetComponent(out MeshFilter _mf);
        if (_mf != null)
        {
            Meshs._Filter.Add(_mf);
            Meshs._Renderer.Add(gameObject.GetComponent<MeshRenderer>());
            Meshs._Transform.Add(transform);
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).TryGetComponent(out _mf);
            if (_mf != null)
            {
                Meshs._Filter.Add(_mf);
                Meshs._Renderer.Add(transform.GetChild(i).GetComponent<MeshRenderer>());
                Meshs._Transform.Add(transform.GetChild(i));
                Meshs._Transform[Meshs._Transform.Count - 1].localScale = Vector3.Scale(Meshs._Transform[Meshs._Transform.Count - 1].localScale, transform.localScale);
            }
        }
    }

    void CreateContainer()
    {
        //create container
        ShatterContainer = new GameObject();
        ShatterContainer.name = gameObject.name + "ShatterContainer";
        ShatterContainer.transform.position = transform.position;
        ShatterContainer.transform.SetParent(transform);
        ShatterContainer.transform.localScale = Vector3.one;
        ShatterContainer.SetActive(false);

        //create Objects
        for (int i = 0; i < Meshs._Filter.Count; i++)
        {
            CreateFragment(i);
        }
    }
    void CreateFragment(int i)
    {
        GameObject ob;
        //test code section
        //grab the information needed
        int[] triangles = Meshs._Filter[i].mesh.triangles;
        Vector3[] vertices = Meshs._Filter[i].mesh.vertices;
        int y = 0;

        a = 0;

        while((a + (TriangleCount * 3)) < triangles.Length)
        {//create the game object
            ob = new GameObject();
            ob.name = "Fragment" + y.ToString("D3");
            y++;

            //apply mesh information to the fragment
            //Mesh mesh = ob.GetComponent<MeshFilter>().mesh;
            Mesh mesh = new Mesh();

            int[] _temptemp = new int[TriangleCount * 3];
        
            for (int x = 0; x < (TriangleCount * 3); x += 3)
            {
                _temptemp[x + 0] = triangles[x + a + 0];
                _temptemp[x + 1] = triangles[x + a + 1];
                _temptemp[x + 2] = triangles[x + a + 2];
            }         

            mesh.vertices = vertices;
            mesh.uv = Meshs._Filter[i].mesh.uv;
            mesh.uv2 = Meshs._Filter[i].mesh.uv2;
            mesh.uv3 = Meshs._Filter[i].mesh.uv3;
            mesh.uv4 = Meshs._Filter[i].mesh.uv4;
            mesh.uv5 = Meshs._Filter[i].mesh.uv5;
            mesh.uv6 = Meshs._Filter[i].mesh.uv6;
            mesh.uv7 = Meshs._Filter[i].mesh.uv7;
            mesh.uv8 = Meshs._Filter[i].mesh.uv8;
            mesh.triangles = _temptemp;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.Optimize();

            AddComponents(ob, i, mesh);

            a += (TriangleCount * 3);
        }

        if (a < triangles.Length)
        {
            int[] _temptemp = new int[TriangleCount * 3];

            for (int x = 0; x < triangles.Length - a; x += 3)
            {
                _temptemp[x + 0] = triangles[x + a + 0];
                _temptemp[x + 1] = triangles[x + a + 1];
                _temptemp[x + 2] = triangles[x + a + 2];
            }

            ob = new GameObject();
            ob.name = "Fragment" + y.ToString("D3");
            y++;

            Mesh mesh = new Mesh();

            //apply mesh information to the fragment

            mesh.vertices = vertices;
            mesh.uv = Meshs._Filter[i].mesh.uv;
            mesh.uv2 = Meshs._Filter[i].mesh.uv2;
            mesh.uv3 = Meshs._Filter[i].mesh.uv3;
            mesh.uv4 = Meshs._Filter[i].mesh.uv4;
            mesh.uv5 = Meshs._Filter[i].mesh.uv5;
            mesh.uv6 = Meshs._Filter[i].mesh.uv6;
            mesh.uv7 = Meshs._Filter[i].mesh.uv7;
            mesh.uv8 = Meshs._Filter[i].mesh.uv8;
            mesh.triangles = _temptemp;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.Optimize();

            AddComponents(ob, i, mesh);
        }
    }

    void AddComponents(GameObject ob, int i, Mesh mesh)
    {
        ob.transform.SetParent(ShatterContainer.transform);
        ob.transform.position = Meshs._Transform[i].position;
        ob.transform.rotation = Meshs._Transform[i].rotation;
        ob.transform.localScale = Meshs._Transform[i].localScale;
        

        //add components
        ob.AddComponent<MeshFilter>();
        ob.GetComponent<MeshFilter>().mesh = mesh;
        ob.AddComponent<MeshRenderer>();
        ob.GetComponent<MeshRenderer>().sharedMaterial = Meshs._Renderer[i].sharedMaterial;
        ob.AddComponent<MeshCollider>();
        ob.GetComponent<MeshCollider>().sharedMesh = mesh;
        ob.GetComponent<MeshCollider>().convex = true;
        ob.AddComponent<Rigidbody>();

        ob.transform.localScale = Vector3.one;

        ob.AddComponent<ShatterFragmentLogic>();
        ob.GetComponent<ShatterFragmentLogic>().Initialize(ExplosionForce, ExplosionPoint, ExplosionRadius);
    }


	private void OnCollisionEnter(Collision collision)
	{
        // If we collide with a herd agent, shatter
        if (collision.gameObject.layer == LayerMask.NameToLayer("Cow"))
		{
            run = true;
            ShatterContainer.SetActive(true);
            foreach (MeshRenderer mr in Meshs._Renderer)
                mr.enabled = false;

            GetComponent<Collider>().enabled = false;
        }
    }

    private void OnEnable()
    {
        // Reset the shattered object
        ShatterContainer.SetActive(false);
        foreach (MeshRenderer mr in Meshs._Renderer)
            mr.enabled = true;
    }
}
struct RendererVariables
{
    public List<MeshFilter> _Filter;
    public List<MeshRenderer> _Renderer;
    public List<Transform> _Transform;
}
