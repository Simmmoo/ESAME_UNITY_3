using UnityEngine;
using UnityEngine.InputSystem;

public class SwipeJumpHandler : MonoBehaviour
{
    public PlayerController player; // Trascina qui il tuo gatto nell'Inspector

    private Vector2 startPos;
    private float minSwipeDistance = 50f; // Distanza minima del dito per saltare

    void Update()
    {
        // Supporto Touch
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            var touch = Touchscreen.current.primaryTouch;

            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {
                startPos = touch.position.ReadValue();
            }

            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended)
            {
                Vector2 endPos = touch.position.ReadValue();
                float swipeVertical = endPos.y - startPos.y;

                // Se il dito è andato verso l'alto per più di 50 pixel
                if (swipeVertical > minSwipeDistance)
                {
                    player.MobileJump();
                }
            }
        }

        // Supporto Mouse (Per testare subito in Unity Editor)
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            startPos = Mouse.current.position.ReadValue();
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            Vector2 endPos = Mouse.current.position.ReadValue();
            if (endPos.y - startPos.y > minSwipeDistance)
            {
                player.MobileJump();
            }
        }
    }
}