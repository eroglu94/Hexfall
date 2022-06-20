using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeReference] private int _numberOfColors = 5;
    [SerializeReference] private Vector2 _gridSize = new Vector2(8, 9);
    [SerializeReference] private GameObject _hexagonPrefab;


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

    }

    void StartGame()
    {
        // Initialize Grid
        // Initialize HexPrefabs
        //-------------------------------


        var gridManager = GetComponent<GridManager>();

        // Initialize Grid
        gridManager.InitializeGrid(_gridSize);

        //Initialize HexPrefabs
        InitializeHexes(gridManager.HexGrid, _numberOfColors);

    }

    void RestartGame()
    {

    }

    void InitializeHexes(List<GridManager.Hex> hexGrid, int numberOfColors)
    {
        // Initialize Hexes according to hexgrid data.
        foreach (var hex in hexGrid)
        {
            // Set Properties of temporary Hexagon Prefab
            _hexagonPrefab.GetComponent<Hexagon>().Color = PickRandomColor(numberOfColors);
            _hexagonPrefab.GetComponent<Hexagon>().Hex = hex;
            //----------------------------

            // Instantiate and spawn inital hexes
            GameObject hexGameObject = Instantiate(_hexagonPrefab, hex.Location, Quaternion.identity);
            hexGameObject.transform.localScale = Vector2.one * hex.HexagonSize;
            hexGameObject.transform.SetParent(GameObject.FindGameObjectWithTag("HexagonArea").transform, false);
            //---------------------------------

        }
    }


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
