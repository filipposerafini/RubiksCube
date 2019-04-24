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
    
    bool isNew = false;
    bool isRotating = false;
    bool isShuffling = false;
    bool isExploded = false;
    
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
        {
            CheckInput();
            if (!isNew && isComplete() && !isExploded && !isRotating && Input.touchCount == 0)
                Explode(3.0f, 500.0f);
        }
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
            isNew = true;
            isExploded = false;
        }
    }

    bool isComplete()
    {
        return isSideComplete(Cubelets.FindAll(x => Mathf.Round(x.transform.localPosition.x) == -1)) &&
            isSideComplete(Cubelets.FindAll(x => Mathf.Round(x.transform.localPosition.x) == 1)) &&
            isSideComplete(Cubelets.FindAll(x => Mathf.Round(x.transform.localPosition.y) == -1)) &&
            isSideComplete(Cubelets.FindAll(x => Mathf.Round(x.transform.localPosition.y) == 1)) &&
            isSideComplete(Cubelets.FindAll(x => Mathf.Round(x.transform.localPosition.z) == -1)) &&
            isSideComplete(Cubelets.FindAll(x => Mathf.Round(x.transform.localPosition.z) == 1));
    }

    bool isSideComplete(List<GameObject> slice)
    {
        Cubelet center = null;
        GameObject centerPlane = null;
        int index = -1;
        for (int i = 0; i < 9; i++)
        {
            int count = 0;
            for (int j = 0; j < 6; j++)
            {
                if (slice[i].GetComponent<Cubelet>().Planes[j].activeInHierarchy)
                {
                    count++;
                    index = j;
                }
            }
            if (count == 1)
            {
                center = slice[i].GetComponent<Cubelet>();
                centerPlane = center.Planes[index];
                break;
            }
        }
        if (index != -1)
        {
            Vector3 position = Quaternion.Inverse(center.transform.localRotation) * centerPlane.transform.localPosition;
            foreach (GameObject cube in slice)
                if (!cube.GetComponent<Cubelet>().Planes[index].activeInHierarchy ||
                        (Quaternion.Inverse(cube.transform.localRotation) * cube.GetComponent<Cubelet>().Planes[index].transform.localPosition) != position)
                    return false;
            return true;
        }
        return false;
    }

    void Explode(float radius, float power)
    {
        foreach (GameObject cubelet in Cubelets)
        {
            cubelet.AddComponent<Rigidbody>();
            cubelet.GetComponent<Rigidbody>().mass = 1;
            cubelet.GetComponent<Rigidbody>().useGravity = false;
        }

        Vector3 explosionPos = new Vector3(Random.Range(-1, 2), Random.Range(-1, 2), Random.Range(-1, 2));
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddExplosionForce(power, explosionPos, radius, 3.0F);
        }
        isExploded = true;
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
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            Vector3 origin = ray.origin;
            switch(touch.phase)
            {
                case TouchPhase.Began:
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
                        break;
                    }
                case TouchPhase.Moved:
                    {
                        if (movableSlices.Count != 0)
                        {
                            Vector3 rotationVector = Vector3.zero;
                            Vector3 initialVector = rotationVector;
                            if (Mathf.Abs(touch.deltaPosition.x) > Mathf.Abs(touch.deltaPosition.y) && touch.deltaPosition.x != 0)
                            {
                                if (movingSlice.Count == 0)
                                    movingSlice = movableSlices[0];
                                if (movingSlice.All(movableSlices[0].Contains))
                                {
                                    if (Mathf.Abs(position.x) >= 1.5)
                                        rotationVector = new Vector3(0, -touch.deltaPosition.x * speed, 0);
                                    if (position.y >= 1.5)
                                    {
                                        if (origin.z > Mathf.Abs(origin.x))
                                            rotationVector = new Vector3(0, 0, touch.deltaPosition.x * speed);
                                        else if (origin.z < -Mathf.Abs(origin.x))
                                            rotationVector = new Vector3(0, 0, -touch.deltaPosition.x * speed);
                                        else if (origin.x > Mathf.Abs(origin.z))
                                            rotationVector = new Vector3(touch.deltaPosition.x * speed, 0, 0);
                                        else if (origin.x < -Mathf.Abs(origin.z))
                                            rotationVector = new Vector3(-touch.deltaPosition.x * speed, 0, 0);
                                    }
                                    else if (position.y <= -1.5)
                                    {
                                        if (origin.z > Mathf.Abs(origin.x))
                                            rotationVector = new Vector3(0, 0, -touch.deltaPosition.x * speed);
                                        else if (origin.z < -Mathf.Abs(origin.x))
                                            rotationVector = new Vector3(0, 0, touch.deltaPosition.x * speed);
                                        else if (origin.x > Mathf.Abs(origin.z))
                                            rotationVector = new Vector3(-touch.deltaPosition.x * speed, 0, 0);
                                        else if (origin.x < -Mathf.Abs(origin.z))
                                            rotationVector = new Vector3(touch.deltaPosition.x * speed, 0, 0);
                                    }
                                    if (Mathf.Abs(position.z) >= 1.5)
                                        rotationVector = new Vector3(0, -touch.deltaPosition.x * speed, 0);
                                }
                            }
                            else if (Mathf.Abs(touch.deltaPosition.x) < Mathf.Abs(touch.deltaPosition.y) && touch.deltaPosition.y != 0)
                            {
                                if (movingSlice.Count == 0)
                                    movingSlice = movableSlices[1];
                                if (movingSlice.All(movableSlices[1].Contains))
                                {
                                    if (position.x >= 1.5)
                                        rotationVector = new Vector3(0, 0, touch.deltaPosition.y * speed);
                                    else if (position.x <= 1.5)
                                        rotationVector = new Vector3(0, 0, -touch.deltaPosition.y * speed);
                                    if (Mathf.Abs(position.y) >= 1.5)
                                    {
                                        if (origin.z > Mathf.Abs(origin.x))
                                            rotationVector = new Vector3(-touch.deltaPosition.y * speed, 0, 0);
                                        else if (origin.z < -Mathf.Abs(origin.x))
                                            rotationVector = new Vector3(touch.deltaPosition.y * speed, 0, 0);
                                        else if (origin.x > Mathf.Abs(origin.z))
                                            rotationVector = new Vector3(0, 0, touch.deltaPosition.y * speed);
                                        else if (origin.x < -Mathf.Abs(origin.z))
                                            rotationVector = new Vector3(0, 0, -touch.deltaPosition.y * speed);
                                    }
                                    if (position.z >= 1.5)
                                        rotationVector = new Vector3(-touch.deltaPosition.y * speed, 0, 0);
                                    else if (position.z <= -1.5)
                                        rotationVector = new Vector3(touch.deltaPosition.y * speed, 0, 0);
                                }
                            }
                            if (!rotationVector.Equals(initialVector))
                                rotation = rotationVector;
                            foreach (GameObject cubelet in movingSlice)
                                cubelet.transform.RotateAround(Vector3.zero, rotationVector, speed);
                        }
                        break;
                    }
                case TouchPhase.Ended:
                    {
                        if (movingSlice.Count != 0)
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
                        break;
                    }
                default: break;
            }
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
        FixCube();
        isRotating = false;
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
        FixCube();
        isRotating = false;
    }

    public void Shuffle()
    {
        if (!isExploded && !isRotating && !isShuffling && movableSlices.Count == 0)
        {
            StartCoroutine(ShuffleRoutine());
            isNew = false;
        }
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
            StartCoroutine(Rotate(moveCubelets, rotationVector, 90, 10));
            yield return new WaitForSeconds(.25f);
        }
        isShuffling = false;
    }
}
