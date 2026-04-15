using UnityEngine;

public class MobileInputHandler : MonoBehaviour
{
    public PlayerController player;

    private Vector2 startTouchPos;
    private float swipeThreshold = 50f;

    void Update()
    {
        // Supporto Touch (Mobile)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            HandleInputDetection(touch.position, touch.phase == TouchPhase.Began, touch.phase == TouchPhase.Ended);
        }
        // Supporto Mouse (Per testare su PC)
        else if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
        {
            HandleInputDetection(Input.mousePosition, Input.GetMouseButtonDown(0), Input.GetMouseButtonUp(0));
        }
    }

    private void HandleInputDetection(Vector2 pos, bool started, bool ended)
    {
        if (started) startTouchPos = pos;
        if (ended)
        {
            if (pos.y - startTouchPos.y > swipeThreshold)
            {
                player.MobileJump();
            }
        }
    }

    public void OnLeftZoneDown() => player.MobileMove(-1f);
    public void OnRightZoneDown() => player.MobileMove(1f);
    public void OnZoneUp() => player.MobileMove(0f);
}