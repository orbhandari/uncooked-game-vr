using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Assigned to L Hand controller to not conflict with distance grab
public class Teleport : MonoBehaviour
{
    // Attributes for ray pointer
    [Header( "Ray Properties" )]
    public LineRenderer lineRenderer;
    private Material defaultLineMat;
    public Material teleportableLineMat; // Indicates the material used when pointing at a teleportable surface
    public float maxDistance = 10f; // Maximum distance for the raycast

    [Header( "Others" )]
    public OVRPlayerController player;
    public GameObject teleportTargetPointer;

    private Renderer teleportTargetPointerRenderer;
    private Vector3 teleportTarget = new Vector3();
    private bool aboutToTeleport = false;

    // Start is called before the first frame update
    void Start()
    {
        defaultLineMat = lineRenderer.material;
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
        lineRenderer.material = defaultLineMat;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance))
        {
            // Update the positions of the LineRenderer to match the ray
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, transform.position); // Start position
            lineRenderer.SetPosition(1, hit.point); // End position (where the ray hits)

            if (hit.collider.gameObject.tag == "Teleportable")
            {
                // Here we assume ray hits a teleportable surface
                lineRenderer.material = teleportableLineMat;

                if (OVRInput.Get(OVRInput.RawButton.Y))
                {
                    aboutToTeleport = true;
                    teleportTarget = hit.point;

                    // Teleport marker to let player better know where they are aiming to teleport
                    teleportTargetPointerRenderer.enabled = true;
                    teleportTargetPointer.transform.position = hit.point;
                }
            }
        }
        else lineRenderer.enabled = false;

        if (OVRInput.GetUp(OVRInput.RawButton.Y) && aboutToTeleport)
        {
            aboutToTeleport = false;
            player.GetComponent<CharacterController>().Move(teleportTarget - transform.position);
            teleportTargetPointerRenderer.enabled = false;
        }
    }
}
