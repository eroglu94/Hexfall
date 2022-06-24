using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using Random = UnityEngine.Random;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class GameManager : MonoBehaviour
{
    [SerializeReference] private int _numberOfColors = 5;
    [SerializeReference] private Vector2 _gridSize = new Vector2(8, 9);
    [SerializeReference] private int _scorePerHexagon = 5;
    [SerializeReference] private int _bombCallScore = 1000;
    [SerializeReference] private GameObject _hexagonPrefab;
    [SerializeReference] private GameObject _particleEffect;
    [SerializeReference] private GameObject _sparkParticles;

    [SerializeReference]
    private Color32[] _hexagonColors = new[] { new Color32(28, 230, 255, 255), new Color32(255, 74, 70, 255), new Color32(0, 137, 65, 255), new Color32(226, 7, 160, 255), new Color32(110, 255, 70, 255), new Color32(240, 255, 70, 255), new Color32(0, 77, 67, 255), new Color32(122, 73, 0, 255) };

    //private GameObject _selectedHexagon;
    private int _selectedHexagon;
    private List<GridManager.HexTile> _hexTiles;
    private List<GameObject> _hexObjects;

    private InputManager im;

    private int _score = 0;
    private int _bombScore = 0;
    private int _moveCount = 0;
    private bool _isRotating;
    private int _rotateCount;
    private bool _isSpawning;
    private bool _isFilling;
    private bool _isDestroying;
    private bool _isHexesMoving;
    private bool _isGameOver;




    // Start is called before the first frame update
    void Start()
    {
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.Full); // Log stack trace

        StartGame();
        im = InputManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (_hexTiles.Count == 0)
        {
            return;
        }


        var activeTouch = im.activeTouch;


        // Finger up
        if (Touch.activeFingers.Count == 1 && activeTouch.phase == TouchPhase.Ended && !_isRotating)
        {
            //Check if restart button is triggered
            if (im.Hit.name == "RestartGame")
            {
                im.Hit = null;
                RestartGame();
                return;
            }

            //Check if ViewMove button is triggered
            if (im.Hit.name == "ViewMove")
            {
                ViewPossibleMoveButton();
                return;
            }

            //Allow selecting when no hexes are moving around
            if (!_isHexesMoving)
            {
                _selectedHexagon = im.Hit.GetComponent<Hexagon>().CurrentTile.Id;
            }
            if (FindHex(_selectedHexagon) && !_isHexesMoving)
            {
                AlignSelectionImage(FindHex(_selectedHexagon));
            }

        }

        // Don't let code pass this when game is over
        if (_isGameOver)
        {
            return;
        }

        // Check if user swiped his finger to rotate the hexes
        // Allow enter for completion of rotation animation
        if (((Touch.activeFingers.Count == 1 && activeTouch.startScreenPosition.y + 10 < activeTouch.screenPosition.y) || (Touch.activeFingers.Count == 1 && activeTouch.startScreenPosition.y - 10 > activeTouch.screenPosition.y))
          || _isRotating)
        {
            // Swipe happened
            // Reset rotateCount
            //---------------------------
            if (!_isRotating)
            {
                _rotateCount = 0;
            }


            if (FindHex(_selectedHexagon))
            {

            RotateAgain:
                _isRotating = true;
                RotateSelectedHexagons(FindHex(_selectedHexagon));

                if (FindHex(_selectedHexagon).transform.rotation.eulerAngles.z >= 120)
                {
                    // One rotate is complete. Check for score, if no score, rotate one more
                    _isRotating = false;
                    ManualRotationSelectedHexagons(FindHex(_selectedHexagon), 120);
                    UpdateGrid();
                    if (CheckForScore().Count != 0)
                    {


                        IncrementMoveCount();
                        DecreaseBombTimers();
                    }
                    if (_rotateCount >= 2)
                    {
                        _isRotating = false;
                    }
                    else if (CheckForScore().Count == 0 && _rotateCount < 2)
                    {
                        // No available explosion/score. Rotate one more
                        _rotateCount++;
                        goto RotateAgain;
                    }
                }
            }
        }

        // General check of game in each frame
        if (!_isRotating)
        {
            StartCoroutine(IsHexesMoving());
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
        // Destroy all previous game objects
        foreach (var hexObject in _hexObjects)
        {
            Destroy(hexObject);
        }

        _hexObjects = new List<GameObject>();
        _hexTiles = new List<GridManager.HexTile>();

        // Reset global variables
        IncrementScore(0);
        IncrementMoveCount(0);
        _score = 0;
        _moveCount = 0;
        _bombScore = 0;
        _isGameOver = false;
        GameObject.Find("txtMessage").GetComponent<TMPro.TextMeshProUGUI>().text = "Good Luck!";
        StartGame();
    }

    /// <summary>
    /// Wrapper update function for ScoreCheck, FillEmptySpaces after destruction, Spawn new hexagons
    /// </summary>
    /// <returns></returns>
    IEnumerator CheckGame()
    {
        // This will check game status

        //Check For Score
        var scoreHexagons = CheckForScore();
        if (scoreHexagons.Count != 0 && !_isSpawning && !_isHexesMoving)
        {
            // Destroy the hexagons
            // Increment score, move count
            _isDestroying = true;
            foreach (var scoreHexagon in scoreHexagons)
            {
                SpawnDestroyParticles(scoreHexagon.Hexagon);
                //Destroy(scoreHexagon.Hexagon.gameObject);
                scoreHexagon.Hexagon.gameObject.transform.localPosition = new Vector3(-1000, -1000);

                scoreHexagon.IsBomb = false;
                scoreHexagon.Hexagon.Disarm();

                scoreHexagon.IsDestroyed = true;

                IncrementScore(_scorePerHexagon);
            }
            _isDestroying = false;
            _isSpawning = true;

        }
        //----------------------------------------------


        // Fill the empty spaces below the columns
        // At the same time start spawning new ones
        if (!_isDestroying && _isSpawning && !_isHexesMoving)
        {
            if (!_isFilling)
            {
                FillEmptyPlaces();
            }
            SpawnMissingHexagons();
            _isSpawning = false;
        }
        //--------------------------------------
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// Initialize hexes as game objects. Uses grid data generated in Grid Manager
    /// </summary>
    /// <param name="numberOfColors"></param>
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

    /// <summary>
    /// Iterates through all _hextiles. Finds destroyed ones and spawns them.
    /// </summary>
    /// <returns>Spawned Hextiles</returns>
    List<GridManager.HexTile> SpawnMissingHexagons()
    {
        var spawnedHexagons = new List<GridManager.HexTile>();
        foreach (var hexTile in _hexTiles)
        {
            if (hexTile.IsDestroyed)
            {
                //Create new one at top of the screen. At the same line
                hexTile.IsBomb = false;
                hexTile.Hexagon.Disarm();
                if (_bombScore >= _bombCallScore)
                {   // Spawns bomb hexagon if score threshold is passed
                    _bombScore = _bombScore - _bombCallScore;
                    hexTile.IsBomb = true;
                    hexTile.Hexagon.MakeSelfBomb();
                }
                hexTile.Color = PickRandomColor(_numberOfColors);
                hexTile.Hexagon.UpdateColor();
                hexTile.IsDestroyed = false;
                var position = hexTile.Location;
                hexTile.Hexagon.gameObject.transform.localPosition = new Vector3(position.x, 70); // Spawn at 70pixel above the parent canvas
                hexTile.Hexagon.UpdateSelfWithTransition();
                spawnedHexagons.Add(hexTile);
            }
        }
        UpdateHexTiles();
        _isFilling = false;
        return spawnedHexagons;
    }

    /// <summary>
    /// Fills empty hextiles upon destructon by shifting each object in that columns
    /// </summary>
    void FillEmptyPlaces()
    {

        // Iterate destroyed hexagons.
        // Shift down remaining hexes in that column.
        // Change hextiles of destroyed hexagon to empty spaces
        // -----------------------------------------------------------------

        var destroyedHexagons = _hexTiles.FindAll(x => x.IsDestroyed);
        if (destroyedHexagons.Count == 0)
            return;

        _isFilling = true;

        var tmpHexTiles = new List<GridManager.HexTile>(); // We will make soft copy of hexTiles because we will lose information about empty cells after shifting.
        foreach (var hexTile in _hexTiles)
        {
            tmpHexTiles.Add(new GridManager.HexTile(hexTile));
        }

        List<int> freeColumns = new List<int>(); // Ex: 1,1,2   or   1,1,1,2,2,3

        foreach (var destroyedHexagon in destroyedHexagons)
        {

            var column = Convert.ToInt32(destroyedHexagon.OffsetCoords.x);
            var row = Convert.ToInt32(destroyedHexagon.OffsetCoords.y);
            freeColumns.Add(column);
            //GridManager.HexTile firstRowHexTile = new GridManager.HexTile(_hexTiles.Find(x => x.AxialCoords == new Vector2(column, 0)));

            foreach (var hexTile in _hexTiles)
            {
                // Shift all upper hexes by 1 hex. Same Column & smaller row index => upper hexes
                if (Convert.ToInt32(hexTile.OffsetCoords.x) == column && Convert.ToInt32(hexTile.OffsetCoords.y) < row)
                {

                    var newOffsetCoord = hexTile.OffsetCoords + new Vector2(0, +1); // To send it 1 hex below. Ex: 0,0 => 0,1

                    //Find newOffsetCoord in hextile and swap them
                    var oneHexTileBelow = new GridManager.HexTile(_hexTiles.Find(x => x.OffsetCoords == newOffsetCoord));
                    hexTile.Hexagon.Shift(oneHexTileBelow);
                    if (!hexTile.IsDestroyed)
                    {
                        hexTile.Hexagon.UpdateSelfWithTransition();
                    }
                }
            }
        }

        // freeColumns and destroyed hexagons count should be same practically
        // assign their hextiles to one the empty cells but do not move them to empty places
        List<int> placedColumns = new List<int>();
        for (int i = 0; i < freeColumns.Count; i++)
        {
            var placedCount = placedColumns.Count(x => x == freeColumns[i]);
            var placeTile = new GridManager.HexTile(tmpHexTiles.Find(x => x.OffsetCoords == new Vector2(freeColumns[i], placedCount)));
            placeTile.Hexagon = destroyedHexagons[i].Hexagon;
            placeTile.IsDestroyed = true;
            destroyedHexagons[i].Hexagon.Switch(placeTile, true, false);

            placedColumns.Add(freeColumns[i]);
        }

        UpdateHexTiles();
    }

    /// <summary>
    /// Checks for 3-hexagonal group of the same color are together at the moment, If true, function will return that group
    /// </summary>
    /// <returns>3-hexagonal group of the same color hexagon tiles</returns>
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

    /// <summary>
    /// Checks for possible player move by iterating through all hextiles, does virtual move for each of them and checks for possible score
    /// </summary>
    /// <returns>hextile that has score potential if rotated by selecting it</returns>
    GridManager.HexTile CheckForPossibleMove()
    {
        // We need UpdateGrid & CheckForScore functions to calculate possible move
        // Idea is, iterate _selectedHexagon one by one to all hexes.
        // Do update grid and CheckForScore.
        //------------------------------------------------------
        enabled = false;
        UpdateHexTiles(); // In case of loss of data;


        foreach (var hexTile in _hexTiles)
        {
            _selectedHexagon = hexTile.Id;
            var coords = hexTile.OffsetCoords;
            var possibleScore = new List<GridManager.HexTile>();

            for (int i = 0; i < 3; i++)
            {
                UpdateGrid();
                var tmpScore = CheckForScore();

                if (tmpScore.Count != 0)
                {
                    possibleScore = tmpScore;
                }
            }

            // Before returning our result, we need to make our grid same as before.
            // We need the reset the rotation of current hex group.
            // Thats why we don't immediatly returned the result. We should wait small loop to finish;
            if (possibleScore.Count != 0)
            {
                enabled = true;
                return hexTile;
            }
        }

        enabled = true;
        return null;
    }

    /// <summary>
    /// Checks all hexes for any movement. Edits global variable _isHexesMoving. Useful for waiting for animations to complete.
    /// </summary>
    /// <returns></returns>
    IEnumerator IsHexesMoving()
    {
        var p1 = new List<Vector3>();
        foreach (var hexTile in _hexTiles)
        {
            try
            {
                p1.Add(hexTile.Hexagon.gameObject.transform.localPosition);
            }
            catch (Exception e)
            {

            }
        }

        yield return new WaitForSeconds(0.1f);

        var p2 = new List<Vector3>();
        foreach (var hexTile in _hexTiles)
        {
            try
            {
                p2.Add(hexTile.Hexagon.gameObject.transform.localPosition);
            }
            catch (Exception e)
            {
            }
        }

        bool anyDiffer = false;
        for (int i = 0; i < p1.Count; i++)
        {
            if (p1[i] != p2[i])
            {
                try
                {
                    anyDiffer = true;
                }
                catch (Exception e)
                {
                }
            }
        }

        if (anyDiffer)
        {
            _isHexesMoving = true;

        }
        else
        {

            //Check if there is any hex not in his location
            var anyMove = false;
            foreach (var hexTile in _hexTiles)
            {
                var targetLoc = hexTile.Location;
                var currLoc = hexTile.Hexagon.gameObject.transform.localPosition;

                if (Vector3.Distance(currLoc, targetLoc) > 0.01f)
                {
                    hexTile.Hexagon.UpdateSelfWithTransition();
                    anyMove = true;
                }
            }

            _isHexesMoving = anyMove;
        }
    }

    /// <summary>
    /// User swipes the selected hexagons then Update grid after each rotation animation of selected hexagons are complete
    /// </summary>
    void UpdateGrid()
    {
        // This function will update grid after each rotation animation of selected hexagons are complete.
        // "up" & "down" swipe

        // current          --> second neigbor
        // second           --> first neigbor
        // first neigbor   --> current


        var current = FindHex(_selectedHexagon);
        var firstN = FindNeighborHexagons(current.CurrentTile.Neighbors)[0];
        var secondN = FindNeighborHexagons(current.CurrentTile.Neighbors)[1];

        var tmpTile = new GridManager.HexTile(current.CurrentTile);


        // ReArrange _hexGameObject list
        if (current.CurrentTile.Neighbors[0].Name + current.CurrentTile.Neighbors[1].Name == "NNE")
        {

            current.Switch(new GridManager.HexTile(secondN.CurrentTile));
            secondN.Switch(new GridManager.HexTile(firstN.CurrentTile));
            firstN.Switch(new GridManager.HexTile(tmpTile));

            //Change _selectedHexagon to new one
            _selectedHexagon = firstN.CurrentTile.Id;
        }
        else
        {
            current.Switch(new GridManager.HexTile(firstN.CurrentTile));
            firstN.Switch(new GridManager.HexTile(secondN.CurrentTile));
            secondN.Switch(new GridManager.HexTile(tmpTile));

            //Change _selectedHexagon to new one
            _selectedHexagon = secondN.CurrentTile.Id;
        }


        // Update hexTiles
        UpdateHexTiles();
    }

    /// <summary>
    /// Iterates hex gameObjects and saves them in _hexTiles. Safety function for in case of loss of data when reference editing in some functions
    /// </summary>
    void UpdateHexTiles()
    {
        // Update hexTiles
        _hexTiles = new List<GridManager.HexTile>();
        foreach (var hexObject in _hexObjects)
        {
            _hexTiles.Add(hexObject.GetComponent<Hexagon>().CurrentTile);
        }
        _hexTiles.Sort((x, y) => x.Id.CompareTo(y.Id));
    }

    /// <summary>
    /// DecreaseBomb timers by iterating all _hextiles. Should be called before or after the user's successfull move
    /// </summary>
    void DecreaseBombTimers()
    {
        //Find all bombs. Decrease their timers by one
        foreach (var hexTile in _hexTiles)
        {
            if (hexTile.IsBomb)
            {
                var isBombExploded = hexTile.Hexagon.UpdateBombText();

                if (isBombExploded)
                {
                    // Game Over
                    GameObject.Find("txtMessage").GetComponent<TMPro.TextMeshProUGUI>().text = "Bomb is exploded. Game Over :(";
                    _isGameOver = true;
                }
            }
        }
    }

    /// <summary>
    /// Finds neighbor hexagons
    /// </summary>
    /// <param name="neighbors"></param>
    /// <returns>Hexagon</returns>
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

    /// <summary>
    /// Finds Neighbor Hex Tiles
    /// </summary>
    /// <param name="neighbors"></param>
    /// <returns>HexTile</returns>
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

    /// <summary>
    /// Finds Hexagon in _hexTiles
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Hexagon FindHex(int id)
    {
        foreach (var hexTile in _hexTiles)
        {
            if (id == hexTile.Id)
            {
                return hexTile.Hexagon;
            }
        }

        return null;
    }

    /// <summary>
    /// Spawns 360 degree pixel particles on given hexagon
    /// </summary>
    /// <param name="destroyedHexagon"></param>
    void SpawnDestroyParticles(Hexagon destroyedHexagon)
    {
        var spawnPosition = new Vector3(destroyedHexagon.transform.localPosition.x,
            destroyedHexagon.transform.localPosition.y, -20);

        GameObject particle = Instantiate(_particleEffect, spawnPosition, Quaternion.identity);

        var color1 = destroyedHexagon.CurrentTile.Color;
        var color2 = new Color(color1.r * 1.5f, color1.g * 1.5f, color1.b * 1.5f);


        var settings = particle.GetComponent<ParticleSystem>().main;
        var startColor = settings.startColor;

        settings.startColor = new ParticleSystem.MinMaxGradient(color1, color2);

        particle.transform.SetParent(GameObject.FindGameObjectWithTag("HexagonArea").transform, false);

    }

    /// <summary>
    /// Rotates selected hexagon group and selection image itself.
    /// </summary>
    /// <param name="selectedHexagon"></param>
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
            obj.transform.RotateAround(selectionImage.transform.position, Vector3.forward, 550 * Time.deltaTime);
        }
    }

    /// <summary>
    /// Manually rotate selected hexagons without animation.
    /// </summary>
    /// <param name="selectedHexagon"></param>
    /// <param name="angle"></param>
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

    /// <summary>
    /// Increments MoveCount and updates UI text
    /// </summary>
    /// <param name="moveCount">if other than -1 is sended, it will override the moveCount UI</param>
    void IncrementMoveCount(int moveCount = -1)
    {
        if (moveCount != -1)
        {
            var txtobj = GameObject.Find("txtMoveCount").GetComponent<TMPro.TextMeshProUGUI>();

            txtobj.text = moveCount.ToString();
        }
        else
        {
            var txtobj = GameObject.Find("txtMoveCount").GetComponent<TMPro.TextMeshProUGUI>();
            _moveCount += 1;
            txtobj.text = _moveCount.ToString();
        }


    }

    /// <summary>
    /// Inceremnt Score Count and updates UI text
    /// </summary>
    /// <param name="score">if 0 is sended, it will override UI text</param>
    void IncrementScore(int score)
    {
        if (score == 0)
        {
            GameObject.Find("txtScore").GetComponent<TMPro.TextMeshProUGUI>().text = 0.ToString();
            return;
        }

        var txtobj = GameObject.Find("txtScore").GetComponent<TMPro.TextMeshProUGUI>();
        //var scoreCount = Convert.ToInt32(txtobj.text);
        _score += score;
        _bombScore += score;
        txtobj.text = _score.ToString();
    }

    /// <summary>
    /// UI button. View Possible Move button function
    /// </summary>
    void ViewPossibleMoveButton()
    {
        var possibleMove = CheckForPossibleMove();
        if (possibleMove != null)
        {
            var spawnPosition = new Vector3(possibleMove.Location.x,
                possibleMove.Location.y, -20);

            GameObject particle = Instantiate(_sparkParticles, spawnPosition, Quaternion.identity);

            particle.transform.SetParent(GameObject.FindGameObjectWithTag("HexagonArea").transform, false);
        }
        else
        {
            GameObject.Find("txtMessage").GetComponent<TMPro.TextMeshProUGUI>().text = "No Possible Move. Game Over :(";
            _isGameOver = true;
        }
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
        var n1 = hexagon.CurrentTile.Neighbors[0].Name;
        var n2 = hexagon.CurrentTile.Neighbors[1].Name;
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

        GameObject.FindGameObjectWithTag("Selection").transform.localScale = Vector3.one * (hexagon.CurrentTile.HexagonSize * 1.3f);
        GameObject.FindGameObjectWithTag("Selection").transform.localRotation = new Quaternion(0, 0, angle, 0);
        GameObject.FindGameObjectWithTag("Selection").GetComponent<SelectionImage>().defaultAngle = angle;


    }

    #endregion

    #region Utility

    Color PickRandomColor(int numberOfColors)
    {

        var tmpColor = new Color();
        tmpColor = _hexagonColors[Random.Range(0, numberOfColors)];
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
