using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hexagon : MonoBehaviour
{
    // Start is called before the first frame update
    //public GridManager.Hex Hex;
    public GridManager.HexTile CurrentTile;
    //public Hexagon(Color color)
    //{
    //    this.GetComponent<SpriteRenderer>().color = color;
    //    this.CurrentTile.Color = color;
    //}

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //this.GetComponent<SpriteRenderer>().color = Color;
        //CurrentTile.Hexagon = this;

        // Spin the object around the target at 20 degrees/second.
        //transform.RotateAround(this.transform.position, Vector3.forward, 20 * Time.deltaTime);
        //transform.RotateAround(new Vector3(30,30), Vector3.forward, 20 * Time.deltaTime);
        //if (CurrentTile.Hexagon == null)
        //{
        //    CurrentTile.Hexagon = this;
        //}
    }

    public void UpdateSelf()
    {
        this.GetComponent<SpriteRenderer>().color = CurrentTile.Color;
        this.transform.localPosition = CurrentTile.Location;
        CurrentTile.Hexagon = this;
        this.transform.localRotation = Quaternion.identity;
    }

    public void UpdateSelfWithTransition()
    {
        this.GetComponent<SpriteRenderer>().color = CurrentTile.Color;
        CurrentTile.Hexagon = this;
        this.transform.localRotation = Quaternion.identity;

        //this.transform.localPosition = CurrentTile.Location;
        StartCoroutine(MoveToPosition(this.transform, CurrentTile.Location, 0.3f));

    }

    public void UpdateColor()
    {
        this.GetComponent<SpriteRenderer>().color = CurrentTile.Color;
    }

    public void Switch(Hexagon newHexagon, bool preserveColor = true)
    {
        var colorBefore = CurrentTile.Color;

        CurrentTile = newHexagon.CurrentTile;
        if (preserveColor)
            CurrentTile.Color = colorBefore;

        UpdateSelf();
    }

    public void Shift(GridManager.HexTile newHexTile)
    {
        CurrentTile.Neighbors = newHexTile.Neighbors;
        CurrentTile.AllNeighbors = newHexTile.AllNeighbors;
        CurrentTile.AxialCoords = newHexTile.AxialCoords;
        //CurrentTile.Color = newHexTile.Color
        CurrentTile.Hexagon = this;
        //CurrentTile.IsDestroyed = CurrentTile.IsDestroyed;
        CurrentTile.Location = newHexTile.Location;
        CurrentTile.OffsetCoords = newHexTile.OffsetCoords;
        CurrentTile.HexagonSize = newHexTile.HexagonSize;
        CurrentTile.Id = newHexTile.Id;
    }
    public void Switch(GridManager.HexTile newHexagon, bool preserveColor = true, bool updateSelf = true)
    {
        var colorBefore = CurrentTile.Color;
        
        CurrentTile = newHexagon;
        CurrentTile.Hexagon = this;
        if (preserveColor)
            CurrentTile.Color = colorBefore;

        if (updateSelf)
        {
            UpdateSelf();
        }
    }

    IEnumerator MoveToPosition(Transform transform, Vector3 position, float timeToReachTarget)
    {
        var currentPos = transform.localPosition;
        var t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / timeToReachTarget;
            transform.localPosition = Vector3.Lerp(currentPos, position, t);
            yield return null;
        }
    }
}
