using System;
using UnityEngine;

public class GrabbableObject : MonoBehaviour
{
    [SerializeField]
    protected bool m_allowOffhandGrab = true;
    private int test = 0;

    [SerializeField]
    protected Collider[] m_grabPoints = null;

    protected bool m_grabbedKinematic = false;
    protected Collider m_grabbedCollider = null;
    protected GrabberController m_grabbedBy = null;

    public Material originalMaterial; // 用于存储原始材质

    /// <summary>
    /// If true, the object can currently be grabbed.
    /// </summary>
    public bool allowOffhandGrab
    {
        get { return m_allowOffhandGrab; }
    }

    public bool isGrabbed
    {
        get { return m_grabbedBy != null; }
    }

    public GrabberController grabbedBy
    {
        get { return m_grabbedBy; }
    }
    public Transform grabbedTransform
    {
        get { return m_grabbedCollider.transform; }
    }

    public Rigidbody grabbedRigidbody
    {
        get { return m_grabbedCollider.attachedRigidbody; }
    }
    public Collider[] grabPoints
    {
        get { return m_grabPoints; }
    }


    virtual public void GrabBegin(GrabberController hand, Collider grabPoint)
    {
        m_grabbedBy = hand;
        m_grabbedCollider = grabPoint;
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
    }

    /// <summary>
    /// Notifies the object that it has been released.
    /// </summary>
    virtual public void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        rb.isKinematic = m_grabbedKinematic;
        //rb.velocity = linearVelocity;
        //rb.angularVelocity = angularVelocity;
        m_grabbedBy = null;
        m_grabbedCollider = null;
    }

    void Awake()
    {
        if (m_grabPoints.Length == 0)
        {
            // Get the collider from the grabbable
            Collider collider = this.GetComponent<Collider>();
            if (collider == null)
            {
                throw new ArgumentException("Grabbables cannot have zero grab points and no collider " +
                                            "-- please add a grab point or collider.");
            }

            // Create a default grab point
            m_grabPoints = new Collider[1] { collider };

            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                originalMaterial = renderer.material; // 存储初始材质
            }
        }
    }

    protected virtual void Start()
    {
        m_grabbedKinematic = GetComponent<Rigidbody>().isKinematic;
    }

    void OnDestroy()
    {
        if (m_grabbedBy != null)
        {
            // Notify the hand to release destroyed grabbables
            m_grabbedBy.ForceRelease(this);
        }
    }
}
