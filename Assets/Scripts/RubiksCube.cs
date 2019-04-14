using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RubiksCube : MonoBehaviour
{

    public GameObject CubeletPrefab;
    public int speed = 5;
    List<GameObject> Cubelets = new List<GameObject>();
    List<List<GameObject>> movableSlices = new List<List<GameObject>>();
    List<GameObject> movingSlice = new List<GameObject>();
    Vector3 position = Vector3.zero;
    Vector3 rotation = Vector3.zero;
    
    bool isRotating = false;
    bool isShuffling = false;
    bool movingX = false;
    bool movingY = false;
    
    Vector3[] RotationVectors = 
    {
        Vector3.right, Vector3.up, Vector3.forward,
        Vector3.left, Vector3.down, Vector3.back
    };

    void Start()
    {
        CreateCube();
    }
    
    void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
            if (Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();
        if (!isRotating && !isShuffling)
            CheckInput();
    }
    
    public void CreateCube()
    {
        if (!isRotating && !isShuffling && movableSlices.Count == 0)
        {
            foreach (GameObject cubelet in Cubelets)
                DestroyImmediate(cubelet);

            Cubelets.Clear();
            movableSlices.Clear();
            movingSlice.Clear();

            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                    for (int z = -1; z <= 1; z++)
                    {
                        GameObject cubelet = Instantiate(CubeletPrefab, transform, false);
                        cubelet.transform.localPosition = new Vector3(-x, -y, z);
                        cubelet.GetComponent<Cubelet>().SetColor(-x, -y, z);
                        Cubelets.Add(cubelet);
                    }

        }
    }

    void FixCube()
    {
        foreach (GameObject cubelet in Cubelets)
        {
            cubelet.transform.localPosition = new Vector3(
                    Mathf.Round(cubelet.transform.localPosition.x),
                    Mathf.Round(cubelet.transform.localPosition.y),
                    Mathf.Round(cubelet.transform.localPosition.z));
            cubelet.transform.localRotation = Quaternion.Euler(
                    Round((int)cubelet.transform.localRotation.eulerAngles.x),
                    Round((int)cubelet.transform.localRotation.eulerAngles.y),
                    Round((int)cubelet.transform.localRotation.eulerAngles.z));
        }
    }

    int Round(int angle)
    {
        if (angle % 90 > 45)
            return angle + (90 - angle % 90);
        else
            return angle - angle % 90;
    }

    void CheckInput()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 origin = ray.origin;

        if (Input.GetMouseButtonDown(0))
        {
            movableSlices.Clear();
            movingSlice.Clear();
            rotation = Vector3.zero;
            position = Vector3.zero;
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {
                if (hit.transform.name == "Cubelet(Clone)")
                    return;
                Transform selectedSide = hit.transform;
                Transform selectedCubelet = selectedSide.parent;
                position = selectedCubelet.localRotation * selectedSide.localPosition + selectedCubelet.localPosition;
                if (Mathf.Abs(position.x) >= 1.5)
                {
                    movableSlices.Add(GetYSlice(selectedCubelet));
                    movableSlices.Add(GetZSlice(selectedCubelet));
                }
                if (Mathf.Abs(position.y) >= 1.5)
                {
                    if (origin.x > Mathf.Abs(origin.z) || origin.x < -Mathf.Abs(origin.z))
                    {
                        movableSlices.Add(GetXSlice(selectedCubelet));
                        movableSlices.Add(GetZSlice(selectedCubelet));
                    }
                    else if (origin.z > Mathf.Abs(origin.x) || origin.z < -Mathf.Abs(origin.x))
                    {
                        movableSlices.Add(GetZSlice(selectedCubelet));
                        movableSlices.Add(GetXSlice(selectedCubelet));
                    }
                }
                if (Mathf.Abs(position.z) >= 1.5)
                {
                    movableSlices.Add(GetYSlice(selectedCubelet));
                    movableSlices.Add(GetXSlice(selectedCubelet));
                }
            }
        }
        if (Input.GetMouseButton(0) && movableSlices.Count != 0)
        {
            Vector3 rotationVector = Vector3.zero;
            Vector3 initialVector = rotationVector;
            if (Mathf.Abs(Input.GetAxis("Mouse X")) > Mathf.Abs(Input.GetAxis("Mouse Y")) && Input.GetAxis("Mouse X") != 0)
            {
                if (movingSlice.Count == 0)
                    movingSlice = movableSlices[0];
                if (movingSlice.All(movableSlices[0].Contains))
                {
                    if (Mathf.Abs(position.x) >= 1.5)
                        rotationVector = new Vector3(0, -Input.GetAxis("Mouse X") * speed, 0);
                    if (position.y >= 1.5)
                    {
                        if (origin.z > Mathf.Abs(origin.x))
                            rotationVector = new Vector3(0, 0, Input.GetAxis("Mouse X") * speed);
                        else if (origin.z < -Mathf.Abs(origin.x))
                            rotationVector = new Vector3(0, 0, -Input.GetAxis("Mouse X") * speed);
                        else if (origin.x > Mathf.Abs(origin.z))
                            rotationVector = new Vector3(Input.GetAxis("Mouse X") * speed, 0, 0);
                        else if (origin.x < -Mathf.Abs(origin.z))
                            rotationVector = new Vector3(-Input.GetAxis("Mouse X") * speed, 0, 0);
                    }
                    else if (position.y <= -1.5)
                    {
                        if (origin.z > Mathf.Abs(origin.x))
                            rotationVector = new Vector3(0, 0, -Input.GetAxis("Mouse X") * speed);
                        else if (origin.z < -Mathf.Abs(origin.x))
                            rotationVector = new Vector3(0, 0, Input.GetAxis("Mouse X") * speed);
                        else if (origin.x > Mathf.Abs(origin.z))
                            rotationVector = new Vector3(-Input.GetAxis("Mouse X") * speed, 0, 0);
                        else if (origin.x < -Mathf.Abs(origin.z))
                            rotationVector = new Vector3(Input.GetAxis("Mouse X") * speed, 0, 0);
                    }
                    if (Mathf.Abs(position.z) >= 1.5)
                        rotationVector = new Vector3(0, -Input.GetAxis("Mouse X") * speed, 0);
                }
            }
            else if (Mathf.Abs(Input.GetAxis("Mouse X")) < Mathf.Abs(Input.GetAxis("Mouse Y")) && Input.GetAxis("Mouse Y") != 0)
            {
                if (movingSlice.Count == 0)
                    movingSlice = movableSlices[1];
                if (movingSlice.All(movableSlices[1].Contains))
                {
                    if (position.x >= 1.5)
                        rotationVector = new Vector3(0, 0, Input.GetAxis("Mouse Y") * speed);
                    else if (position.x <= 1.5)
                        rotationVector = new Vector3(0, 0, -Input.GetAxis("Mouse Y") * speed);
                    if (Mathf.Abs(position.y) >= 1.5)
                    {
                        if (origin.z > Mathf.Abs(origin.x))
                            rotationVector = new Vector3(-Input.GetAxis("Mouse Y") * speed, 0, 0);
                        else if (origin.z < -Mathf.Abs(origin.x))
                            rotationVector = new Vector3(Input.GetAxis("Mouse Y") * speed, 0, 0);
                        else if (origin.x > Mathf.Abs(origin.z))
                            rotationVector = new Vector3(0, 0, Input.GetAxis("Mouse Y") * speed);
                        else if (origin.x < -Mathf.Abs(origin.z))
                            rotationVector = new Vector3(0, 0, -Input.GetAxis("Mouse Y") * speed);
                    }
                   if (position.z >= 1.5)
                        rotationVector = new Vector3(-Input.GetAxis("Mouse Y") * speed, 0, 0);
                    else if (position.z <= -1.5)
                        rotationVector = new Vector3(Input.GetAxis("Mouse Y") * speed, 0, 0);
                }
            }
            if (!rotationVector.Equals(initialVector))
                rotation = rotationVector;
            foreach (GameObject cubelet in movingSlice)
                cubelet.transform.RotateAround(Vector3.zero, rotationVector, speed);
        }
        if (Input.GetMouseButtonUp(0) && movingSlice.Count != 0)
        {
            int angle = 0;
            Vector3 rotationVector = Vector3.zero;

            if ((int)movingSlice[0].transform.localRotation.eulerAngles.x % 90 != 0)
                angle = (int)movingSlice[0].transform.localRotation.eulerAngles.x % 90;
            else if ((int)movingSlice[0].transform.localRotation.eulerAngles.y % 90 != 0)
                angle = (int)movingSlice[0].transform.localRotation.eulerAngles.y % 90;
            else if ((int)movingSlice[0].transform.localRotation.eulerAngles.z % 90 != 0)
                angle = (int)movingSlice[0].transform.localRotation.eulerAngles.z % 90;

            if (rotation.x != 0)
                rotationVector = new Vector3(angle > 45 ? 1 : -1, 0, 0);
            else if (rotation.y != 0)
                rotationVector = new Vector3(0, angle > 45 ? 1 : -1, 0);
            else if (rotation.z != 0)
                rotationVector = new Vector3(0, 0, angle > 45 ? 1 : -1);
            StartCoroutine(CompleteRotation(movingSlice, rotationVector, speed));
        }
    }

    List<GameObject> GetXSlice(Transform transform)
    {
        return Cubelets.FindAll(x => Mathf.Round(x.transform.localPosition.x) == transform.localPosition.x);
    }

    List<GameObject> GetYSlice(Transform transform)
    {
        return Cubelets.FindAll(x => Mathf.Round(x.transform.localPosition.y) == transform.localPosition.y);
    }

    List<GameObject> GetZSlice(Transform transform)
    {
        return Cubelets.FindAll(x => Mathf.Round(x.transform.localPosition.z) == transform.localPosition.z);
    }

    IEnumerator Rotate(List<GameObject> cubelets, Vector3 rotationVector, int angle, int speed)
    {
        isRotating = true;
        while (angle > 0)
        {
            foreach (GameObject cubelet in cubelets)
                cubelet.transform.RotateAround(Vector3.zero, rotationVector, (angle % speed == 0) ? speed : 1);
            angle -= (angle % speed == 0) ? speed : 1;
            yield return null;
        }
        isRotating = false;
        FixCube();
    }

    IEnumerator CompleteRotation(List<GameObject> cubelets, Vector3 rotationVector, int speed)
    {
        isRotating = true;
        if ((int)cubelets[0].transform.localRotation.eulerAngles.x % 90 != 0)
        {
            while ((int)cubelets[0].transform.localRotation.eulerAngles.x % 90 != 0)
            {
                foreach (GameObject cubelet in cubelets)
                    cubelet.transform.RotateAround(Vector3.zero, rotationVector, ((int)cubelets[0].transform.localRotation.eulerAngles.x % speed == 0) ? speed : 1);
                yield return null;
            }
        }
        else if ((int)cubelets[0].transform.localRotation.eulerAngles.y % 90 != 0)
        {
            while ((int)cubelets[0].transform.localRotation.eulerAngles.y % 90 != 0)
            {
                foreach (GameObject cubelet in cubelets)
                    cubelet.transform.RotateAround(Vector3.zero, rotationVector, ((int)cubelets[0].transform.localRotation.eulerAngles.y % speed == 0) ? speed : 1);
                yield return null;
            }
        }
        else if ((int)cubelets[0].transform.localRotation.eulerAngles.z % 90 != 0)
        {
            while ((int)cubelets[0].transform.localRotation.eulerAngles.z % 90 != 0)
            {
                foreach (GameObject cubelet in cubelets)
                    cubelet.transform.RotateAround(Vector3.zero, rotationVector, ((int)cubelets[0].transform.localRotation.eulerAngles.z % speed == 0) ? speed : 1);
                yield return null;
            }
        }
        isRotating = false;
        FixCube();
    }

    public void Shuffle()
    {
        if (!isRotating && !isShuffling && movableSlices.Count == 0)
            StartCoroutine(ShuffleRoutine());
    }

    IEnumerator ShuffleRoutine()
    {
        isShuffling = true;
        int axis, slice, direction;
        List<GameObject> moveCubelets = new List<GameObject>();
        Vector3 rotationVector;
        for (int moveCount = Random.Range(15, 30); moveCount >= 0; moveCount--)
        {
            axis = Random.Range(0, 3);
            slice = Random.Range(-1, 2);
            if (axis == 0)
                moveCubelets = Cubelets.FindAll(x => Mathf.Round(x.transform.localPosition.x) == slice);
            else if (axis == 1)
                moveCubelets = Cubelets.FindAll(x => Mathf.Round(x.transform.localPosition.y) == slice);
            else
                moveCubelets = Cubelets.FindAll(x => Mathf.Round(x.transform.localPosition.z) == slice);

            direction = Random.Range(0,2);
            rotationVector = RotationVectors[axis + 3 * direction];
            StartCoroutine(Rotate(moveCubelets, rotationVector, 90, 15));
            yield return new WaitForSeconds(.15f);
        }
        isShuffling = false;
    }
}
