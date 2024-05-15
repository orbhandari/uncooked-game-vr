using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaltShaker : MonoBehaviour
{
    public GameObject prefabToCreate; // Reference to the salt prefab
    
    private Vector3 spawnPosition; // Position where salt is spawned
    [SerializeField] float generationInterval = 1.0f;
    private bool generateObjects = true;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GenerateObjectsCoroutine());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator GenerateObjectsCoroutine()
    {
        // Continuously generate objects while the boolean variable is true
        while (generateObjects)
        {
            // Calculate the angle between the object's forward direction and the desired direction
            float angle = Vector3.Angle(Vector3.up, transform.up);
            spawnPosition = transform.position + transform.up * 0.5f;

            if (angle >= 100.0f && this.GetComponent<CustomObjectProperties>() != null) {
                // Instantiate the object prefab
                // Hack by Om: Only allow pouring AFTER shrinking
                if (this.GetComponent<CustomObjectProperties>().isShrunk)
                    Instantiate(prefabToCreate, spawnPosition, Quaternion.identity);
            }

            // Wait for the specified interval before generating the next object
            yield return new WaitForSeconds(generationInterval);
        }
    }
}
