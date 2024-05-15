using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoneyPot : MonoBehaviour
{

    void Start()
    {

        Debug.Log("This is just a simple log lost among others :'-( ");

        Debug.LogWarning("This is a warning way easier to find :-) ");

        int an_int = 123456;
        Debug.LogWarning(an_int);

        Debug.LogError("This is an error ;-) !");

        int[] array = new int[5];
        array[5] = 5;   // This will raise an error

    }

}