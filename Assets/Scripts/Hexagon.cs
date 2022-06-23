using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Hexagon : MonoBehaviour
{
    public GameObject BombObject;
    public int BombMaxRound = 6;
    public GridManager.HexTile CurrentTile;
    // Start is called before the first frame update
    void Start()
    {
        //var gameObj = new GameObject("text");
        //gameObj.transform.SetParent(this.transform);
        //gameObj.AddComponent<TMPro.TextMeshPro>();
        //gameObj.GetComponent<TMPro.TextMeshPro>().text = "5";

    }

    // Update is called once per frame
    void Update()
    {
        // Bugfix - Have bomb child object but don't have IsBomb property?
        if (transform.childCount > 0 && !CurrentTile.IsBomb)
        {
            CurrentTile.IsBomb = true;
        }
    }

    public void MakeSelfBomb()
    {
        var childObj = Instantiate(BombObject, this.transform);
        childObj.transform.localPosition = new Vector3(0, 0, -20);
        this.GetComponentInChildren<TMPro.TextMeshPro>().text = BombMaxRound.ToString();
        CurrentTile.IsBomb = true;
    }

    public void Disarm()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        CurrentTile.IsBomb = false;
    }
    public void UpdateBombText()
    {
        try
        {
            var oldNumber = Convert.ToInt32(this.GetComponentInChildren<TextMeshPro>().text);
            oldNumber--;
            this.GetComponentInChildren<TextMeshPro>().text = oldNumber.ToString();
        }
        catch (Exception e)
        {

        }
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
