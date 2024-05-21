using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using PDollarGestureRecognizer;

// Assigned to R Hand controller
// CONTROLS: Uses *only* R Index Trigger to not conflict with grabs
public class SpellCasting : MonoBehaviour
{
    enum Spell
    {
        FIRE,
        SHRINK,
        ENLARGE,
        UNKNOWN
    };

    // Controller input clash prevention
    [SerializeField] double clashPreventionThreshold = 0.002f;

    // For drawing the spell shape
    private bool hasStartedSpell = false;

    // Keep track of positions during spell drawing
    private List<Vector3> positionsTraced = new List<Vector3>();
    [SerializeField] float movementTrackingPrecision = 0.05f;

    // Spell drawing effect
    [SerializeField] GameObject trailCube;
    [SerializeField] GameObject cubePrefab;
    private GameObject trailPointer;

    // PCloud recognizer
    private List<Gesture> trainingSet = new List<Gesture>();

    // Flame spell
    [SerializeField] ParticleSystem flame;
    [SerializeField] GameObject fireEmitter;
    [SerializeField] AudioSource magicCastSound;

    // Shrinking spell
    [SerializeField] DistanceGrab rayScript;

    [SerializeField] Transform rightControllerAnchor;



    // Start is called before the first frame update
    void Start()
    {
        //Load pre-made gestures
        TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("Spell-gesture-presets/");
        foreach (TextAsset gestureXml in gesturesXml)
            trainingSet.Add(GestureIO.ReadGestureFromXML(gestureXml.text));
        Debug.Log("training set: " + trainingSet.ToArray());
    }

    // Update is called once per frame
    void Update()
    {
        flame.transform.position = fireEmitter.transform.position;
        flame.transform.forward = transform.forward;

        // Check if holding down (long press) the right index trigger
        if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger) && OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) < clashPreventionThreshold)
        {
            if (!hasStartedSpell)
            {
                StartControllerMovement();
            }

            TraceControllerMovement();
        }
        // Check if just finished getting out of spell drawing mode
        else if (hasStartedSpell)
        {
            if (RecognizeSpell() == Spell.FIRE)
            {
                CastFireSpell();
            }
            else if (RecognizeSpell() == Spell.SHRINK)
            {
                CastShrinkingSpell();
            }
            else if (RecognizeSpell() == Spell.ENLARGE)
            {
                CastEnlargeSpell();
            }
        }
    }

    void StartControllerMovement()
    {
        Debug.Log("Starting spell drawing mode!");
        hasStartedSpell = true;

        if (trailCube && !trailPointer)
        {
            trailPointer = Instantiate(trailCube, this.transform.position, Quaternion.identity);
            trailPointer.transform.SetParent(this.transform);
        }

        if (positionsTraced.Count != 0)
        {
            positionsTraced.Clear();
        }

        addCube();
    }

    void TraceControllerMovement()
    {
        // Retain only discretely spaced positions of the controller movement
        Vector3 previousPosition = positionsTraced[positionsTraced.Count - 1];
        Vector3 controllerPosition = this.transform.position;
        if (Vector3.Distance(previousPosition, controllerPosition) >= movementTrackingPrecision)
        {
            addCube();
        }
    }

    void deleteTrail()
    {
        if (trailPointer)
        {
            Destroy(trailPointer);
        }
    }

    Spell RecognizeSpell()
    {
        Debug.Log("Ending controller movement...");

        // Destroy trail upon release of trigger
        deleteTrail();

        hasStartedSpell = false;

        Point[] pointCloud = new Point[positionsTraced.Count];
        // Our positionsTraced contains Vector3, but the PCloud algorithm takes in 2D points, so we project onto camera screen plane
        for (int i = 0; i < positionsTraced.Count; i++)
        {
            Vector2 point = Camera.main.WorldToScreenPoint(positionsTraced[i]);
            pointCloud[i] = new Point(point.x, point.y, 0);
        }

        Gesture candidate = new Gesture(pointCloud.ToArray());

        Result gestureResult = PointCloudRecognizer.Classify(candidate, trainingSet.ToArray());

        Debug.Log(gestureResult.GestureClass + " " + gestureResult.Score);

        // TODO: Define score thresholds
        if (gestureResult.GestureClass == "O")
        {
            return Spell.FIRE;
        }
        else if (gestureResult.GestureClass == "I")
        {
            return Spell.SHRINK;
        }
        else if (gestureResult.GestureClass == "M")
        {
            return Spell.ENLARGE;
        }
        else
        {
            return Spell.UNKNOWN;
        }
    }


    void addCube()
    {
        Vector3 controllerPosition = this.transform.position;
        positionsTraced.Add(controllerPosition);
        if (cubePrefab)
        {
            Destroy(Instantiate(cubePrefab, controllerPosition, Quaternion.identity), 3);
        }
    }


    void CastFireSpell()
    {
        Debug.Log("Casting fire spell!");
        magicCastSound.Play();

        if (flame)
        {
            flame.Play();

            flame.transform.position = fireEmitter.transform.position;
            flame.transform.forward = transform.forward;
            //flame.transform.SetParent(fireEmitter.transform);
            flame.Play();
        }

        // Vibration of controller during duration of flame
        if (flame.IsAlive())
        {
            OVRInput.SetControllerVibration(1, 1, OVRInput.Controller.RTouch);
        }
    }

    void CastShrinkingSpell()
    {
        Debug.Log("Casting shrinking spell!");
        // TODO: play shrinking magic sound effect

        // Get the reference to grabbedObject from ObjectHover ray casting functionality
        GameObject selectedObject = DistanceGrab.selectedObject;
        bool isSelected = DistanceGrab.isSelected;

        // An object is not selected, do nothing
        if (selectedObject == null || !isSelected)
        {
            if (selectedObject == null)
            {
                Debug.Log("ISSUE: null object");
            }
            Debug.Log("ISSUE: " + isSelected);

            Debug.Log("An object is not selected, do nothing");
            return;
        }

        // Check if we are allowed the shrink the object.
        // Hack: Must have clearer and cleaner ways to do this...
        if (selectedObject.GetComponent<CustomObjectProperties>() == null)
        {
            Debug.Log("No custom object properties detected while shrinking.");
            // Here, the object must have never been shrunk before. So we can shrink it.
            selectedObject.AddComponent<CustomObjectProperties>();
            Debug.Log("Added custom object properties.");
            selectedObject.transform.localScale = selectedObject.transform.localScale * 0.5f;
            Debug.Log("Shrinking.");
            selectedObject.GetComponent<CustomObjectProperties>().isShrunk = true;
            Debug.Log("Setting isShrunk to true.");

        }
        else
        {
            // Here, we must check.
            if (selectedObject.GetComponent<CustomObjectProperties>().isShrunk)
            {
                Debug.Log("Object has been shrunk before! Cannot shrink further.");
            }
            else
            {
                selectedObject.transform.localScale = selectedObject.transform.localScale * 0.5f;
                selectedObject.GetComponent<CustomObjectProperties>().isShrunk = true;
            }
        }


        rayScript.UnselectObject(selectedObject);
    }

    void CastEnlargeSpell()
    {
        Debug.Log("Casting enlarging spell!");
        // TODO: play enlarging magic sound effect

        // Get the reference to grabbedObject from ObjectHover ray casting functionality
        GameObject selectedObject = DistanceGrab.selectedObject;
        bool isSelected = DistanceGrab.isSelected;

        // An object is not selected, do nothing
        if (selectedObject == null || !isSelected)
        {
            if (selectedObject == null)
            {
                Debug.Log("ISSUE: null object");
            }
            Debug.Log("ISSUE: " + isSelected);

            Debug.Log("An object is not selected, do nothing");
            return;
        }

        // Check if we are allowed the enlarge the object.
        // Component is null iff never been shrunk before.
        // Hack: Must have clearer and cleaner ways to do this...
        if (selectedObject.GetComponent<CustomObjectProperties>() != null)
        {
            // It has been shrunk before, but we must check its status first.
            if (selectedObject.GetComponent<CustomObjectProperties>().isShrunk)
            {
                // Revert back to original size.
                selectedObject.transform.localScale = selectedObject.transform.localScale * 2.0f;
                selectedObject.GetComponent<CustomObjectProperties>().isShrunk = false;
            }
            else
            {
                Debug.Log("Object is not shrunk and cannot be enlarged further!");
            }
        }

        rayScript.UnselectObject(selectedObject);
    }
}