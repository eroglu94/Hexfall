using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Hexagon : MonoBehaviour
{
    public GameObject BombText;
    public Sprite BombImage;
    public int BombMaxRound = 6;
    public GridManager.HexTile CurrentTile;

    private Sprite _defaultImage;

    // Start is called before the first frame update
    void Start()
    {
        _defaultImage = transform.GetComponent<SpriteRenderer>().sprite;
    }

    // Update is called once per frame
    void Update()
    {
        // Bugfix - Have bomb child object but don't have IsBomb property?
        //if (transform.childCount > 0 && !CurrentTile.IsBomb)
        //{
        //    CurrentTile.IsBomb = true;
        //    transform.GetComponent<SpriteRenderer>().sprite = BombImage;
        //}
        //if (!CurrentTile.IsBomb && transform.childCount > 0)
        //{
        //    Disarm();
        //}
    }

    public void MakeSelfBomb()
    {
        CurrentTile.IsBomb = true;
        var childObj = Instantiate(BombText, this.transform);
        childObj.transform.localPosition = new Vector3(0, 0, -20);
        this.GetComponentInChildren<TMPro.TextMeshPro>().text = BombMaxRound.ToString();
        transform.GetComponent<SpriteRenderer>().sprite = BombImage;
    }

    public void Disarm()
    {
        CurrentTile.IsBomb = false;
        transform.GetComponent<SpriteRenderer>().sprite = _defaultImage;
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

    }
    public bool UpdateBombText()
    {
        try
        {
            var oldNumber = Convert.ToInt32(this.GetComponentInChildren<TextMeshPro>().text);
            oldNumber--;
            this.GetComponentInChildren<TextMeshPro>().text = oldNumber.ToString();

            if (oldNumber <= -1)
            {
                // Bomb is exploded
                return true;
            }
            
        }
        catch (Exception e)
        {

        }
        return false;
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
    
    /// <summary>
    /// Shifts self with given hexagon. Only locational data is shifted
    /// </summary>
    /// <param name="newHexTile"></param>
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

    /// <summary>
    /// Switches self with given hexagon
    /// </summary>
    /// <param name="newHexagon"></param>
    /// <param name="preserveColor"></param>
    /// <param name="updateSelf"></param>
    public void Switch(GridManager.HexTile newHexagon, bool preserveColor = true, bool updateSelf = true)
    {
        var colorBefore = CurrentTile.Color;

        //CurrentTile = newHexagon;
        Shift(newHexagon);

        CurrentTile.Hexagon = this;
        if (preserveColor)
            CurrentTile.Color = colorBefore;

        if (updateSelf)
        {
            UpdateSelf();
        }
    }

    public void Switch(Hexagon newHexagon, bool preserveColor = true)
    {
        var colorBefore = CurrentTile.Color;

        CurrentTile = newHexagon.CurrentTile;
        if (preserveColor)
            CurrentTile.Color = colorBefore;

        UpdateSelf();
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
