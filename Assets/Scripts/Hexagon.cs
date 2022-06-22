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
    }

    public void UpdateSelf()
    {
        this.GetComponent<SpriteRenderer>().color = CurrentTile.Color;
        this.transform.localPosition = CurrentTile.Location;
        CurrentTile.Hexagon = this;
        this.transform.localRotation = Quaternion.identity;
    }

    public void Switch(Hexagon newHexagon, bool preserveColor = true)
    {
        var colorBefore = CurrentTile.Color;

        CurrentTile = newHexagon.CurrentTile;
        if (preserveColor)
            CurrentTile.Color = colorBefore;

        UpdateSelf();
    } 
    public void Switch(GridManager.HexTile newHexagon, bool preserveColor = true)
    {
        var colorBefore = CurrentTile.Color;

        CurrentTile = newHexagon;
        if (preserveColor)
            CurrentTile.Color = colorBefore;

        UpdateSelf();
    }

    //public GameObject GameObject()
    //{
    //    return this.transform.gameObject;
    //}
}
