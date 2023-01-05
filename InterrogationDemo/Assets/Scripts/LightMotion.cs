using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightMotion : MonoBehaviour
{
    public float scale;

    void Update()
    {
        //Rotates z angle with the sin function
        float z = scale * Mathf.Sin(Time.time) + 180;
        transform.eulerAngles = new Vector3(0f, 0f, z);
    }
}
