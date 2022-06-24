using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using UnityEngine;
using Color = UnityEngine.Color;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

public class GridManager : MonoBehaviour
{
    [SerializeReference] private Vector2 _canvasSize;
    [System.Serializable]
    public class HexTile
    {
        public int Id;
        public Color Color;
        public Vector2 Location;
        public Vector2 AxialCoords;
        public Vector2 OffsetCoords;
        public float HexagonSize;
        public List<HexTileNeighbor> AllNeighbors;
        public List<HexTileNeighbor> Neighbors;
        public Hexagon Hexagon;
        public bool IsDestroyed;
        public bool IsBomb;

        public HexTile() { }
        public HexTile(HexTile copyHexTile)
        {
            Id = copyHexTile.Id;
            Color = copyHexTile.Color;
            Location = copyHexTile.Location;
            AxialCoords = copyHexTile.AxialCoords;
            OffsetCoords = copyHexTile.OffsetCoords;
            HexagonSize = copyHexTile.HexagonSize;
            AllNeighbors = copyHexTile.AllNeighbors;
            Neighbors = copyHexTile.Neighbors;
            Hexagon = copyHexTile.Hexagon;
            IsDestroyed = copyHexTile.IsDestroyed;
            IsBomb = copyHexTile.IsBomb;
        }
    }

    [System.Serializable]
    public class HexTileNeighbor
    {
        public string Name;
        public Vector2 AxialCordinate;
    }


    /// <summary>
    /// Initializes Grid System, Calculates relative positions and inner fields of hexTiles for future usages
    /// </summary>
    /// <param name="gridSize"></param>
    /// <returns></returns>
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
        int count = 0;
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


                var newHex = new HexTile
                {
                    Location = tempPos,
                    OffsetCoords = new Vector2(column, row),
                    AxialCoords = OddqToAxialCoordinate(new Vector2(column, row)),
                    HexagonSize = hexagonSize,
                    Id = count

                };

                hexTiles.Add(newHex);
                count++;
            }


        }

        //Align all hexes in the middle
        var coordinates = new List<Vector2>();
        foreach (var hexTile in hexTiles)
        {
            coordinates.Add(hexTile.Location);
        }
        var objRectangle = BaundaryOfCoordinates(coordinates);

        var leftMargin = (canvasWidthHeight.x - objRectangle.Width) / 2;
        var topMargin = (canvasWidthHeight.y - objRectangle.Height) / 2;
        topMargin = topMargin / 2; 

        foreach (var hexTile in hexTiles)
        {
            hexTile.Location = new Vector2(hexTile.Location.x + leftMargin, hexTile.Location.y - topMargin);
        }
        //-------------------------------------------------------------


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
                Name = "N"
            },
            new HexTileNeighbor
            {
                AxialCordinate = new Vector2(-1,0),
                Name = "NW"
            },new HexTileNeighbor
            {
                AxialCordinate = new Vector2(-1,+1),
                Name = "SW"
            }, new HexTileNeighbor
            {
                AxialCordinate = new Vector2(0, +1),
                Name = "S"
            },
            new HexTileNeighbor
            {
                AxialCordinate = new Vector2(+1,0),
                Name = "SE"
            },
            new HexTileNeighbor
            {
            AxialCordinate = new Vector2(+1,-1),
            Name = "NE"
            }
        };

        for (int i = 0; i < axialDirectionVectors.Length; i++)
        {
            var neighborCoordinate = new HexTileNeighbor
            {
                AxialCordinate = new Vector2(location.x + axialDirectionVectors[i].AxialCordinate.x,
                location.y + axialDirectionVectors[i].AxialCordinate.y),
                Name = axialDirectionVectors[i].Name
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


    #region Utility
    /// <summary>
    /// Finds Boundry as Rectangle of given coordinate list
    /// </summary>
    /// <param name="tiles"></param>
    /// <returns></returns>
    Rectangle BaundaryOfCoordinates(List<Vector2> tiles)
    {
        //starting point is set high
        float xleft = 10000;
        float xright = -10000;
        float yTop = -10000;
        float ybottom = 10000;

        for (int i = 0; i < tiles.Count; i++)
        {
            //find the most left
            if (tiles[i].x < xleft)
            {
                xleft = tiles[i].x;
            }

            //find the most right
            if (tiles[i].x > xright)
            {
                xright = tiles[i].x;
            }
            //find the top
            if (tiles[i].y > yTop)
            {
                yTop = tiles[i].y;
            }
            //find the bottom
            if (tiles[i].y < ybottom)
            {
                ybottom = tiles[i].y;
            }
        }


        return new Rectangle(Convert.ToInt32(xleft), Convert.ToInt32(yTop), Convert.ToInt32(xright - xleft), Convert.ToInt32(yTop - ybottom));
    }

    #endregion
}
