using UnityEngine;
using UnityEngine.InputSystem;
using EzySlice;

public class Slice : MonoBehaviour
{
    public InputActionReference leftHandGrabAction;
    public InputActionReference rightHandGrabAction;
    public GameObject objectToSlice;
    public Material crossSectionMaterial;
    public float sliceDistanceThreshold = 0.1f;
    private bool isLeftHandGrabbing = false;
    private bool isRightHandGrabbing = false;
    private bool isReadyToSlice = false;
    public Transform leftHandTransform;
    public Transform rightHandTransform;

    private void Update()
    {
        if (isLeftHandGrabbing && isRightHandGrabbing)
        {
            float distance = Vector3.Distance(leftHandTransform.position, rightHandTransform.position);
            if (distance > sliceDistanceThreshold && !isReadyToSlice)
            {
                PerformSlice();
                isReadyToSlice = true;
            }
        }
    }

    private void OnEnable()
    {
        leftHandGrabAction.action.Enable();
        rightHandGrabAction.action.Enable();

        leftHandGrabAction.action.performed += OnLeftHandGrab;
        leftHandGrabAction.action.canceled += OnLeftHandRelease;

        rightHandGrabAction.action.performed += OnRightHandGrab;
        rightHandGrabAction.action.canceled += OnRightHandRelease;
    }

    private void OnDisable()
    {
        leftHandGrabAction.action.Disable();
        rightHandGrabAction.action.Disable();

        leftHandGrabAction.action.performed -= OnLeftHandGrab;
        leftHandGrabAction.action.canceled -= OnLeftHandRelease;

        rightHandGrabAction.action.performed -= OnRightHandGrab;
        rightHandGrabAction.action.canceled -= OnRightHandRelease;
    }

    private void OnLeftHandGrab(InputAction.CallbackContext context)
    {
        isLeftHandGrabbing = true;
        isReadyToSlice = false;
    }

    private void OnRightHandGrab(InputAction.CallbackContext context)
    {
        isRightHandGrabbing = true;
        isReadyToSlice = false;
    }

    private void OnLeftHandRelease(InputAction.CallbackContext context)
    {
        isLeftHandGrabbing = false;
    }

    private void OnRightHandRelease(InputAction.CallbackContext context)
    {
        isRightHandGrabbing = false;
    }


    private void CheckBothHandsGrabbing()
    {
        if (isLeftHandGrabbing && isRightHandGrabbing)
        {
            Debug.Log("Both hands are grabbing the object!");
            PerformSlice();
        }
    }

    private void PerformSlice()
    {
        if (leftHandTransform == null || rightHandTransform == null)
        {
            Debug.LogError("Hand transforms are not set.");
            return;
        }

        Vector3 midpoint = (leftHandTransform.position + rightHandTransform.position) / 2;
        Vector3 directionBetweenHands = (rightHandTransform.position - leftHandTransform.position).normalized;
        Vector3 planeNormal = Vector3.Cross(directionBetweenHands, Vector3.up).normalized;

        SlicedHull slicedObject = objectToSlice.Slice(midpoint, planeNormal, crossSectionMaterial);

        if (slicedObject != null)
        {
            GameObject upperHull = slicedObject.CreateUpperHull(objectToSlice, crossSectionMaterial);
            GameObject lowerHull = slicedObject.CreateLowerHull(objectToSlice, crossSectionMaterial);

            if (upperHull != null)
            {

                upperHull.AddComponent<Rigidbody>().isKinematic = false;
                upperHull.AddComponent<MeshCollider>().convex = true;
                upperHull.transform.position = objectToSlice.transform.position;

                upperHull.GetComponent<Rigidbody>().AddForce(planeNormal * 0f, ForceMode.Impulse);
            }

            if (lowerHull != null)
            {
                lowerHull.AddComponent<Rigidbody>().isKinematic = false;
                lowerHull.AddComponent<MeshCollider>().convex = true;
                lowerHull.transform.position = objectToSlice.transform.position;

                lowerHull.GetComponent<Rigidbody>().AddForce(-planeNormal * 0f, ForceMode.Impulse);
            }

            Destroy(objectToSlice);
        }
    }
}