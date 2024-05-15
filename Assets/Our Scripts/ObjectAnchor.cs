using UnityEngine;

public class ObjectAnchor : MonoBehaviour
{

    [Header("Grasping Properties")]
    public float graspingRadius = 0.1f;

    // Store initial transform parent
    protected Transform initial_transform_parent;

    void Start()
    {
        initial_transform_parent = transform.parent;
    }


    // Store the hand controller this object will be attached to

    protected HandController grabbedBy = null;

    /* TODO: Also, make sure object does not collide with player (if in hand) */
    /* TODO: Something like SetPlayerIgnoreCollision(gameObject, true); */
    /* TODO: Read https://forum.unity.com/threads/ovr-grabble-causing-player-to-move-backwards-once-object-is-grabbed.717752/ */

    public void attach_to(HandController hand_controller, Collider collider)
    {
        /* TODO: What is a gameObject? Is it the prefined type? If so, why is it this color? */
        //if (collider.gameObject != gameObject) {
        //          return;
        //      }

        // Store the hand controller in memory
        this.grabbedBy = hand_controller;

        // Set the object to be placed in the hand controller referential
        transform.SetParent(hand_controller.transform);
    }

    public void detach_from(HandController hand_controller, Collider collider)
    {
        // Make sure that the right hand controller ask for the release
        if (this.grabbedBy != hand_controller) return;

        // Detach the hand controller
        this.grabbedBy = null;

        // Set the object to be placed in the original transform parent
        transform.SetParent(initial_transform_parent);
    }

    public bool is_grabbable() { return grabbedBy == null; }

    public float get_grasping_radius() { return graspingRadius; }
}