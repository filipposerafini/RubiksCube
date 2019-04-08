using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubiksCube : MonoBehaviour
{

    public GameObject CubeletPrefab;
    public int speed = 5;
    Transform CubeTransform;
    List<GameObject> Cubelets = new List<GameObject>();
    GameObject CenterCubelet;
    Transform selectedSide, selectedCubelet;
    List<List<GameObject>> movableSlices = new List<List<GameObject>>();
    List<GameObject> movingSlice = new List<GameObject>();
    Vector3 rotationVector = new Vector3(0, 0, 0);
    
    bool isRotating = false;
    bool isShuffling = false;
    
    List<GameObject> UpCubelets
    {
        get
        {
            return Cubelets.FindAll(x => Mathf.Round(x.transform.localPosition.y) == 1);
        }
    }
    
    List<GameObject> DownCubelets
    {
        get
        {
            return Cubelets.FindAll(x => Mathf.Round(x.transform.localPosition.y) == -1);
        }
    }

    List<GameObject> FrontCubelets
    {
        get
        {
            return Cubelets.FindAll(x => Mathf.Round(x.transform.localPosition.z) == -1);
        }
    }
    
    List<GameObject> BackCubelets
    {
        get
        {
            return Cubelets.FindAll(x => Mathf.Round(x.transform.localPosition.z) == 1);
        }
    }
    
    List<GameObject> LeftCubelets
    {
        get
        {
            return Cubelets.FindAll(x => Mathf.Round(x.transform.localPosition.x) == 1);
        }
    }
    
    List<GameObject> RightCubelets
    {
        get
        {
            return Cubelets.FindAll(x => Mathf.Round(x.transform.localPosition.x) == -1);
        }
    }
    
    List<GameObject> MiddleCubelets
    {
        get
        {
            return Cubelets.FindAll(x => Mathf.Round(x.transform.localPosition.x) == 0);
        }
    }
    
    List<GameObject> StandingCubelets
    {
        get
        {
            return Cubelets.FindAll(x => Mathf.Round(x.transform.localPosition.z) == 0);
        }
    }
    
    List<GameObject> EquatorCubelets
    {
        get
        {
            return Cubelets.FindAll(x => Mathf.Round(x.transform.localPosition.y) == 0);
        }
    }
    
    Vector3[] RotationVectors = 
    {
        new Vector3(0, 1, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 0),
        new Vector3(0, -1, 0), new Vector3(0, 0, -1), new Vector3(-1, 0, 0)
    };

    void Start()
    {
        CubeTransform = transform;
        CreateCube();
    }
    
    void Update()
    {
        if (!isRotating)
            KeyboardInput();
        MouseInput();
    }
    
    void CreateCube()
    {
        foreach (GameObject cubelet in Cubelets)
            DestroyImmediate(cubelet);

        Cubelets.Clear();

        for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
                for (int z = -1; z <= 1; z++)
                {
                    GameObject cubelet = Instantiate(CubeletPrefab, CubeTransform, false);
                    cubelet.transform.localPosition = new Vector3(-x, -y, z);
                    cubelet.GetComponent<Cubelet>().SetColor(-x, -y, z);
                    Cubelets.Add(cubelet);
                }
        CenterCubelet = Cubelets[13];
    }
    
    void KeyboardInput()
    {
        int direction;
        if (Input.GetKey(KeyCode.LeftShift))
            direction = -1;
        else
            direction = 1;
            
        if (Input.GetKeyDown(KeyCode.Return) && !isShuffling)
            StartCoroutine(Shuffle());
        else if (Input.GetKeyDown(KeyCode.Escape) && !isShuffling)
            CreateCube();
        else if (Input.GetKeyDown(KeyCode.U))
            StartCoroutine(Rotate(UpCubelets, new Vector3(0, direction, 0), speed));
        else if (Input.GetKeyDown(KeyCode.E))
            StartCoroutine(Rotate(EquatorCubelets, new Vector3(0, direction, 0), speed));        
        else if (Input.GetKeyDown(KeyCode.D))
            StartCoroutine(Rotate(DownCubelets, new Vector3(0, direction, 0), speed));
        else if (Input.GetKeyDown(KeyCode.F))
            StartCoroutine(Rotate(FrontCubelets, new Vector3(0, 0, direction), speed));
        else if (Input.GetKeyDown(KeyCode.S))
            StartCoroutine(Rotate(StandingCubelets, new Vector3(0, 0, direction), speed));
        else if (Input.GetKeyDown(KeyCode.B))
            StartCoroutine(Rotate(BackCubelets, new Vector3(0, 0, direction), speed));
        else if (Input.GetKeyDown(KeyCode.L))
            StartCoroutine(Rotate(LeftCubelets, new Vector3(direction, 0, 0), speed));
        else if (Input.GetKeyDown(KeyCode.M))
            StartCoroutine(Rotate(MiddleCubelets, new Vector3(direction, 0, 0), speed));
        else if (Input.GetKeyDown(KeyCode.R))
            StartCoroutine(Rotate(RightCubelets, new Vector3(direction, 0, 0), speed));
    }

    void MouseInput()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {
                if (hit.transform.name == "Cubelet(Clone)")
                    return;
                selectedSide = hit.transform;
                selectedCubelet = selectedSide.parent;
                selectedSide.Rotate(selectedSide.rotation.eulerAngles);
                Vector3 position = selectedSide.position + selectedCubelet.position;
                if (Mathf.Abs(position.x) > 1.5)
                {
                    movableSlices.Add(GetYSlice(selectedCubelet));
                    movableSlices.Add(GetZSlice(selectedCubelet));
                }
                if (Mathf.Abs(position.y) > 1.5)
                {
                    movableSlices.Add(GetXSlice(selectedCubelet));
                    movableSlices.Add(GetZSlice(selectedCubelet));
                }
                if (Mathf.Abs(position.z) > 1.5)
                {
                    movableSlices.Add(GetYSlice(selectedCubelet));
                    movableSlices.Add(GetXSlice(selectedCubelet));
                }
                foreach (List<GameObject> slice in movableSlices)
                    foreach (GameObject go in slice)
                        go.GetComponent<Renderer>().material.color = Color.white;
            }
        }
        if (Input.GetMouseButton(0) && movableSlices.Count != 0)
        {
            if (Mathf.Abs(Input.GetAxis("Mouse X")) > Mathf.Abs(Input.GetAxis("Mouse Y")))
            {
                if (movingSlice.Count == 0)
                    movingSlice = movableSlices[0];
                if (movingSlice == movableSlices[0])
                {

                }
            }
            else if (Mathf.Abs(Input.GetAxis("Mouse X")) < Mathf.Abs(Input.GetAxis("Mouse Y")))
            {
                if (movingSlice.Count == 0)
                    movingSlice = movableSlices[1];
                if (movingSlice == movableSlices[1])
                {

                }
            }
            else
                rotationVector = new Vector3(0, 0, 0);
            foreach (GameObject cubelet in movingSlice)
                cubelet.transform.RotateAround(CenterCubelet.transform.position, rotationVector, speed);
        }
        if (Input.GetMouseButtonUp(0))
        {
            StartCoroutine(EndRotation());
            movableSlices.Clear();
            movingSlice.Clear();
            foreach (GameObject cube in Cubelets)
                cube.GetComponent<Renderer>().material.color = Color.black;
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

    IEnumerator Shuffle()
    {
        int move, direction;
        List<GameObject> moveCubelets = new List<GameObject>();
        Vector3 rotationVector;

        isShuffling = true;

        for (int moveCount = Random.Range(15, 30); moveCount >= 0; moveCount--)
        {
            move = Random.Range(0, 9);

            switch (move)
            {
                case 0: moveCubelets = UpCubelets; break;
                case 1: moveCubelets = EquatorCubelets; break;
                case 2: moveCubelets = DownCubelets; break;
                case 3: moveCubelets = FrontCubelets; break;
                case 4: moveCubelets = StandingCubelets; break;
                case 5: moveCubelets = BackCubelets; break;
                case 6: moveCubelets = LeftCubelets; break;
                case 7: moveCubelets = MiddleCubelets; break;
                case 8: moveCubelets = RightCubelets; break;
            }

            direction = Random.Range(0,2);
            rotationVector = RotationVectors[move/3 + 3*direction];
            StartCoroutine(Rotate(moveCubelets, rotationVector, 15));
            yield return new WaitForSeconds(.15f);
        }

        isShuffling = false;
    }
    
    IEnumerator Rotate(List<GameObject> cubelets, Vector3 rotationVector, int speed)
    {
        isRotating = true;
        int angle = 0;
        
        while (angle < 90)
        {
            foreach (GameObject cubelet in cubelets)
                cubelet.transform.RotateAround(CenterCubelet.transform.position, rotationVector, speed);
            angle += speed;
            yield return null;
        }
        isRotating = false;
    }

    IEnumerator EndRotation()
    {
        yield return null;
    }
}
