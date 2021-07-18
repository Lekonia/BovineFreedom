using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShatterOnCollision : MonoBehaviour
{
    public GameObject replacement;

    void OnCollisionEnter()
    {
        GameObject.Instantiate(replacement, transform.position, transform.rotation);

        Destroy(gameObject);
    }
}
