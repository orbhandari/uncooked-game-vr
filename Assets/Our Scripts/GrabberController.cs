using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrabberController : MonoBehaviour
{
    public float grabBegin = 0.5f;
    public float grabEnd = 0.5f;

    [SerializeField]
    protected Transform m_gripTransform = null;

    [SerializeField]
    protected Collider[] m_grabVolumes = null;

    [SerializeField]
    protected OVRInput.Controller m_controller;

    [SerializeField]
    protected Transform m_parentTransform;

    [SerializeField]
    protected GameObject m_player;

    [SerializeField] 
    protected Material highlightMaterial; // 高亮显示的材质

    protected Vector3 m_lastPos;
    protected Quaternion m_lastRot;
    protected Quaternion m_anchorOffsetRotation;
    protected Vector3 m_anchorOffsetPosition;
    protected float m_prevFlex;
    protected GrabbableObject m_grabbedObj = null;
    protected Dictionary<GrabbableObject, int> m_grabCandidates = new Dictionary<GrabbableObject, int>();

    public GrabbableObject grabbedObject
    {
        get { return m_grabbedObj; }
    }

    protected virtual void Awake()
    {
        m_anchorOffsetPosition = transform.localPosition;
        m_anchorOffsetRotation = transform.localRotation;
    }

    protected virtual void Start()
    {
        m_lastPos = transform.position;
        m_lastRot = transform.rotation;
        if (m_parentTransform == null)
        {
            m_parentTransform = gameObject.transform;
        }
        SetPlayerIgnoreCollision(gameObject, true);
    }

    virtual public void Update()
    {
        Vector3 destPos = m_parentTransform.TransformPoint(m_anchorOffsetPosition);
        Quaternion destRot = m_parentTransform.rotation * m_anchorOffsetRotation;
        GetComponent<Rigidbody>().MovePosition(destPos);
        GetComponent<Rigidbody>().MoveRotation(destRot);

        m_lastPos = transform.position;
        m_lastRot = transform.rotation;

        float prevFlex = m_prevFlex;
        float primaryHandValue = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, m_controller);
        float primaryIndexValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, m_controller);

        //Debug.Log($"Primary Trigger: {primaryHandValue}, Secondary Trigger: {primaryIndexValue}");

        bool areBothTriggersPressed = primaryHandValue > 0.5f && primaryIndexValue > 0.5f;
        m_prevFlex = areBothTriggersPressed ? 1.0f : 0.0f; 

        // float prevFlex = m_prevFlex;
        // m_prevFlex = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, m_controller);
        // Debug.Log($"Primary Trigger: {m_prevFlex}");

        // Debug.Log($"Are Both Triggers Pressed: {areBothTriggersPressed}, m_prevFlex: {m_prevFlex}");

        CheckForGrabOrRelease(prevFlex);
    }

    void OnTriggerEnter(Collider otherCollider)
    {
        GrabbableObject grabbable = otherCollider.GetComponent<GrabbableObject>() ??
                                 otherCollider.GetComponentInParent<GrabbableObject>();
        if (grabbable == null) return;

        int refCount = 0;
        m_grabCandidates.TryGetValue(grabbable, out refCount);
        m_grabCandidates[grabbable] = refCount + 1;
    }

    void OnTriggerExit(Collider otherCollider)
    {
        GrabbableObject grabbable = otherCollider.GetComponent<GrabbableObject>() ??
                                 otherCollider.GetComponentInParent<GrabbableObject>();
        if (grabbable == null) return;

        int refCount = 0;
        if (m_grabCandidates.TryGetValue(grabbable, out refCount) && refCount > 1)
        {
            m_grabCandidates[grabbable] = refCount - 1;
        }
        else
        {
            m_grabCandidates.Remove(grabbable);
        }
    }

    protected void CheckForGrabOrRelease(float prevFlex)
    {
        if ((m_prevFlex >= grabBegin) && (prevFlex < grabBegin))
        {
            GrabBegin();
        }
        else if ((m_prevFlex <= grabEnd) && (prevFlex > grabEnd))
        {
            GrabEnd();
        }
    }

    protected virtual void GrabBegin()
    {
        float closestMagSq = float.MaxValue;
        GrabbableObject closestGrabbable = null;
        Collider closestGrabbableCollider = null;

        foreach (GrabbableObject grabbable in m_grabCandidates.Keys)
        {
            if (grabbable.isGrabbed && !grabbable.allowOffhandGrab)
                continue;

            foreach (Collider grabbableCollider in grabbable.grabPoints)
            {
                Vector3 closestPointOnBounds = grabbableCollider.ClosestPointOnBounds(m_gripTransform.position);
                float grabbableMagSq = (m_gripTransform.position - closestPointOnBounds).sqrMagnitude;
                if (grabbableMagSq < closestMagSq)
                {
                    closestMagSq = grabbableMagSq;
                    closestGrabbable = grabbable;
                    closestGrabbableCollider = grabbableCollider;
                }
            }
        }

        if (closestGrabbable != null)
        {
            if (closestGrabbable.isGrabbed)
            {
                closestGrabbable.grabbedBy.OffhandGrabbed(closestGrabbable);
            }

            m_grabbedObj = closestGrabbable;
            m_grabbedObj.GrabBegin(this, closestGrabbableCollider);
            m_grabbedObj.transform.SetParent(transform);
            HighlightObject(m_grabbedObj);
        }
    }

    protected virtual void GrabEnd()
    {
        if (m_grabbedObj != null)
        {
            m_grabbedObj.GrabEnd(Vector3.zero, Vector3.zero); // Assuming zero velocity for simplicity
            m_grabbedObj.transform.SetParent(null);
            UnhighlightObject(m_grabbedObj);
            m_grabbedObj = null;
            Debug.Log("GrabEnd: Rigidbody isKinematic = " + gameObject.GetComponent<Rigidbody>().isKinematic);
        }
    }

    protected void SetPlayerIgnoreCollision(GameObject grabbable, bool ignore)
    {
        if (m_player != null)
        {
            Collider[] playerColliders = m_player.GetComponentsInChildren<Collider>();
            foreach (Collider pc in playerColliders)
            {
                Collider[] colliders = grabbable.GetComponentsInChildren<Collider>();
                foreach (Collider c in colliders)
                {
                    Physics.IgnoreCollision(c, pc, ignore);
                }
            }
        }
    }

    protected virtual void OffhandGrabbed(GrabbableObject grabbable)
    {
        if (m_grabbedObj == grabbable)
        {
            GrabEnd();
        }
    }

    public void ForceRelease(GrabbableObject grabbable)
    {
        bool canRelease = (
            (m_grabbedObj != null) &&
            (m_grabbedObj == grabbable)
        );
        if (canRelease)
        {
            GrabEnd();
        }
    }

    public OVRInput.Controller Controller {
        get {  return m_controller; }
    }

    private void HighlightObject(GrabbableObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            obj.originalMaterial = renderer.material;
            renderer.material = highlightMaterial;
        }
    }

    private void UnhighlightObject(GrabbableObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null && obj.originalMaterial != null)
        {
            renderer.material = obj.originalMaterial;
        }
    }

}