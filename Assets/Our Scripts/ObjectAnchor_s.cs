using UnityEngine;
using System.Collections.Generic;


public class ObjectAnchor_s : MonoBehaviour
{

    [Header("Grasping Properties")]
    public float graspingRadius = 0.1f;

    // Store initial transform parent
    protected Transform initial_transform_parent;


    public int maxSliceCount = 3;
    public int sliceCount = 0;



    public bool CanBeSlicedAgain()
    {
        return sliceCount < maxSliceCount;
    }

    public void IncrementSliceCount()
    {
        sliceCount++;
    }

    void Update()
    {
        if (handControllers.Count > 0)
        {
            Vector3 targetPosition = Vector3.zero;
            foreach (var hand in handControllers)
            {
                targetPosition += hand.transform.position;
            }
            targetPosition /= handControllers.Count;

            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10);
        }
    }



    void Start()
    {
        initial_transform_parent = transform.parent;

        BoxCollider collider = GetComponent<BoxCollider>();
        if (collider != null)
        {
            Vector3 size = collider.size;
            float maxDimension = Mathf.Max(size.x, size.y, size.z) * 0.5f;
            graspingRadius = maxDimension;
        }
    }



    // Store the hand controller this object will be attached to 
    //protected HandController hand_controller = null; //comment out because an object when two hand interaction, an object should be conytolled by multiple conytollers.

    protected List<HandController_s> handControllers = new List<HandController_s>();

    /*
    public void attach_to(HandController_s hand_controller)
    {
        if (!handControllers.Contains(hand_controller))
        {
            handControllers.Add(hand_controller);
            // comment
            // transform.SetParent(hand_controller.transform, false);

            if (GetComponent<Rigidbody>() != null)
            {
                GetComponent<Rigidbody>().isKinematic = true;
            }
        }
    }
    */

    public void attach_to(HandController_s hand_controller)
    {
        if (!handControllers.Contains(hand_controller))
        {
            handControllers.Add(hand_controller);
            transform.SetParent(hand_controller.transform);
            transform.localPosition = Vector3.zero;

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
        }
    }



    /*
    public void detach_from(HandController_s hand_controller)
    {
        if (handControllers.Contains(hand_controller))
        {
            handControllers.Remove(hand_controller);
            if (handControllers.Count == 0)
            {
                transform.SetParent(initial_transform_parent, false);
                if (GetComponent<Rigidbody>() != null)
                {
                    GetComponent<Rigidbody>().isKinematic = false;
                }
            }
        }
    }
    */

    public void detach_from(HandController_s hand_controller)
    {
        if (handControllers.Contains(hand_controller))
        {
            handControllers.Remove(hand_controller);

            if (handControllers.Count == 0)
            {
                transform.SetParent(initial_transform_parent);

                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                }
            }
        }
    }


    //allow an object can still be grabbed after one hand grab
    public bool is_available()
    {
        return true;
    }


    public float get_grasping_radius() { return graspingRadius; }


    public void SetupAsGrabbable()
    {
       // Debug.Log("Setting up grabbable for: " + gameObject.name);

        // Ensure the collider is convex and ready for physics interactions
        MeshCollider collider = GetComponent<MeshCollider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<MeshCollider>();
            collider.convex = true;
        }
        else
        {
            collider.convex = true;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = false;

        gameObject.tag = "Grabbable";
    }


}

