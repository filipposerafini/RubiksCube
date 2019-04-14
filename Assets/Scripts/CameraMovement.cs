using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public int speed = 5;
    float x = 45.0f;
    float y = 30.0f;
    bool rotating = false;

    void LateUpdate()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    {
                        if (!Physics.Raycast(Camera.main.ScreenPointToRay(touch.position)))
                            rotating = true;
                        break;
                    }
                case TouchPhase.Moved:
                    {
                        if (rotating)
                        {
                            x += touch.deltaPosition.x * speed * Time.deltaTime;
                            y -= touch.deltaPosition.y * speed * Time.deltaTime;
                            y = Mathf.Clamp(y, -85, 85);
                        }
                        break;
                    }
                case TouchPhase.Ended:
                    {
                        rotating = false;
                        break;
                    }
                default: break;
            }
        }
        Quaternion rotation = Quaternion.Euler(0, x, y);
        transform.parent.rotation = Quaternion.Lerp(transform.parent.rotation, rotation, Time.deltaTime * speed);
    }
}
