using UnityEngine;
using UnityEngine.InputSystem;

public class SwipeDetection : MonoBehaviour
{
    public delegate void SwipeDelegate(Vector2 direction);
    public event SwipeDelegate OnSwipePerformed;

    [SerializeField] private float swipeResistance = 100f;
    private CatInputs controls;
    private Vector2 initialPosition;

    private void Awake() => controls = new CatInputs();
    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Start()
    {
        // Salva la posizione quando il dito tocca lo schermo
        controls.Player.PrimaryContact.started += ctx => {
            initialPosition = controls.Player.PrimaryPosition.ReadValue<Vector2>();
        };

        // Calcola lo swipe quando il dito viene alzato
        controls.Player.PrimaryContact.canceled += ctx => DetectSwipe();
    }

    private void DetectSwipe()
    {
        Vector2 currentPosition = controls.Player.PrimaryPosition.ReadValue<Vector2>();
        Vector2 delta = currentPosition - initialPosition;

        // Se lo spostamento verticale verso l'alto supera la resistenza
        if (delta.y > swipeResistance && Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
        {
            if (OnSwipePerformed != null)
                OnSwipePerformed(Vector2.up);
        }
    }
}