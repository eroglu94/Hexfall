using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class GameManager : MonoBehaviour
{
    [SerializeReference] private int _numberOfColors = 5;
    [SerializeReference] private Vector2 _gridSize = new Vector2(8, 9);
    [SerializeReference] private GameObject _hexagonPrefab;
    [SerializeReference] private GameObject _particleEffect;

    private GameObject _selectedHexagon;
    private List<GridManager.HexTile> _hexTiles;
    private List<GameObject> _hexObjects;

    private InputManager im;


    private bool _isRotating;
    private bool _isSpawning;
    private bool _isDestroying;

    // Start is called before the first frame update
    void Start()
    {
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.Full); // Remove stack trace of Debug.Log messages

        StartGame();
        im = InputManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {

        var activeTouch = im.activeTouch;


        // Finger up
        if (Touch.activeFingers.Count == 1 && activeTouch.phase == TouchPhase.Ended && !_isRotating)
        {
            _selectedHexagon = im.Hit;

            if (_selectedHexagon)
            {
                AlignSelectionImage(_selectedHexagon.GetComponent<Hexagon>());


                //SpawnDestroyParticles(_selectedHexagon.GetComponent<Hexagon>());
                //Destroy(_selectedHexagon);
            }

        }

        try
        {
            //_selectedHexagon.GetComponent<SpriteRenderer>().color = Color.black;

        }
        catch (Exception e)
        {

        }

        //if ((Touch.activeFingers.Count == 1 && activeTouch.startScreenPosition.y - 10 > activeTouch.screenPosition.y) || _isRotating)
        //{
        //    // Swipe iþlemi oldu demektir.
        //    // Down Swipe
        //    // RotateSelectedHexagons(_selectedHexagon.GetComponent<Hexagon>());

        //    _isRotating = true;
        //    RotateSelectedHexagons(_selectedHexagon.GetComponent<Hexagon>());

        //    if (_selectedHexagon.transform.rotation.eulerAngles.z >= 115)
        //    {
        //        _isRotating = false;
        //        ManualRotationSelectedHexagons(_selectedHexagon.GetComponent<Hexagon>(), 120);
        //    }
        //}

        if ((Touch.activeFingers.Count == 1 && activeTouch.startScreenPosition.y + 10 < activeTouch.screenPosition.y) || _isRotating)
        {
            // Swipe iþlemi oldu demektir.
            // Down Swipe
            // RotateSelectedHexagons(_selectedHexagon.GetComponent<Hexagon>());
            if (_selectedHexagon)
            {

                _isRotating = true;
                RotateSelectedHexagons(_selectedHexagon.GetComponent<Hexagon>());

                if (_selectedHexagon.transform.rotation.eulerAngles.z >= 120)
                {
                    _isRotating = false;
                    ManualRotationSelectedHexagons(_selectedHexagon.GetComponent<Hexagon>(), 120);
                    UpdateGrid();
                }
            }
        }


        if (!_isRotating)
        {
            StartCoroutine(CheckGame());
        }


    }

    void StartGame()
    {
        // Initialize Grid
        // Initialize HexPrefabs
        //-------------------------------


        var gridManager = GetComponent<GridManager>();

        // Initialize Grid
        _hexTiles = gridManager.InitializeGrid(_gridSize);

        //Initialize HexPrefabs
        InitializeHexes(_numberOfColors);

    }

    void RestartGame()
    {

    }

    void InitializeHexes(int numberOfColors)
    {
        // First, create HexTiles and check for and available score. We don't want player to get score at the beginning. It is not fair :/
        // Initialize Hexes according to hexgrid data.
        // ----------------------------------------------
        var initialScore = true;
        while (initialScore)
        {

            foreach (var hexTile in _hexTiles)
            {
                hexTile.Color = PickRandomColor(numberOfColors);
            }
            var scoreList = CheckForScore();
            if (scoreList.Count == 0)
            {
                initialScore = false;

            }
        }


        // Initialize Hexes according to hexgrid data.
        _hexObjects = new List<GameObject>();
        foreach (var hexTile in _hexTiles)
        {
            // Set Properties of temporary HexTile Prefab
            //hexTile.Color = PickRandomColor(numberOfColors);
            _hexagonPrefab.GetComponent<Hexagon>().CurrentTile = hexTile;

            //----------------------------

            // Instantiate and spawn inital hexes
            GameObject hexGameObject = Instantiate(_hexagonPrefab, hexTile.Location, Quaternion.identity);
            hexGameObject.transform.localScale = Vector2.one * hexTile.HexagonSize;
            hexGameObject.transform.SetParent(GameObject.FindGameObjectWithTag("HexagonArea").transform, false);
            //---------------------------------

            hexTile.Hexagon = hexGameObject.GetComponent<Hexagon>();
            hexGameObject.GetComponent<Hexagon>().UpdateSelf();
            _hexObjects.Add(hexGameObject);
        }
    }

    IEnumerator CheckGame()
    {
        // This will check 


        //Check For Score
        var scoreHexagons = CheckForScore();
        if (scoreHexagons.Count != 0 && !_isSpawning)
        {
            // Destroy the hexagons
            // Increment score, move count
            _isDestroying = true;
            foreach (var scoreHexagon in scoreHexagons)
            {
                SpawnDestroyParticles(scoreHexagon.Hexagon);
                //Destroy(scoreHexagon.Hexagon.gameObject);
                scoreHexagon.Hexagon.gameObject.transform.localPosition = new Vector3(-1000, -1000);
                scoreHexagon.IsDestroyed = true;


            }
            _isDestroying = false;
            _isSpawning = true;
        }
        //----------------------------------------------


        //Spawn Missing Hexagons
        var spawnedHexagons = SpawnMissingHexagons();
        if (spawnedHexagons.Count != 0 && !_isDestroying)
        {
            
            //Slide down the new hexagons to their place.
            _isSpawning = true;
            foreach (var spawnedHexagon in spawnedHexagons)
            {
                var step = 1f * Time.deltaTime; // calculate distance to move
                var position = spawnedHexagon.Hexagon.gameObject.transform.localPosition;
                var targetPosition = spawnedHexagon.Location;
                StartCoroutine(MoveToPosition(spawnedHexagon.Hexagon.transform, targetPosition, 1f));

            }

            //if (Vector3.Distance(position, targetPosition) < 0.001f)
            //{
            //    _isSpawning = false;
            //}

        }
        //-----------------------------------------

        yield return new WaitForSeconds(0.1f);
    }


    List<GridManager.HexTile> SpawnMissingHexagons()
    {
        var spawnedHexagons = new List<GridManager.HexTile>();
        foreach (var hexTile in _hexTiles)
        {
            if (hexTile.IsDestroyed)
            {
                //Create new one at top of the screen. At the same line
                hexTile.Color = PickRandomColor(_numberOfColors);
                hexTile.Hexagon.UpdateColor();
                hexTile.IsDestroyed = false;
                var position = hexTile.Location;
                hexTile.Hexagon.gameObject.transform.localPosition = new Vector3(position.x, 70); // Spawn at 70pixel above the parent canvas

                spawnedHexagons.Add(hexTile);
            }
        }

        return spawnedHexagons;
    }

    List<GridManager.HexTile> CheckForScore()
    {
        // This will check the Hexagon Grid for possible same color grouping.
        //  If yes then it will return the gameobjects

        foreach (var hexTile in _hexTiles)
        {
            var colorHash = hexTile.Color.GetHashCode();
            var sameColorNeighbors = new List<GridManager.HexTile>();
            //sameColorNeighbors.Add(hexTile); //Add self tile first.
            var scoreNeighbors = new List<GridManager.HexTile>();

            foreach (var hexTileNeighbor in FindNeighborHexTiles(hexTile.AllNeighbors))
            {
                if (hexTileNeighbor.Color.GetHashCode() == colorHash)
                {
                    sameColorNeighbors.Add(hexTileNeighbor);
                }
            }

            foreach (var sameColorNeighbor in sameColorNeighbors)
            {
                foreach (var thirdNeighbor in sameColorNeighbors)
                {
                    if (sameColorNeighbor.AllNeighbors.Exists(x => x.AxialCordinate == thirdNeighbor.AxialCoords))
                    {
                        scoreNeighbors.Add(hexTile);
                        scoreNeighbors.Add(sameColorNeighbor);
                        scoreNeighbors.Add(thirdNeighbor);
                        return scoreNeighbors;
                    }
                }
            }
            //if (sameColorNeighbors.Count >= 3)
            //{
            //    Debug.Log("Possible Scores Count: " + sameColorNeighbors.Count);
            //    return sameColorNeighbors;
            //}
        }

        return new List<GridManager.HexTile>();

    }

    void UpdateGrid(string swipeDirection = "up")
    {
        // This function will update grid after each rotation animation of selected hexagons are complete.
        // "up" & "down" swipe

        // current          --> second neigbor
        // second           --> first neigbor
        // first neigbor   --> current


        var current = _selectedHexagon.GetComponent<Hexagon>();
        var firstN = FindNeighborHexagons(current.CurrentTile.Neighbors)[0];
        var secondN = FindNeighborHexagons(current.CurrentTile.Neighbors)[1];


        var tmp = new GameObject().AddComponent<Hexagon>();
        tmp.CurrentTile = current.CurrentTile;
        //current = firstN;
        //firstN = secondN;
        //secondN = tmp;

        // ReArrange _hexGameObject list

        current.Switch(new GridManager.HexTile(firstN.CurrentTile));
        firstN.Switch(new GridManager.HexTile(secondN.CurrentTile));
        secondN.Switch(new GridManager.HexTile(tmp.CurrentTile));


        //Change _selectedHexagon to new one
        _selectedHexagon = secondN.gameObject;

        // Update hexTiles
        _hexTiles = new List<GridManager.HexTile>();
        foreach (var hexObject in _hexObjects)
        {
            _hexTiles.Add(hexObject.GetComponent<Hexagon>().CurrentTile);
        }
    }

    List<Hexagon> FindNeighborHexagons(List<GridManager.HexTileNeighbor> neighbors)
    {
        var neighborGameObjects = new List<Hexagon>();

        foreach (var neighbor in neighbors)
        {
            var element = _hexTiles.Find(x => x.AxialCoords == neighbor.AxialCordinate);

            neighborGameObjects.Add(element.Hexagon);

        }
        return neighborGameObjects;
    }

    List<GridManager.HexTile> FindNeighborHexTiles(List<GridManager.HexTileNeighbor> neighbors)
    {
        var neighborGameObjects = new List<GridManager.HexTile>();

        foreach (var neighbor in neighbors)
        {
            var element = _hexTiles.Find(x => x.AxialCoords == neighbor.AxialCordinate);

            neighborGameObjects.Add(element);

        }
        return neighborGameObjects;
    }

    GameObject FindHexObj(Vector2 axialCoord)
    {
        foreach (var hexObject in _hexObjects)
        {
            if (axialCoord == hexObject.GetComponent<Hexagon>().CurrentTile.AxialCoords)
            {
                return hexObject;
            }
        }

        return null;
    }

    List<Hexagon> FindNeighborObjects2(List<GridManager.HexTileNeighbor> neighbors)
    {
        var neighborGameObjects = new List<Hexagon>();

        foreach (var neighbor in neighbors)
        {
            var element = _hexTiles.Find(x => x.AxialCoords == neighbor.AxialCordinate);

            if (neighborGameObjects.Count == 1)
            {
                // Check if previously founded neighbor is also neigbor of the current one.
                //neighborGameObjects[0];
                //element
                if (element.AllNeighbors.Exists(x => x.AxialCordinate == neighborGameObjects[0].CurrentTile.AxialCoords))
                {
                    neighborGameObjects.Add(element.Hexagon);
                    return neighborGameObjects;
                }
                continue; // Continue the loop until find suitable neighbor
            }

            neighborGameObjects.Add(element.Hexagon);

            // Returns when found 2 neighbours.
            if (neighborGameObjects.Count == 2)
            {
                return neighborGameObjects;
            }
        }

        // Code should never come to this line.
        return new List<Hexagon>();
    }

    void SpawnDestroyParticles(Hexagon destroyedHexagon)
    {
        var spawnPosition = new Vector3(destroyedHexagon.transform.localPosition.x,
            destroyedHexagon.transform.localPosition.y, -20);

        GameObject particle = Instantiate(_particleEffect, spawnPosition, Quaternion.identity);
        //particle.transform.localScale = Vector2.one * destroyedHexagon.CurrentTile.HexagonSize;
        var settings = particle.GetComponent<ParticleSystem>().main;
        settings.startColor = destroyedHexagon.CurrentTile.Color;
        particle.transform.SetParent(GameObject.FindGameObjectWithTag("HexagonArea").transform, false);

    }

    void RotateSelectedHexagons(Hexagon selectedHexagon)
    {
        var selectionImage = GameObject.FindGameObjectWithTag("Selection");
        var neighborHexagons = FindNeighborHexagons(selectedHexagon.CurrentTile.Neighbors);

        var rotatingObjects = new List<GameObject>();
        rotatingObjects.Add(selectionImage);
        rotatingObjects.Add(selectedHexagon.gameObject);
        rotatingObjects.Add(neighborHexagons[0].gameObject);
        rotatingObjects.Add(neighborHexagons[1].gameObject);


        // Spin the object around the target at 20 degrees/second.
        //transform.RotateAround(this.transform.position, Vector3.forward, 20 * Time.deltaTime);
        //transform.RotateAround(new Vector3(30,30), Vector3.forward, 20 * Time.deltaTime);

        // Transform around middle position, selected image position is our middle position since we calculated it before.
        foreach (var obj in rotatingObjects)
        {
            //transform.RotateAround(Camera.main.ScreenToWorldPoint(selectionImage.transform.position), Vector3.forward, 5 * Time.deltaTime);

            //obj.transform.RotateAround(Camera.main.ScreenToWorldPoint(selectionImage.transform.position), Vector3.forward, 5 * Time.deltaTime);
            obj.transform.RotateAround(selectionImage.transform.position, Vector3.forward, 360 * Time.deltaTime);
        }
    }

    void ManualRotationSelectedHexagons(Hexagon selectedHexagon, int angle)
    {
        var selectionImage = GameObject.FindGameObjectWithTag("Selection");
        var neighborHexagons = FindNeighborHexagons(selectedHexagon.CurrentTile.Neighbors);

        var rotatingObjects = new List<GameObject>();
        //rotatingObjects.Add(selectionImage);
        rotatingObjects.Add(selectedHexagon.gameObject);
        rotatingObjects.Add(neighborHexagons[0].gameObject);
        rotatingObjects.Add(neighborHexagons[1].gameObject);


        // Spin the object around the target at 20 degrees/second.
        //transform.RotateAround(this.transform.position, Vector3.forward, 20 * Time.deltaTime);
        //transform.RotateAround(new Vector3(30,30), Vector3.forward, 20 * Time.deltaTime);

        // Transform around middle position, selected image position is our middle position since we calculated it before.
        foreach (var obj in rotatingObjects)
        {
            //transform.RotateAround(Camera.main.ScreenToWorldPoint(selectionImage.transform.position), Vector3.forward, 5 * Time.deltaTime);

            //obj.transform.RotateAround(Camera.main.ScreenToWorldPoint(selectionImage.transform.position), Vector3.forward, 5 * Time.deltaTime);
            obj.transform.eulerAngles = new Vector3(0, 0, angle);
        }

        var defaultAngle = selectionImage.GetComponent<SelectionImage>().defaultAngle;
        selectionImage.transform.localRotation = Quaternion.Euler(0, 0, angle + defaultAngle);
    }

    #region Selection

    void AlignSelectionImage(Hexagon hexagon)
    {
        // Find middle point of 2 neighbors and hexagon itself and place selection image at that point
        // Find order of neighbors and rotate the selection image
        // TODO istediðimiz 3'lüyü alamadýðýmýz durumlar var. bir kere daha týkladýðýnda öbür seçimi almamýz lazým. Örnek apk oyununda öyle yapýlmýþ.
        // -----------------------------------------


        // Find neighbor objects and calculate the center Point
        var neigbors = FindNeighborHexagons(hexagon.CurrentTile.Neighbors);

        var p1 = hexagon.CurrentTile.Location;
        var p2 = neigbors[0].CurrentTile.Location;
        var p3 = neigbors[1].CurrentTile.Location;

        var centerPoint = new Vector2((p1.x + p2.x + p3.x) / 3, (p1.y + p2.y + p3.y) / 3);
        GameObject.FindGameObjectWithTag("Selection").transform.localPosition = new Vector3(centerPoint.x, centerPoint.y, -10);
        //--------------------------------------------------------

        // Find order of neighbors and rotate the selection image
        var n1 = hexagon.CurrentTile.Neighbors[0].name;
        var n2 = hexagon.CurrentTile.Neighbors[1].name;
        var r = n1 + n2;
        var angle = 0;
        switch (r)
        {
            case "NNW":
                angle = 60;
                break;
            case "NWSW":
                angle = 0;
                break;
            case "SWS":
                angle = 60;
                break;
            case "SSE":
                angle = 0;
                break;
            case "SENE":
                angle = 0;
                break;
            case "NEN":
                angle = 0;
                break;
            default:
                break;

        }

        GameObject.FindGameObjectWithTag("Selection").transform.localRotation = new Quaternion(0, 0, angle, 0);
        GameObject.FindGameObjectWithTag("Selection").GetComponent<SelectionImage>().defaultAngle = angle;


    }

    #endregion


    #region Utility

    List<Color> GenerateRandomColors(int numberOfColors)
    {
        var colorList = new List<Color>();

        //for (int i = 0; i < numberOfColors; i++)
        //{
        //    Color randomColor = new Color(
        //        Random.Range(0f, 1f),
        //        Random.Range(0f, 1f),
        //        Random.Range(0f, 1f)
        //    );
        //    //var randomColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        //    colorList.Add(randomColor);

        //}

        var indexcolors = new string[]
        {
            "#1CE6FF", "#FF34FF", "#FF4A46", "#008941", "#006FA6", "#A30059",
            "#FFDBE5", "#7A4900", "#0000A6", "#63FFAC", "#B79762", "#004D43", "#8FB0FF", "#997D87",
            "#5A0007", "#809693", "#FEFFE6", "#1B4400", "#4FC601", "#3B5DFF", "#4A3B53", "#FF2F80",
            "#61615A", "#BA0900", "#6B7900", "#00C2A0", "#FFAA92", "#FF90C9", "#B903AA", "#D16100",
            "#DDEFFF", "#000035", "#7B4F4B", "#A1C299", "#300018", "#0AA6D8", "#013349", "#00846F",
            "#372101", "#FFB500", "#C2FFED", "#A079BF", "#CC0744", "#C0B9B2", "#C2FF99", "#001E09",
            "#00489C", "#6F0062", "#0CBD66", "#EEC3FF", "#456D75", "#B77B68", "#7A87A1", "#788D66",
            "#885578", "#FAD09F", "#FF8A9A", "#D157A0", "#BEC459", "#456648", "#0086ED", "#886F4C",
        };

        for (int i = 0; i < numberOfColors; i++)
        {
            ColorUtility.TryParseHtmlString(indexcolors[i], out var tmpColor);
            colorList.Add(tmpColor);
        }

        return colorList;
    }
    Color PickRandomColor(int numberOfColors)
    {

        //for (int i = 0; i < numberOfColors; i++)
        //{
        //    Color randomColor = new Color(
        //        Random.Range(0f, 1f),
        //        Random.Range(0f, 1f),
        //        Random.Range(0f, 1f)
        //    );
        //    //var randomColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        //    colorList.Add(randomColor);

        //}

        var indexcolors = new string[]
        {
            "#1CE6FF", "#FF34FF", "#FF4A46", "#008941", "#006FA6", "#A30059",
            "#FFDBE5", "#7A4900", "#0000A6", "#63FFAC", "#B79762", "#004D43", "#8FB0FF", "#997D87",
            "#5A0007", "#809693", "#FEFFE6", "#1B4400", "#4FC601", "#3B5DFF", "#4A3B53", "#FF2F80",
            "#61615A", "#BA0900", "#6B7900", "#00C2A0", "#FFAA92", "#FF90C9", "#B903AA", "#D16100",
            "#DDEFFF", "#000035", "#7B4F4B", "#A1C299", "#300018", "#0AA6D8", "#013349", "#00846F",
            "#372101", "#FFB500", "#C2FFED", "#A079BF", "#CC0744", "#C0B9B2", "#C2FF99", "#001E09",
            "#00489C", "#6F0062", "#0CBD66", "#EEC3FF", "#456D75", "#B77B68", "#7A87A1", "#788D66",
            "#885578", "#FAD09F", "#FF8A9A", "#D157A0", "#BEC459", "#456648", "#0086ED", "#886F4C",
        };


        ColorUtility.TryParseHtmlString(indexcolors[Random.Range(0, numberOfColors)], out var tmpColor);
        return tmpColor;
    }

    IEnumerator MoveToPosition(Transform transform, Vector3 position, float timeToReachTarget, bool isHexMoving = true)
    {
        var currentPos = transform.localPosition;
        var t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / timeToReachTarget;
            transform.localPosition = Vector3.Lerp(currentPos, position, t);
            yield return null;
        }

        if (isHexMoving)
        {
            _isSpawning = false;

        }
    }

    #endregion
}
