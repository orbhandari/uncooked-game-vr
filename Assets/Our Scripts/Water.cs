using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnCollisionEnter(Collision collision)
    {
        // Destroy(this);
        if (!collision.gameObject.CompareTag("Container"))
        {
            Destroy(gameObject, 100f);
        }
        else gameObject.GetComponent<Rigidbody>().isKinematic = true;
    }
}
