using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int NumberOfColors = 5;
    public Vector2 GridSize = new Vector2(8, 9);


    // Start is called before the first frame update
    void Start()
    {
        var gridManager = GetComponent<GridManager>();
        gridManager.Deneme();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void StartGame()
    {

    }

    void RestartGame()
    {

    }
}
