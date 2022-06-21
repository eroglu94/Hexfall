using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

public class GridManager : MonoBehaviour
{
    [SerializeReference] private Vector2 _canvasSize;
    [SerializeReference] private GameObject _hexagonPrefab; //TODO transfer this GameManager

    [System.Serializable]
    public class HexTile
    {
        public Vector2 Location;
        public Vector2 AxialCoords;
        public Vector2 OffsetCoords;
        public float HexagonSize;
        public List<HexTileNeighbor> AllNeighbors;
        public List<HexTileNeighbor> Neighbors;
        public Hexagon Hexagon;
    }

    [System.Serializable]
    public class HexTileNeighbor
    {
        public string name;
        public Vector2 AxialCordinate;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public List<HexTile> InitializeGrid(Vector2 gridSize)
    {
        List<HexTile> hexTiles = new List<HexTile>();
        // Create & Align grid
        // Store coordinates for future usage
        //----------------------------------------------------

        // We need $sizeOfCanvas and $gridSize to calculate each #hexagon size to fit the canvas;
        // Get Canvas Size
        RectTransform parentCanvas = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        Vector2 canvasWidthHeight = new Vector2(parentCanvas.rect.width, parentCanvas.rect.height);
        _canvasSize = canvasWidthHeight;

        // Find maximum size of 1 hexagon to fit screen
        var hexagonSize = _canvasSize.y / (gridSize.y - 0.5f) > _canvasSize.x / (gridSize.x - 1f) ? _canvasSize.x / (gridSize.x - 1f) : _canvasSize.y / (gridSize.y - 0.5f);
        Debug.Log($"HexTile Size: {hexagonSize}");


        // Create hexagons
        var hexagons = new List<List<GameObject>>(); //Store Hexagons
        Vector2 initialPosition = new Vector2(0, 0); // Initial Positions
        Quaternion rotation = new Quaternion(0, 0, 0, 0);
        for (int row = 0; row < gridSize.y; row++)
        {
            var tempPos = initialPosition;
            for (int column = 0; column < gridSize.x; column++)
            {

                tempPos.x = initialPosition.x + (hexagonSize * column * 0.75f) + (5 * column); // inital position + offset + margin
                tempPos.y = initialPosition.y - (hexagonSize * row * 0.85f) - (5 * row); // inital position - offset - margin

                //We need extra -y margin to odd columns. So we need to calculate odd-even columns to arrange hexagons, since they need to fit each other
                if (column % 2 != 0)
                {   //Column is odd
                    tempPos.y = initialPosition.y - (hexagonSize * row * 0.85f) - (hexagonSize / 2.30f) - (5 * row); //Extra -y margin to odd
                }


                //// TEST
                //var test = HexagonPrefab.GetComponent<HexTile>();
                //var test2 = new HexTile(new Vector2(), new Color());
                //var test3 = HexagonPrefab;


                var newHex = new HexTile
                {
                    Location = tempPos,
                    OffsetCoords = new Vector2(column, row),
                    AxialCoords = OddqToAxialCoordinate(new Vector2(column, row)),
                    HexagonSize = hexagonSize

                };

                hexTiles.Add(newHex);
            }


        }

        //TODO align all hexes in the middle of canvas !important

        // find available neighbors of all hexes
        foreach (var hex in hexTiles)
        {
            // Find all possible neighbors
            // Eliminate the neighbors that does not exist
            //--------------------------------------------

            //Finding all neighbors
            var allNeighbors = GetAxialNeighborsCoordinates(hex.AxialCoords);
            var actualNeighbors = new List<HexTileNeighbor>();

            //Eliminating neighbors
            foreach (var possibleNeighbor in allNeighbors)
            {
                if (hexTiles.Exists(x => x.AxialCoords == possibleNeighbor.AxialCordinate))
                {   //Neighbor Exists
                    actualNeighbors.Add(possibleNeighbor);
                }
            }


            hex.AllNeighbors = actualNeighbors; //TEST burayý kontrol et. deep copy mi yapýyor belli deðil
        }

        // Find 2 neighbors for selection of all hexes
        foreach (var hex in hexTiles)
        {
            var allNeighbors = hex.AllNeighbors;
            var selectionNeighbors = new List<HexTileNeighbor>();
            var previousTile = new HexTile();

            foreach (var possibleSelectionNeighbor in allNeighbors)
            {

                var neighborTile = hexTiles.Find(x => x.AxialCoords == possibleSelectionNeighbor.AxialCordinate);

                if (selectionNeighbors.Count == 1)
                {
                    // Check if previously founded neighbor is also neigbor of the current one.
                    if (neighborTile.AllNeighbors.Exists(x => x.AxialCordinate == previousTile.AxialCoords))
                    {
                        selectionNeighbors.Add(possibleSelectionNeighbor);
                        break;
                    }
                    continue; // Continue the loop until find suitable neighbor
                }

                previousTile = neighborTile;
                selectionNeighbors.Add(possibleSelectionNeighbor);

            }

            hex.Neighbors = selectionNeighbors;
        }


        return hexTiles;
    }

    /// <summary>
    /// Calculates Axial Coordinates of all neighbors of given HexTile
    /// </summary>
    /// <param name="location">Axial Coordinate of HexTile (column,row)</param>
    /// <returns></returns>
    List<HexTileNeighbor> GetAxialNeighborsCoordinates(Vector2 location)
    {
        var axialNeighbors = new List<HexTileNeighbor>();
        HexTileNeighbor[] axialDirectionVectors =
        {
            new HexTileNeighbor
            {
                AxialCordinate = new Vector2(0,-1),
                name = "N"
            },
            new HexTileNeighbor
            {
                AxialCordinate = new Vector2(-1,0),
                name = "NW"
            },new HexTileNeighbor
            {
                AxialCordinate = new Vector2(-1,+1),
                name = "SW"
            }, new HexTileNeighbor
            {
                AxialCordinate = new Vector2(0, +1),
                name = "S"
            },
            new HexTileNeighbor
            {
                AxialCordinate = new Vector2(+1,0),
                name = "SE"
            },
            new HexTileNeighbor
            {
            AxialCordinate = new Vector2(+1,-1),
            name = "NE"
            }
        };

        for (int i = 0; i < axialDirectionVectors.Length; i++)
        {
            var neighborCoordinate = new HexTileNeighbor
            {
                AxialCordinate = new Vector2(location.x + axialDirectionVectors[i].AxialCordinate.x,
                location.y + axialDirectionVectors[i].AxialCordinate.y),
                name = axialDirectionVectors[i].name
            };
            axialNeighbors.Add(neighborCoordinate);
        }

        return axialNeighbors;
    }

    /// <summary>
    /// Converts odd-q offset coordinate to Axial Coordinate
    /// </summary>
    /// <param name="oddqCoordinate">oddq offset coordinate (column,row)</param>
    /// <returns>Vector2(column,row) - x=column, y=row</returns>
    Vector2 OddqToAxialCoordinate(Vector2 oddqCoordinate)
    {
        //Offset odd-q system. Odd columns are shifted down.
        var hexCol = oddqCoordinate.x;
        var hexRow = oddqCoordinate.y;

        var column = hexCol;
        var row = hexRow - (hexCol - (Convert.ToInt32(hexCol) & 1)) / 2;

        return new Vector2(column, row);
    }



    List<Color> GenerateRandomColors(int numberOfColors)
    {
        //TODO delete this function. Transfer it to GameManager.cs

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
}
