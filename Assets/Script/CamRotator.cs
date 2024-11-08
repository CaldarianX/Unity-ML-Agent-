using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamRotator : MonoBehaviour
{
    public float speed = 40f;

    private void Update() {
        transform.Rotate(0,speed*Time.deltaTime,0);
    }
}
