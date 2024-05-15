using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

// Assigned to R Hand controller to not conflict with Teleport
public class DistanceGrab : MonoBehaviour
{
    public Material highlightMaterial; // Material for highlighting
    public LineRenderer lineRenderer;
    public float maxDistance = 10f; // Maximum distance for the raycast
    public AudioSource audioSource;
    public Material grabbableLineMat;

    private Material defaultLineMat;
    private GameObject previousHitObject; // Previously highlighted object
    private Material previousHighlightMaterial;

    private GameObject hitObject;
    private GameObject grabbedObject;
    private Transform initialTransformParent;
    private bool isObjectClose = false;

    // Attributes for shrinking
    public static GameObject selectedObject;
    public static bool isSelected = false;
    public Material selectedMaterial;
    private Material beforeSelectedMaterial;

    // Hack by Om: To prevent clash with controls
    // Controller input clash prevention
    [SerializeField] double clashPreventionThreshold = 0.002f;

    void StartVibration()
    {
        float vibrationIntensity = 0.5f;

        // Trigger vibration on highlight
        OVRInput.SetControllerVibration(vibrationIntensity, vibrationIntensity, OVRInput.Controller.LTouch);
        OVRInput.SetControllerVibration(vibrationIntensity, vibrationIntensity, OVRInput.Controller.RTouch);
        Invoke("StopVibration", 0.01f);
    }

    void StopVibration()
    {
        // Stop haptic feedback on both controllers
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
    }

    void HighlightObject(GameObject obj)
    {
        // Store the original material of the object
        // Hack by Om
        if (selectedObject != obj)
        {
            previousHighlightMaterial = obj.GetComponent<Renderer>().material;
            obj.GetComponent<Renderer>().material = highlightMaterial;
        }   
        StartVibration();
    }

    void UnhighlightObject(GameObject obj)
    {
        // Revert the object's material to the original one
        // Hack by Om
        if (selectedObject != obj)
            obj.GetComponent<Renderer>().material = previousHighlightMaterial;
    }

    void GrabObject()
    {
        // Disable physics interactions while the object is grabbed
        grabbedObject = hitObject;
        if (hitObject.GetComponent<Rigidbody>() != null) grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
        initialTransformParent = grabbedObject.transform.parent;
        grabbedObject.transform.SetParent(transform);
    }

    void ReleaseObject()
    {
        // Enable physics interactions and release the object
        if (hitObject.GetComponent<Rigidbody>() != null) grabbedObject.GetComponent<Rigidbody>().isKinematic = false;
        grabbedObject.transform.SetParent(initialTransformParent);
        grabbedObject = null;
        hitObject = null;
    }

    void UpdateGrabbedObject()
    {
        Vector3 toBePosition = transform.position + transform.forward * 1f;
        Vector3 moveDirection = (toBePosition - grabbedObject.transform.position).normalized;

        float moveAmount = 3.5f * Time.deltaTime;
        isObjectClose |= Vector3.Distance(toBePosition, grabbedObject.transform.position) <= 0.5f;

        // Update the position and rotation of the grabbed object to match the controller
        if (isObjectClose) grabbedObject.transform.position = transform.position + transform.forward * 1f;
        else grabbedObject.transform.position += moveDirection * moveAmount;
        grabbedObject.transform.rotation = transform.rotation;
    }

    void SelectObject(GameObject obj)
    {
        // Store the selected object
        selectedObject = obj;
        isSelected = true;
        beforeSelectedMaterial = previousHighlightMaterial;
        obj.GetComponent<Renderer>().material = selectedMaterial;
        StartVibration();
    }

    public void UnselectObject(GameObject obj)
    {
        // Unstore the selected object
        selectedObject = null;
        isSelected = false;
        obj.GetComponent<Renderer>().material = beforeSelectedMaterial;
    }

    // Start is called before the first frame update
    void Start()
    {
        defaultLineMat = lineRenderer.material;
    }

    // Update is called once per frame
    void Update()
    {
        // Default line color when pointing at uninteractive surfaces
        lineRenderer.material = defaultLineMat;

        // Hack by Om: Changed the commented (original) line 
        //if (OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > 0.5 && OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.5)
        if (OVRInput.Get(OVRInput.RawButton.RHandTrigger) && OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) < clashPreventionThreshold)
        {
            // Play the sound effect if it's not already playing
            //if (!audioSource.isPlaying) audioSource.Play();

            // If an object is grabbed, update its position and rotation
            if (grabbedObject != null) UpdateGrabbedObject();
            else if (hitObject != null) GrabObject();

            lineRenderer.enabled = false;
        }
        // HACK by Om: Changed to GetDown instead, as we want a "toggle-style" selection control
        else if (OVRInput.GetDown(OVRInput.RawButton.A) && hitObject != null)
        {
            // HACK by Om: selectedObject != null -------> selectedObject == null
            if (selectedObject == null) SelectObject(hitObject);
            else UnselectObject(selectedObject);

            lineRenderer.enabled = false;
        }
        else
        {
            // Stop the sound effect if the button is released
            //if (audioSource.isPlaying)
            //{
            //    audioSource.Stop();
            //}
            isObjectClose = false;
            if (grabbedObject != null) ReleaseObject();

            // Perform raycasting to detect objects the controller is hovering over
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance))
            {
                // Update the positions of the LineRenderer to match the ray
                lineRenderer.enabled = true;
                lineRenderer.SetPosition(0, transform.position); // Start position
                lineRenderer.SetPosition(1, hit.point); // End position (where the ray hits)

                if (hit.collider.gameObject.tag == "Grabbable")
                {
                    hitObject = hit.collider.gameObject;
                    lineRenderer.material = grabbableLineMat;

                    // Check if the hit object is different from the previously highlighted object
                    if (hitObject != previousHitObject)
                    {
                        // Unhighlight the previous object if it exists
                        if (previousHitObject != null) UnhighlightObject(previousHitObject);

                        // Highlight the new hit object
                        HighlightObject(hitObject);

                        // Update the previously highlighted object
                        previousHitObject = hitObject;
                    }
                }
                else
                {
                    if (previousHitObject != null)
                    {
                        UnhighlightObject(previousHitObject);
                        previousHitObject = null;
                    }
                    grabbedObject = null;
                    hitObject = null;
                }
            }
            else
            {
                // If no object is hit, unhighlight the previous object if it exists
                if (previousHitObject != null)
                {
                    UnhighlightObject(previousHitObject);
                    previousHitObject = null;
                }
                lineRenderer.enabled = false;
                grabbedObject = null;
                hitObject = null;
            }
        }
    }
}
