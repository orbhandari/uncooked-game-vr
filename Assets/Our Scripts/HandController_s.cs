using UnityEngine;
using UnityEngine.InputSystem;

public class HandController_s : MonoBehaviour
{

	// Store the hand type to know which button should be pressed
	public enum HandType : int { LeftHand, RightHand };
	[Header("Hand Properties")]
	public HandType handType;


	//change into public
	public ObjectAnchor_s objectGrasped;

	public Vector3 initialGrabPosition;
	private bool isGrabbing;

	public bool IsHoldingObject()
	{
		// Check if the hand is currently holding an object
		return objectGrasped != null;
	}

	public ObjectAnchor_s GetGrabbedObject()
	{
		// Return the ObjectAnchor instance of the object being currently grabbed
		return objectGrasped;
	}

	public Vector3 GetGrabbedObjectPosition()
	{
		// Return the position of the grabbed object, if any
		return objectGrasped != null ? objectGrasped.transform.position : Vector3.zero;
	}


	// Store the player controller to forward it to the object
	[Header("Player Controller")]
	public MainPlayerController playerController;



	// Store all gameobjects containing an Anchor
	// N.B. This list is static as it is the same list for all hands controller
	// thus there is no need to duplicate it for each instance
	static protected ObjectAnchor_s[] anchors_in_the_scene;


	public static void RefreshAnchorsInScene()
	{
		anchors_in_the_scene = GameObject.FindObjectsOfType<ObjectAnchor_s>();
		Debug.LogWarning(anchors_in_the_scene);
	}


	void Start()
	{
		// Prevent multiple fetch
		if (anchors_in_the_scene == null) anchors_in_the_scene = GameObject.FindObjectsOfType<ObjectAnchor_s>();
		Debug.LogWarning(anchors_in_the_scene);
	}



	//change the logic of hand close
	// This method checks that the hand is closed depending on the hand side
	protected bool is_hand_closed()
	{
		// Case of a left hand
		if (handType == HandType.LeftHand) return
		 // OVRInput.Get(OVRInput.Button.Three)                           // Check that the A button is pressed
		 // && OVRInput.Get(OVRInput.Button.Four)                         // Check that the B button is pressed
		   OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) > 0.5     // Check that the middle finger is pressing
		  && OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > 0.5;   // Check that the index finger is pressing


		// Case of a right hand
		else return
			// OVRInput.Get(OVRInput.Button.One)                             // Check that the A button is pressed
			// && OVRInput.Get(OVRInput.Button.Two)                          // Check that the B button is pressed
			 OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > 0.5   // Check that the middle finger is pressing
			&& OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.5; // Check that the index finger is pressing
	}


	// Automatically called at each frame
	void Update() { handle_controller_behavior(); }


	// Store the previous state of triggers to detect edges
	protected bool is_hand_closed_previous_frame = false;

	// Store the object atached to this hand
	// N.B. This can be extended by using a list to attach several objects at the same time
	protected ObjectAnchor_s object_grasped = null;

	/// <summary>
	/// This method handles the linking of object anchors to this hand controller
	/// </summary>
	///


	//change the logic of grab, hands should be collided with the object rather than distant grab.
	protected void handle_controller_behavior()
	{
		bool handClosed = is_hand_closed();
		//Debug.Log("Hand Closed State: " + handClosed);
		if (handClosed != is_hand_closed_previous_frame)
		{
			Debug.Log("Hand state changed");
			is_hand_closed_previous_frame = handClosed;
			if (handClosed)
			{
				Debug.Log("Attempting to grab object");
				AttemptToGrabObject();
			}
			else
			{
				Debug.Log("Attempting to release object");
				ReleaseObject();
			}
		}
	}


	private void AttemptToGrabObject()
	{
		float closestDistance = Mathf.Infinity;
		ObjectAnchor_s closestAnchor = null;

		foreach (var anchor in anchors_in_the_scene)
		{
			if (!anchor.is_available())
				continue;

			float distanceToSurface = Vector3.Distance(transform.position, anchor.transform.position) - anchor.get_grasping_radius();
			if (distanceToSurface <= 0.05f && distanceToSurface < closestDistance)
			{
				closestAnchor = anchor;
				closestDistance = distanceToSurface;
			}
		}

		if (closestAnchor != null && closestAnchor.is_available())
		{
			Debug.Log("Grasping object: " + closestAnchor.name);
			closestAnchor.attach_to(this);
			objectGrasped = closestAnchor;
			initialGrabPosition = transform.position;
			isGrabbing = true;
		}
	}

	public void ReleaseObject()
	{
		if (objectGrasped != null)
		{
			Debug.Log("Releasing object: " + objectGrasped.name);
			objectGrasped.detach_from(this);
			objectGrasped = null;
			isGrabbing = false;
		}

	}

}
