using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class GameManager : MonoBehaviour
{
    [SerializeReference] private int _numberOfColors = 5;
    [SerializeReference] private Vector2 _gridSize = new Vector2(8, 9);
    [SerializeReference] private GameObject _hexagonPrefab;

    private GameObject _selectedHexagon;
    private List<GridManager.HexTile> _hexTiles;

    private InputManager inputManager;

    // Start is called before the first frame update
    void Start()
    {
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None); // Remove stack trace of Debug.Log messages

        StartGame();
        inputManager = InputManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (Touch.activeFingers.Count == 1)
        {
            if (inputManager.Hit != null)
            {
                _selectedHexagon = inputManager.Hit;
                //_selectedHexagon.GetComponent<Hexagon>().Color = Color.black;

                ////Find Neigbors and select them
                //var neighbors = FindNeighborObjects(_selectedHexagon.GetComponent<Hexagon>().CurrentTile.Neighbors);

                //foreach (var element in neighbors)
                //{
                //    element.Color = Color.black;
                //}

                AlignSelectionImage(_selectedHexagon.GetComponent<Hexagon>());

            }
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
        // Initialize Hexes according to hexgrid data.
        foreach (var hexTile in _hexTiles)
        {
            // Set Properties of temporary HexTile Prefab
            _hexagonPrefab.GetComponent<Hexagon>().Color = PickRandomColor(numberOfColors);
            _hexagonPrefab.GetComponent<Hexagon>().CurrentTile = hexTile;
            //----------------------------

            // Instantiate and spawn inital hexes
            GameObject hexGameObject = Instantiate(_hexagonPrefab, hexTile.Location, Quaternion.identity);
            hexGameObject.transform.localScale = Vector2.one * hexTile.HexagonSize;
            hexGameObject.transform.SetParent(GameObject.FindGameObjectWithTag("HexagonArea").transform, false);
            //---------------------------------

            hexTile.Hexagon = hexGameObject.GetComponent<Hexagon>();
        }
    }

    List<Hexagon> FindNeighborObjects(List<GridManager.HexTileNeighbor> neighbors)
    {
        var neighborGameObjects = new List<Hexagon>();

        foreach (var neighbor in neighbors)
        {
            var element = _hexTiles.Find(x => x.AxialCoords == neighbor.AxialCordinate);

            neighborGameObjects.Add(element.Hexagon);

        }
        return neighborGameObjects;
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



    #region Selection

    void AlignSelectionImage(Hexagon hexagon)
    {
        // Find middle point of 2 neighbors and hexagon itself and place selection image at that point
        // Find order of neighbors and rotate the selection image
        // -----------------------------------------


        // Find neighbor objects and calculate the center Point
        var neigbors = FindNeighborObjects(hexagon.CurrentTile.Neighbors);

        var p1 = hexagon.CurrentTile.Location;
        var p2 = neigbors[0].CurrentTile.Location;
        var p3 = neigbors[1].CurrentTile.Location;

        var centerPoint = new Vector2((p1.x + p2.x + p3.x) / 3, (p1.y + p2.y + p3.y) / 3);
        GameObject.FindGameObjectWithTag("Selection").transform.localPosition = centerPoint;
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

    #endregion
}
