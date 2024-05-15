using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectHover : MonoBehaviour
{
    public Material highlightMaterial; // Material for highlighting
    // Hack: I added this
    public Material selectMaterial; // Material for selecting
    public LineRenderer lineRenderer;
    public float maxDistance = 10f; // Maximum distance for the raycast
    public Material defaultLineMat;
    public AudioSource audioSource;

    private Transform controllerTransform;
    private GameObject previousHitObject; // Previously highlighted object
    private Material previousMaterial; // Material of the previously highlighted object

    // Hack: I changed this and added isSelected
    public static GameObject grabbedObject;
    // Hack: I changed this and added isSelected
    public static bool isSelected = false;

    private Rigidbody grabbedRigidbody;
    private bool isRaycastHit = false;
    private bool isObjectClose = false;
    public Material redLineMat; // Indicates a red line when pointing at grabbable surface

    // Hack: I added these
    // Teleportation variables
    public Material blueLineMat; // Indicates a blue line when pointing at teleportable surface
    bool aboutToTeleport = false;
    private Vector3 teleportTarget = new Vector3();
    public OVRPlayerController player;
    public GameObject teleportTargetPointer;
    private Renderer teleportTargetPointerRenderer;

    // Hack: I added this
    // Shrinking spell variables
    public static GameObject selectedObject;

    void StartVibration()
    {

        float vibrationIntensity = 0.5f;
        // Trigger vibration on highlight
        OVRInput.SetControllerVibration(vibrationIntensity, vibrationIntensity, OVRInput.Controller.LTouch);
        OVRInput.SetControllerVibration(vibrationIntensity, vibrationIntensity, OVRInput.Controller.RTouch);

        // Stop the vibration after the specified duration
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
        previousMaterial = obj.GetComponent<Renderer>().material;

        // Apply the highlight material to the object
        obj.GetComponent<Renderer>().material = highlightMaterial;
        StartVibration();
    }

    void UnhighlightObject(GameObject obj)
    {
        // Revert the object's material to the original one
        obj.GetComponent<Renderer>().material = previousMaterial;
    }

    // Hack: I added this
    void SelectObject(GameObject obj)
    {
        // Store the selected object
        selectedObject = obj;
        isSelected = true;

        // Store the original material of the object
        previousMaterial = obj.GetComponent<Renderer>().material;

        // Apply the highlight material to the object
        obj.GetComponent<Renderer>().material = selectMaterial;
        StartVibration();
    }

    // Hack: I added this
    public void UnselectObject(GameObject obj)
    {
        // Revert the object's material to the original one
        obj.GetComponent<Renderer>().material = previousMaterial;

        // Unstore the selected object
        selectedObject = null;
        isSelected = false;
    }

    void GrabObject()
    {
        // Disable physics interactions while the object is grabbed
        grabbedRigidbody.isKinematic = true;
        grabbedObject.transform.SetParent(controllerTransform);
    }

    void ReleaseObject()
    {
        // Enable physics interactions and release the object
        grabbedRigidbody.isKinematic = false;
        grabbedObject.transform.SetParent(null);
        grabbedObject = null;
        grabbedRigidbody = null;
    }

    void UpdateGrabbedObject()
    {
        Vector3 toBePosition = controllerTransform.position + controllerTransform.forward * 1.5f;
        Vector3 moveDirection = (toBePosition - grabbedObject.transform.position).normalized;

        float moveAmount = 5f * Time.deltaTime;
        isObjectClose |= Vector3.Distance(toBePosition, grabbedObject.transform.position) <= 0.45f;

        // Update the position and rotation of the grabbed object to match the controller
        if (isObjectClose) grabbedObject.transform.position = controllerTransform.position + controllerTransform.forward * 1.5f;
        else grabbedObject.transform.position += moveDirection * moveAmount;
        grabbedObject.transform.rotation = controllerTransform.rotation;
    }

    // Start is called before the first frame update
    void Start()
    {
        controllerTransform = GameObject.Find("RightControllerAnchor").transform;

        // Initialize the teleportTargetPointer to be invisible
        if (teleportTargetPointer)
        {
            teleportTargetPointerRenderer = teleportTargetPointer.GetComponent<Renderer>();
            teleportTargetPointerRenderer.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Default line color when pointing at uninteractive surfaces
        lineRenderer.material = defaultLineMat;

        if (OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > 0.5 && OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.5)
        {
            // Play the sound effect if it's not already playing
            //if (!audioSource.isPlaying)
            //{
            //    audioSource.Play();
            //}


            // If an object is grabbed, update its position and rotation
            // Hack: I added this null check
            if (grabbedObject != null)
            {
                UpdateGrabbedObject();
            }
            else
            {
                // Grab the object if it has a Rigidbody component
                if (grabbedObject)
                {
                    grabbedRigidbody = grabbedObject.GetComponent<Rigidbody>();
                }

                if (grabbedRigidbody != null)
                {
                    GrabObject();
                }
            }
            lineRenderer.enabled = false;
        }
        // Hack: I added this extra input check
        else if (OVRInput.Get(OVRInput.RawButton.A))
        {

            // Toggle selection
            if (selectedObject != null)
            {
                SelectObject(grabbedObject);
            } else
            {
                UnselectObject(selectedObject);
            }
        }
        else
        {
            // Stop the sound effect if the button is released
            //if (audioSource.isPlaying)
            //{
            //    audioSource.Stop();
            //}

            isObjectClose = false;

            if (grabbedRigidbody != null) ReleaseObject();

            // Perform raycasting to detect objects the controller is hovering over
            RaycastHit hit;
            Ray ray = new Ray(controllerTransform.position, controllerTransform.forward);
            isRaycastHit = Physics.Raycast(controllerTransform.position, controllerTransform.forward, out hit, maxDistance);
            if (isRaycastHit)
            {
                // Update the positions of the LineRenderer to match the ray
                lineRenderer.enabled = true;
                lineRenderer.SetPosition(0, controllerTransform.position); // Start position
                lineRenderer.SetPosition(1, hit.point); // End position (where the ray hits)

                // Hack: I added this tag check
                //if (hit.transform.name != "Terrain") { <--------------- Not sure why you refer to hit.transform?
                if (hit.collider.gameObject.tag == "Grabbable")
                {
                    GameObject hitObject = hit.collider.gameObject;
                    grabbedObject = hitObject;

                    lineRenderer.material = redLineMat;

                    // Check if the hit object is different from the previously highlighted object
                    if (hitObject != previousHitObject)
                    {
                        // Unhighlight the previous object if it exists
                        if (previousHitObject != null)
                        {
                            UnhighlightObject(previousHitObject);
                        }

                        // Highlight the new hit object
                        HighlightObject(hitObject);

                        // Update the previously highlighted object
                        previousHitObject = hitObject;
                    }
                }
                // Hack: I added this tag check
                else if (hit.collider.gameObject.tag == "Teleportable")
                {
                    if (previousHitObject != null)
                    {
                        UnhighlightObject(previousHitObject);
                        previousHitObject = null;
                    }
                    grabbedObject = null;

                    // Hack: I added these
                    // Here we assume ray hits a teleportable surface
                    lineRenderer.material = blueLineMat;

                    if (OVRInput.Get(OVRInput.RawButton.B))
                    {
                        aboutToTeleport = true;

                        teleportTarget = hit.point;

                        // Teleport marker to let player better know where they are aiming to teleport
                        teleportTargetPointerRenderer.enabled = true;
                        teleportTargetPointer.transform.position = hit.point;
                    }
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
            }
        }

        // Hack: I added these
        if (OVRInput.GetUp(OVRInput.RawButton.B) && aboutToTeleport == true)
        {
            aboutToTeleport = false;
            var character_controller = player.GetComponent<CharacterController>();
            character_controller.Move(teleportTarget - controllerTransform.position);
            teleportTargetPointerRenderer.enabled = false;
        }
    }
}
