using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class fingerwatch : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI myTextElement;
    private InputManager im;

    void deneme()
    {
        //Debug.Log($"Phase: {activeTouch.phase} | Position: {activeTouch.screenPosition}");
        //Debug.Log(activeTouch.delta);



    }
    // Start is called before the first frame update
    void Start()
    {
        im = InputManager.Instance;

    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            var activeTouch = im.activeTouch;
            var fingerStr = $"Phase: {activeTouch.phase} | Position: {activeTouch.screenPosition}" + "\n" +
                            "Delta: " + activeTouch.delta + "\n" +
                            "Start Pos: " + activeTouch.startScreenPosition;


            myTextElement.text = fingerStr;
        }
        catch (Exception e)
        {

        }
    }
}
