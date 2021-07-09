using UnityEngine;

public class ShatterFragmentLogic : MonoBehaviour
{
    float min, max, radius;
    Rigidbody myBody;
    Vector3 myPos, myRot, myScale;
    GameObject ExplosionPoint;
    bool run;
    public void Initialize(float _Force, GameObject _Point, float _Radius)
    {
        myPos = transform.localPosition;
        myRot = transform.localEulerAngles;
        myScale = transform.localScale;

        myBody = gameObject.GetComponent<Rigidbody>();
        myBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        min = _Force * 0.9f;
        max = _Force * 1.1f;
        radius = _Radius;

        ExplosionPoint = _Point;

        run = false;
    }
    private void FixedUpdate()
    {
        if(run)
        {
            run = false;
            Explode();
        }
    }
    private void OnEnable()
    {
        ResetFragment();
        run = true;
    }
    private void ResetFragment()
    {
        myBody.isKinematic = true;
        transform.localPosition = myPos;
        transform.localEulerAngles = myRot;
        transform.localScale = myScale;
        myBody.position = transform.position;
    }
    private void Explode()
    {
        myBody.isKinematic = false;
        myBody.AddExplosionForce(Random.Range(min, max), ExplosionPoint.transform.position, radius);
    }
}