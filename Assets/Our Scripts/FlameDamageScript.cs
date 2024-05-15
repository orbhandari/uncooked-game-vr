using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameDamageScript : MonoBehaviour
{
    // Track the cooking progress of food item
    private int cookProgress = 0; // Current number of particle hits
    [SerializeField] float cookTarget = 100;
    private List<GameObject> objectsOnDough = new List<GameObject>();

    // Feedback attributes
    private AudioSource audioSource;
    [SerializeField] ParticleSystem smokeEffect;
    [SerializeField] GameObject cooked_pizza;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("cook prog: " + cookProgress);
    }

    private void OnParticleCollision(GameObject particle)
    {
        Debug.Log("hit");
        // Check how many hits currently
        cookProgress++;

        if (cookProgress >= cookTarget)
        {
            Cook();
        }
    }

    void Cook()
    {
        CheckIngredients();
        CreatePizza();
    }

    // Check to see if all required ingredients is present on the pizza dough
    void CheckIngredients()
    {
        Debug.Log("INGREDIENTS ON DOUGH: ");
        Debug.Log(objectsOnDough.ToArray());
    }

    // Destroys the raw objects and spawns in the cooked pizza
    void CreatePizza()
    {
        //DestroyObjectsOnDough();

        // Destroy the pizza dough itself? Set to inactive for now
        this.GetComponent<Renderer>().enabled = false;

        // Spawn the pizza
        //Instantiate(cooked_pizza, this.transform);
        cooked_pizza.GetComponent<Renderer>().enabled = true;

        // Play smoke effect

        // Play cooking sound
        if (audioSource)
        {
            audioSource.Play();
        }

        // Play end scene
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision enter");
        // Check if the object has a Rigidbody to consider it as a valid object
        if (collision.rigidbody != null)
        {
            objectsOnDough.Add(collision.gameObject);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("Collision exit");
        if (objectsOnDough.Contains(collision.gameObject))
        {
            objectsOnDough.Remove(collision.gameObject);
        }
    }

    public void DestroyObjectsOnDough()
    {
        foreach (GameObject obj in objectsOnDough)
        {
            Destroy(obj);
        }
        objectsOnDough.Clear();
    }
}
