using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;
using UnityEngine.InputSystem;


public class SwingAndSlice : MonoBehaviour
{
    [SerializeField]
    private AudioClip sliceSoundClip; // 私有字段，但通过 SerializeField 属性在 Inspector 中显示
    public float volume = 1.0f;   
    public Transform startSlicePoint;
    public Transform endSlicePoint;
    public VelocityEstimator velocityEstimator;
    public LayerMask sliceableLayer;

    public Material crossSectionMaterial;
    public float cutForce = 0;

    [SerializeField]
    private float vibrationIntensity = 1.0f;

    [SerializeField]
    private float vibrationDuration = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //DrawLine(startSlicePoint.position, endSlicePoint.position, Color.red, 2.0f);
        bool hasHit = Physics.Linecast(startSlicePoint.position, endSlicePoint.position, out RaycastHit hit, sliceableLayer);
        //Debug.Log("Linecast hit: " + hasHit);
        //Debug.Log("LayerMask Value: " + sliceableLayer.value);

        if (hasHit)
        {
            GameObject target = hit.transform.gameObject;
            Slice(target);
        }
        if (startSlicePoint == null || endSlicePoint == null)
        {
            Debug.LogError("StartSlicePoint or EndSlicePoint is not set!");
            return;
        }
    }

    public void Slice(GameObject target)
    {
        Vector3 velocity = velocityEstimator.GetVelocityEstimate();
        Vector3 planeNormal = Vector3.Cross(endSlicePoint.position - startSlicePoint.position, velocity);
        planeNormal.Normalize();

        SlicedHull hull = target.Slice(endSlicePoint.position, planeNormal);

        if(hull != null)
        {   
            AudioSource.PlayClipAtPoint(sliceSoundClip, target.transform.position);
            GrabbableObject grabbable = this.GetComponentInChildren<GrabbableObject>(true);
            // Debug.Log("Name: " + grabbable.name); // 打印物体的名称
            // Debug.Log("Is Grabbed: " + grabbable.isGrabbed); // 打印物体是否被抓取的状态
            // if (grabbable != null && grabbable.isGrabbed)
            // {
            //     
            //     GrabberController grabber = grabbable.grabbedBy;
            //     if (grabber != null)
            //     {
            //         // 这里，我们需要知道哪个手柄正在进行抓取，然后对那个手柄触发振动
            //         // OVRInput.Controller 对应的是当前抓取物体的控制器
            //         OVRInput.Controller controller = grabber.Controller;
            //         StartCoroutine(VibrateController(vibrationDuration, vibrationIntensity, controller));
            //     }
            // }

            // if (grabbable != null)
            // {
            //     Debug.Log("GrabbableObject found.");
            //     if (grabbable.isGrabbed)
            //     {
            //         Debug.Log("Object is currently grabbed.");
            //         GrabberController grabber = grabbable.grabbedBy;
            //         if (grabber != null)
            //         {
            //             Debug.Log("GrabberController is not null.");
            //             OVRInput.Controller controller = grabber.Controller;
            //             StartCoroutine(VibrateController(vibrationDuration, vibrationIntensity, controller));
            //         }
            //         else
            //         {
            //             Debug.Log("GrabberController is null.");
            //         }
            //     }
            //     else
            //     {
            //         Debug.Log("Object is not grabbed.");
            //     }
            // }
            // else
            // {
            //     Debug.Log("No GrabbableObject component found.");
            // }

            Debug.Log("Continue slice" );
            GameObject upperHull = hull.CreateUpperHull(target, crossSectionMaterial);
            SetupSlicedComponent(upperHull);
            upperHull.layer = target.layer;
            upperHull.gameObject.tag = "Grabbable";

            GameObject lowerHull = hull.CreateLowerHull(target, crossSectionMaterial);
            SetupSlicedComponent(lowerHull);
            lowerHull.layer = target.layer;
            lowerHull.gameObject.tag = "Grabbable";

            StartCoroutine(VibrateController(vibrationDuration, vibrationIntensity, OVRInput.Controller.RTouch));

            Destroy(target);
        }
        else{
            Debug.Log("Slice failed to create hull for object: " + target.name);
        }
    }

    public void SetupSlicedComponent(GameObject slicedObject)
    {
        Rigidbody rb = slicedObject.AddComponent<Rigidbody>();
        MeshCollider collider = slicedObject.AddComponent<MeshCollider>();
        collider.convex = true;
        rb.AddExplosionForce(cutForce, slicedObject.transform.position, 1);
    }

    private IEnumerator VibrateController(float duration, float intensity, OVRInput.Controller controller)
    {   
        Debug.Log($"Starting vibration on {controller} with intensity {intensity}");
        OVRInput.SetControllerVibration(intensity, intensity, controller);
        yield return new WaitForSeconds(duration);
        OVRInput.SetControllerVibration(0, 0, controller);
        Debug.Log($"Stopping vibration on {controller}");
    }
}
