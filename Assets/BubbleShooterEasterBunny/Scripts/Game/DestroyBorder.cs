using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DestroyBorder : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D coll)
    {
        OnTriggerEnter2D(coll.collider);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Destroy GameObject: " + other.gameObject.name);
        Destroy(other.gameObject);
    }
}
