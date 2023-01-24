using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour
{
    public Transform cubeToPlace;
    public GameObject cubeToCreate, allCubes, vfxSpawnCube;
    public GameObject[] canvasStartPage;
    public Color[] bgColors;
    public float cubeChangePlaceSpeed = 1f;
    private CubePosition nowCube = new CubePosition(0, 1, 0);
    private Transform mainCam;
    private Rigidbody allCubesRb;
    private Coroutine showCubePlace;
    private Color toCameraColor;
    private bool isLose, firstCube;
    private float camMoveToZPosition = 2f, camMoveToYPosition, camMoveSpeed = 2f, camChangeColorSpeed = 1.5f;
    private int prevCountMaxHorizontal;


    private List<Vector3> allCubesPositions = new List<Vector3>
    {
        new Vector3(0, 0, 0),
        new Vector3(0, 1, 0),
        new Vector3(1, 0, 0),
        new Vector3(-1, 0, 0),
        new Vector3(0, 0, 1),
        new Vector3(0, 0, -1),
        new Vector3(1, 0, 1),
        new Vector3(-1, 0, -1),
        new Vector3(-1, 0, 1),
        new Vector3(1, 0, -1)
    };

    private void Start()
    {
        toCameraColor = Camera.main.backgroundColor;
        mainCam = Camera.main.transform;
        camMoveToYPosition = 3.9f + nowCube.y - 1f;

        allCubesRb = allCubes.GetComponent<Rigidbody>();
        showCubePlace = StartCoroutine(ShowCubePlace());
    }

    private void Update()
    {
        if((Input.GetMouseButtonDown(0) || Input.touchCount > 0) && allCubes != null && cubeToPlace != null && !EventSystem.current.IsPointerOverGameObject())
        {
#if !UNITY_EDITOR
                if(Input.GetTouch(0).phase != TouchPhase.Began)
                    return;
#endif
            if (PlayerPrefs.GetString("music") != "No")
                GetComponent<AudioSource>().Play();
            if (!firstCube)
            {
                firstCube = true;
                foreach (GameObject obj in canvasStartPage)
                    Destroy(obj);
            }

            GameObject newCube = Instantiate(
                cubeToCreate,
                cubeToPlace.position,
                Quaternion.identity);

            newCube.transform.SetParent(allCubes.transform);
            nowCube.setVector(cubeToPlace.position);
            allCubesPositions.Add(nowCube.getVector());

            GameObject newVfx = Instantiate(
                vfxSpawnCube,
                newCube.transform.position,
                Quaternion.identity);
            Destroy(newVfx, 1.5f);

            allCubesRb.isKinematic = true;
            allCubesRb.isKinematic = false;
            SpawnPositions();
            MoveCameraChangeBg();
        }

        if(!isLose && allCubesRb.velocity.magnitude > 0.1f)
        {
            Destroy(cubeToPlace.gameObject);
            isLose = true;
            StopCoroutine(showCubePlace);
        }

        mainCam.localPosition = Vector3.MoveTowards(mainCam.localPosition,
            new Vector3(mainCam.localPosition.x, camMoveToYPosition, mainCam.localPosition.z),
            camMoveSpeed * Time.deltaTime);

        if (Camera.main.backgroundColor != toCameraColor)
            Camera.main.backgroundColor = Color.Lerp(Camera.main.backgroundColor, toCameraColor, Time.deltaTime/camChangeColorSpeed);
    }

    IEnumerator ShowCubePlace()
    {
        while(true)
        {
            SpawnPositions();
            yield return new WaitForSeconds(cubeChangePlaceSpeed);
        }    
    }

    [Obsolete]
    private void SpawnPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        if (IsPositionEmpty(new Vector3(nowCube.x + 1, nowCube.y, nowCube.z))
            && nowCube.x + 1 != cubeToPlace.position.x)
            positions.Add(new Vector3(nowCube.x + 1, nowCube.y, nowCube.z));
        if (IsPositionEmpty(new Vector3(nowCube.x - 1, nowCube.y, nowCube.z))
            && nowCube.x - 1 != cubeToPlace.position.x)
            positions.Add(new Vector3(nowCube.x - 1, nowCube.y, nowCube.z));
        if (IsPositionEmpty(new Vector3(nowCube.x, nowCube.y + 1, nowCube.z))
            && nowCube.y + 1 != cubeToPlace.position.y)
            positions.Add(new Vector3(nowCube.x, nowCube.y + 1, nowCube.z));
        if (IsPositionEmpty(new Vector3(nowCube.x, nowCube.y - 1, nowCube.z))
            && nowCube.y - 1 != cubeToPlace.position.y)
            positions.Add(new Vector3(nowCube.x, nowCube.y - 1, nowCube.z));
        if (IsPositionEmpty(new Vector3(nowCube.x, nowCube.y, nowCube.z + 1))
            && nowCube.z + 1 != cubeToPlace.position.z)
            positions.Add(new Vector3(nowCube.x, nowCube.y, nowCube.z + 1));
        if (IsPositionEmpty(new Vector3(nowCube.x, nowCube.y, nowCube.z - 1))
            && nowCube.z - 1 != cubeToPlace.position.z)
            positions.Add(new Vector3(nowCube.x, nowCube.y, nowCube.z - 1));

        if (positions.Count > 1)
            cubeToPlace.position = positions[UnityEngine.Random.RandomRange(0, positions.Count)];
        else if (positions.Count == 0)
            isLose = true;
        else
            cubeToPlace.position = positions[0];
    }

    private bool IsPositionEmpty(Vector3 targetPosition)
    {
        if(targetPosition.y == 0)
            return false;

        foreach(Vector3 position in allCubesPositions)
        {
            if (position.x == targetPosition.x && position.y == targetPosition.y && position.z == targetPosition.z)
                return false;
        }
        return true;
    }

    private void MoveCameraChangeBg()
    {
        int maxX = 0, maxY = 0, maxZ = 0, maxHorizontal;
        foreach(Vector3 position in allCubesPositions)
        {
            if (Mathf.Abs(Convert.ToInt32(position.x)) > maxX)
                maxX = Convert.ToInt32(position.x);
            if (Convert.ToInt32(position.y) > maxY)
                maxY = Convert.ToInt32(position.y);
            if (Mathf.Abs(Convert.ToInt32(position.z)) > maxZ)
                maxZ = Convert.ToInt32(position.z);
        }

        camMoveToYPosition = 3.9f + nowCube.y - 1f;

        maxHorizontal = maxX > maxZ ? maxX : maxZ;
        if(maxHorizontal % 3 == 0 && prevCountMaxHorizontal != maxHorizontal)
        {
            mainCam.localPosition -= new Vector3(0, 0, camMoveToZPosition);
            prevCountMaxHorizontal = maxHorizontal;
        }

        if (maxY >= 25)
            toCameraColor = bgColors[2];
        else if (maxY >= 15)
            toCameraColor = bgColors[1];
        else if (maxY >= 5)
            toCameraColor = bgColors[0];
    }
}

struct CubePosition
{
    public int x, y, z;

    public CubePosition(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3 getVector()
    {
        return new Vector3(x, y, z);
    }

    public void setVector(Vector3 position)
    {
        x = Convert.ToInt32(position.x);
        y = Convert.ToInt32(position.y);
        z = Convert.ToInt32(position.z);
    }
}