using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PouringContainer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Angle(transform.up, Vector3.up) > 105.0f) {
            foreach (Transform child in transform) {
                child.SetParent(null);
                // please dont remove these two lines, the interaction won't work otherwise. Why are you like this unity????
                child.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                child.gameObject.GetComponent<Rigidbody>().isKinematic = false;
            }

        }
    }

    void OnTriggerEnter(Collider other) {
        other.transform.SetParent(transform);
    }
}
