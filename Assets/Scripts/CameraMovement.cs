using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public int speed = 5;
    float x = 0.0f;
    float y = 0.0f;

    void LateUpdate()
    {
        if (Input.GetMouseButton(1))
        {
            x += Input.GetAxis("Mouse X") * speed;
            y -= Input.GetAxis("Mouse Y") * speed;
            y = Mathf.Clamp(y, -90, 90);
        }

        Quaternion rotation = Quaternion.Euler(0, x, y);
        transform.parent.rotation = Quaternion.Lerp(transform.parent.rotation, rotation, Time.deltaTime * speed);
    }
}
