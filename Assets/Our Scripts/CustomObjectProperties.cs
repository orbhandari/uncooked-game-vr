using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Dynamically attached to an object the first an interaction has interacts with that object
public class CustomObjectProperties : MonoBehaviour
{
    // We only allow each object to be shrunk once.
    // If an object is shrunk, we can only enlarge it.
    // If an object is large, we can only shrink it.
    public bool isShrunk = false;
}
