using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwipeJumpHandler : MonoBehaviour
{
    public PlayerController player;

    private Vector2 startPos;
    private float minSwipeDistance = 50f; // Distanza minima del dito per saltare


    private void Start()
    {
        SwipeDetection.instance.swipePerformed += Jump;
    }

    private void Jump(Vector2 direction)
    {
        if (direction.x < direction.y) {
            player.MobileJump();
        }
        throw new NotImplementedException();
    }


    void Update()
    {
        // Supporto Touch
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            var touch = Touchscreen.current.primaryTouch;
            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {

                startPos = touch.position.ReadValue();
                Debug.Log("start pos " + startPos);
            }
        else if (Touchscreen.current != null )
            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended)
            {
                Vector2 endPos = touch.position.ReadValue();
                float swipeVertical = endPos.y - touch.startPosition.value.y;
                Debug.Log("swipe vertical =" + swipeVertical);
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
            Debug.Log("endPos" +  endPos);
            if (endPos.y - startPos.y > minSwipeDistance)
            {
                player.MobileJump();
            }
        }
    }
}