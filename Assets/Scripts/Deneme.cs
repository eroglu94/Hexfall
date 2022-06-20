using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deneme : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeReference] Vector2 canvasSize;
    void Start()
    {
        // Get Canvas Size to calculate sizes of hexagons
        RectTransform parentCanvas = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        Vector2 canvasWidthHeight = new Vector2(parentCanvas.rect.width, parentCanvas.rect.height);
        canvasSize = canvasWidthHeight;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
