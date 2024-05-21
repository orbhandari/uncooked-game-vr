using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class RayInteracter : MonoBehaviour
{
    // Attributes for ray pointer
    [Header("Ray Properties")]
    public LineRenderer lineRenderer;
    private Material defaultLineMat;
    public float maxDistance = 10f; // Maximum distance for the raycast

    private bool isSelecting = false;

    public TransitionManager transitionManager;


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

    // Start is called before the first frame update
    void Start()
    {
        defaultLineMat = lineRenderer.material;
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

            if (hit.collider.gameObject.tag == "StartTutorial")
            {
                if (!isSelecting)
                {
                    StartVibration();
                    isSelecting = true;
                }

                if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
                {
                    // Load scene
                    transitionManager.GoToSceneAsync(2);
                }
            }
            else if (hit.collider.gameObject.tag == "StartGame")
            {
                if (!isSelecting)
                {
                    StartVibration();
                    isSelecting = true;
                }

                if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
                {
                    // Load scene
                    transitionManager.GoToSceneAsync(1);
                }
            } else
            {
                isSelecting = false;
            }
        }
        else
        {
            lineRenderer.enabled = false;
            isSelecting = false;
        }
    }
}
