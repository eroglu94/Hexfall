using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;


[DefaultExecutionOrder(-1)]
public class InputManager : Singleton<InputManager>
{
    // Start is called before the first frame update

    private TouchControls touchControls;
    public Touch finger;

    void Awake()
    {
        //touchControls = new TouchControls();
        EnhancedTouchSupport.Enable();
    }

    void OnEnable()
    {
        //touchControls.Enable();
    }

    void OnDisable()
    {
        //touchControls.Disable();
    }

    void Start()
    {
        //touchControls.Touch.TouchPress.started += ctx => StartTouch(ctx);
        //touchControls.Touch.TouchPress.canceled += ctx => EndTouch(ctx);
    }

    void Update()
    {
        if (Touch.activeFingers.Count == 1)
        {
            Touch activeTouch = Touch.activeFingers[0].currentTouch;
            finger = activeTouch;
            Debug.Log($"Phase: {activeTouch.phase} | Position: {activeTouch.screenPosition}");
            Debug.Log(activeTouch.delta);
        }
    }

    //private void StartTouch(InputAction.CallbackContext context)
    //{
    //    Debug.Log("Touch Started: " + touchControls.Touch.TouchPosition.ReadValue<Vector2>());
    //}

    //private void EndTouch(InputAction.CallbackContext context)
    //{
    //    Debug.Log("Touch Ended");
    //}

}
