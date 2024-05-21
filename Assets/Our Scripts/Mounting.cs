using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mounting : MonoBehaviour
{

    public GameObject trackingSpace;
    public float mountedHeight = 2.0f;  // Height when mounted on a horse.
    public float normalHeight = 1.0f;
    private bool mounted = false;

    // Summonable cat mount prefab
    [SerializeField] GameObject catMount = null;

    // Summoned cat saved for reference
    private GameObject summonedCat = null;

    // Magic summoning circle effect
    [SerializeField] ParticleSystem summonEffect = null;

    // Reference to player controller
    [SerializeField] OVRPlayerController player = null;

    // Jump
    private CharacterController characterController = null;
    //private Vector3 moveThrottle = new Vector3(0, 0, 0);
    //[SerializeField] float mountedGravity = 0.0f;
    //[SerializeField] float jumpForce = 0.0f;
    //private float simulationRate = 60f;


    public float jumpHeight = 4.0f;
    public float jumpDuration = 2f;
    private float initialY;
    private bool isJumping = false;
    private float jumpStartTime;

    // New view height after mounting
    [SerializeField] float heightIncrease = 1;

    // New speed after mounting
    [SerializeField] float speedIncrease = 2;


    //private void Awake()
    //{
    //    normalHeight = trackingSpace.transform.localPosition.y;
    //}

    // Start is called before the first frame update
    void Start()
    {
        characterController = player.GetComponent<CharacterController>();
        normalHeight = trackingSpace.transform.localPosition.y;
    }

    void StartJump()
    {
        isJumping = true;
        initialY = transform.position.y;
        jumpStartTime = Time.time;
    }

    void ContinueJump()
    {
        float elapsed = Time.time - jumpStartTime;
        float completionRatio = elapsed / jumpDuration;

        if (completionRatio < 1.0f)
        {
            // Calculate the new Y position using a sine wave function for a smooth jump arc
            float newY = initialY + jumpHeight * Mathf.Sin(Mathf.PI * completionRatio);

            // Maintain the X and Z positions based on the current velocity of the character controller
            Vector3 horizontalVelocity = new Vector3(characterController.velocity.x, 0, characterController.velocity.z);

            // Calculate the new position for this frame by adding the horizontal velocity
            Vector3 newPosition = transform.position + horizontalVelocity * Time.deltaTime;
            newPosition.y = newY; // Set the new Y position calculated by the jump logic

            // Move the character to the new position
            characterController.Move(newPosition - transform.position);
        }
        else
        {
            isJumping = false;
            // Optionally force the character back to the ground or handle landing in another way
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.B) && !isJumping && mounted)
        {
            StartJump();
        }

        if (isJumping)
        {
            ContinueJump();
        }



        // Press X to summon
        if (OVRInput.GetDown(OVRInput.RawButton.X))
        {
            if (!isSummoned())
            {
                summonMount();
            }
            else
            {
                surrenderMount();
            }
        }
    }


    // Summons a rideable cat (the mount) and mounts the player
    void summonMount()
    {
        Debug.Log("Summoning mount...");

        // Instantiate cat object
        if (catMount != null)
        {
            summonedCat = Instantiate(catMount, player.transform.position, player.transform.rotation);
            Debug.Log("Instantiated cat mount.");

            if (summonEffect != null)
            {
                Debug.Log("Playing summon effect.");
                summonEffect.transform.position = player.transform.position;
                summonEffect.transform.localPosition = player.transform.position - new Vector3(0, 2.0f, 0);
                summonEffect.Play();
            }

            // TODO: Play meow effect

            // Set player's camera to heightened y-value
            ToggleHeight();
            Debug.Log("Increased player height after mounting.");

            // Set player's speed to faster value
            player.SetMoveScaleMultiplier(speedIncrease);

            // Set cat summon's positions
            summonedCat.transform.SetParent(player.transform);
            summonedCat.transform.localPosition = -0.25f * new Vector3(0, heightIncrease, 0);
            Debug.Log("Set cat's transform to player's.");

            Debug.Log("Summoning mount complete.");
        }
        else
        {
            Debug.Log("Cat mount prefab not found.");
        }

    }


    // "Unsummons" the rideable cat and dismounts the player
    void surrenderMount()
    {
        Debug.Log("Surrendering mount...");
        if (isSummoned())
        {
            Destroy(summonedCat);
            Debug.Log("Summoned cat destroyed.");
        }
        summonedCat = null;

        // Revert player back to normal height
        ToggleHeight();

        // Revert player back to normal speed
        player.SetMoveScaleMultiplier(1.0f);

        Debug.Log("Surrendering mount complete.");
    }


    // Checks if cat is already summoned
    bool isSummoned()
    {
        return summonedCat != null;
    }


    // Hack
    void ToggleHeight()
    {
        if (!mounted)
        {
            trackingSpace.transform.localPosition = new Vector3(0, mountedHeight, 0);
            Debug.Log("Mounted height");
            mounted = true;
        }
        else
        {
            trackingSpace.transform.localPosition = new Vector3(0, normalHeight, 0);
            Debug.Log("Normal height");
            mounted = false;
        }
    }
}
