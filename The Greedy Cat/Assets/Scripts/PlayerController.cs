using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private float horizontalInput;
    private bool isPressingLeft;
    private bool isPressingRight;

    private CatInputs controls;

    [Header("Mobile Swipe")]
    [SerializeField] private SwipeDetection swipeReader; // Trascina qui l'oggetto con lo script SwipeDetection

    [Header("Movimento")]
    [SerializeField] private float speed;
    public float jumpForce;

    [Header("Ground Check")]
    public bool isGrounded = true;
    public float groundCheckDistance;
    public LayerMask whatIsGround;

    [Header("Wall Check")]
    public float wallCheckDistance;
    public bool isWallDetected;
    public LayerMask wallLayer;

    [Header("Wall Mechanics")]
    public bool isGrabbingWall;
    public float wallJumpForce = 10f;
    public float wallJumpHorizontalForce = 6f;

    [Header("Object Check")]
    public float objectCheckDistance;
    public bool isObjectDetected;
    public LayerMask pushableLayer;

    [Header("Push Mechanics")]
    public bool isPushing;
    private GameObject objectToPush;

    private int FacingDirection = 1;
    private bool FacingRight = true;
    private SpriteRenderer myRend;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip jumpSound;
    public AudioClip pushSound;
    public AudioClip snackCollectSound;
    public AudioClip meow;

    private void Awake()
    {
        controls = new CatInputs();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        myRend = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Start()
    {
        // Colleghiamo l'evento dello swipe alla nostra funzione di salto
        if (swipeReader != null)
        {
            swipeReader.OnSwipePerformed += (dir) => {
                if (dir == Vector2.up) OnSwipeJump();
            };
        }
    }

    void Update()
    {
        var keyboard = Keyboard.current;

        if (keyboard != null)
        {
            isPressingLeft = keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
            isPressingRight = keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;
        }

        // Logica per non annullare l'input se si premono A e D insieme (importante per il muro)
        if (isPressingLeft && isPressingRight) horizontalInput = 0;
        else if (isPressingLeft) horizontalInput = -1;
        else if (isPressingRight) horizontalInput = 1;
        else horizontalInput = 0;

        Flip();
        HandleMovement();
        CheckGroundAndWall();
        CheckForPushableObject();
        HandleJump(); // Mantiene il salto da tastiera (Spazio)
        HandleAnimation();

        // SPINTA AUTOMATICA
        if (isObjectDetected && !isPushing && horizontalInput != 0)
        {
            if ((horizontalInput > 0 && FacingDirection == 1) || (horizontalInput < 0 && FacingDirection == -1))
            {
                PushObject();
            }
        }

        if (controls.Player.Meow.triggered)
        {
            PlayMeow();
        }
    }

    void HandleJump()
    {
        if (controls.Player.Jump.triggered)
        {
            OnSwipeJump(); // Usiamo la stessa logica del salto mobile
        }
    }

    // FUNZIONE DI SALTO UNIFICATA (Usata sia da tastiera che da Swipe)
    public void OnSwipeJump()
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            PlayJumpSound();
        }
        else if (isGrabbingWall)
        {
            isGrabbingWall = false;
            rb.gravityScale = 1;
            int jumpDirection = -FacingDirection;
            rb.linearVelocity = new Vector2(jumpDirection * wallJumpHorizontalForce, wallJumpForce);
            PlayJumpSound();
        }
    }

    void Flip()
    {
        if (horizontalInput > 0 && !FacingRight && !isGrabbingWall)
        {
            FacingRight = true;
            myRend.flipX = false;
            FacingDirection = 1;
        }
        else if (horizontalInput < 0 && FacingRight && !isGrabbingWall)
        {
            FacingRight = false;
            myRend.flipX = true;
            FacingDirection = -1;
        }
    }

    void HandleMovement()
    {
        if (isGrabbingWall)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;
        }
        else
        {
            rb.gravityScale = 1;
            rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);
        }
    }

    void CheckGroundAndWall()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround | pushableLayer);
        isWallDetected = Physics2D.Raycast(transform.position, Vector2.right * FacingDirection, wallCheckDistance, wallLayer);
        isGrabbingWall = isWallDetected && !isGrounded && (isPressingLeft || isPressingRight);
    }

    void CheckForPushableObject()
    {
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y - 0.2f), Vector2.right * FacingDirection, objectCheckDistance, pushableLayer);
        isObjectDetected = hit.collider != null;
        objectToPush = isObjectDetected ? hit.collider.gameObject : null;
    }

    void PushObject()
    {
        if (objectToPush != null)
        {
            isPushing = true;
            if (audioSource != null && pushSound != null) audioSource.PlayOneShot(pushSound);
            StartCoroutine(MoveObject(objectToPush, FacingDirection));
        }
    }

    IEnumerator MoveObject(GameObject obj, int direction)
    {
        float duration = 0.2f;
        float elapsedTime = 0f;
        Vector3 startPos = obj.transform.position;
        Vector3 targetPos = startPos + new Vector3(direction, 0, 0);
        while (elapsedTime < duration)
        {
            if (obj == null) yield break;
            obj.transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (obj != null) obj.transform.position = targetPos;
        isPushing = false;
    }

    void HandleAnimation()
    {
        anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool("IsGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("isGrabbingWall", isGrabbingWall);
        anim.SetBool("IsPushing", isPushing);
    }

    public void Die() => Destroy(gameObject);

    void PlayJumpSound() { if (audioSource != null && jumpSound != null) audioSource.PlayOneShot(jumpSound); }
    void PlayMeow() { if (audioSource != null && meow != null) audioSource.PlayOneShot(meow); }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Snack"))
        {
            if (audioSource != null && snackCollectSound != null) audioSource.PlayOneShot(snackCollectSound);
        }
    }
}