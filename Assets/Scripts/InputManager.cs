using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;


[DefaultExecutionOrder(-1)]
public class InputManager : Singleton<InputManager>
{
    // Start is called before the first frame update

    private TouchControls touchControls;
    public Touch activeTouch;
    public GameObject Hit;

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
    GameObject particle;
    void Update()
    {
        if (Touch.activeFingers.Count == 1)
        {
            Touch _activeTouch = Touch.activeFingers[0].currentTouch;
            activeTouch = _activeTouch;
            //Debug.Log($"Phase: {activeTouch.phase} | Position: {activeTouch.screenPosition}");
            //Debug.Log(activeTouch.delta);

            if (_activeTouch.phase == TouchPhase.Ended)
            {
                // Construct a ray from the current touch coordinates
                //Ray ray = Camera.main.ScreenPointToRay(new Vector3(activeTouch.screenPosition.x, activeTouch.screenPosition.y, -10));
                var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(_activeTouch.screenPosition), Vector2.zero, 1000f);
                if (hit.collider != null)
                {
                    //Debug.Log(hit.collider.gameObject.name);
                    Hit = hit.collider.gameObject;
                }

            }
        }
    }

}
