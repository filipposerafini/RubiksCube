using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cubelet : MonoBehaviour
{
    public GameObject UpPlane, DownPlane, FrontPlane, BackPlane, LeftPlane, RightPlane;

    public void SetColor(int x, int y, int z)
    {
        if (y == 1)
            UpPlane.SetActive(true);
        else if (y == -1)
            DownPlane.SetActive(true);
            
        if (x == 1)
            FrontPlane.SetActive(true);
        else if (x == -1)
            BackPlane.SetActive(true);
            
        if (z == -1)
            LeftPlane.SetActive(true);
        else if (z == 1)
            RightPlane.SetActive(true);  
    }
}
