using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwipeManager : Singleton<SwipeManager>
{
    public static Action<Vector2, Vector2> OnSwipePerformed;
    
    [SerializeField] private InputAction positionInputAction;
    [SerializeField] private InputAction pressInputAction;

    private Camera camera;
    private Vector2 initialPosition;
    private float tilePixelDistance;
    private bool swiping;
    private Vector2 CurrentPosition => positionInputAction.ReadValue<Vector2>();

    private void Start()
    {
        camera = Camera.main;
        positionInputAction.Enable();
        pressInputAction.Enable();
        pressInputAction.performed += _ =>
        {
            StartSwipe();
        };

        pressInputAction.canceled += _ =>
        {
            Debug.Log("Swipe Canceled");
            swiping = false;
        };
        
        positionInputAction.performed += _ =>
        {
            HandlePositionChange();
        };
    }
    
    public void CalculateTilePixelDistance(TileObject tileObject1, TileObject tileObject2)
    {
        var camera = Camera.main;
        tilePixelDistance = Mathf.Abs(camera.WorldToScreenPoint(tileObject2.transform.position).x - camera.WorldToScreenPoint(tileObject1.transform.position).x);
    }

    private void StartSwipe()
    {
        if (swiping)
        {
            return;
        }
        
        swiping = true;
        initialPosition = CurrentPosition;
        Debug.Log("Swipe Started");
    }

    private void HandlePositionChange()
    {
        if (swiping && (CurrentPosition - initialPosition).magnitude > tilePixelDistance)
        {
            TryToDetectSwipe();
        }
    }
    
    private void TryToDetectSwipe()
    {
        if (!GameManager.instance.inputEnabled)
        {
            return;
        }
        
        swiping = false;
        Vector2 initial = camera.ScreenToWorldPoint(new Vector3(initialPosition.x, initialPosition.y, camera.nearClipPlane));
        Vector2 current = camera.ScreenToWorldPoint(new Vector3(CurrentPosition.x, CurrentPosition.y, camera.nearClipPlane));
        
        Debug.Log("Swipe Ended: " + initial + " - " + current);
        OnSwipePerformed?.Invoke(initial, current);
    }
}
