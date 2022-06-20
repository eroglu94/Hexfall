using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hexagon : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 Location;
    public Color Color;
    public Vector3 AxialCoord;
    public Vector3 OffsetCoord; //odd-q coordinate
    public int CreationOrder;

    public Hexagon(Vector3 location, Color color)
    {
        this.Location = location;

        this.GetComponent<SpriteRenderer>().color = color;
        this.Color = color;
    }

    void Start()
    {
        this.GetComponent<SpriteRenderer>().color = Color;
    }

    // Update is called once per frame
    void Update()
    {
        // Spin the object around the target at 20 degrees/second.
        //transform.RotateAround(this.transform.position, Vector3.forward, 20 * Time.deltaTime);
        //transform.RotateAround(new Vector3(30,30), Vector3.forward, 20 * Time.deltaTime);
    }
}
