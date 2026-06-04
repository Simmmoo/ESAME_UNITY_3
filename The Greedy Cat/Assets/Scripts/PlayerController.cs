using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private float horizontalInput;
    private CatInputs controls;

    [Header("Movimento")]
    [SerializeField] private float speed = 7f;
    public float jumpForce = 12f;

    [Header("Ground Check")]
    public bool isGrounded = true;
    public float groundCheckDistance = 0.3f;
    public LayerMask whatIsGround;

    [Header("Wall Check")]
    public float wallCheckDistance = 0.5f;
    public bool isWallDetected;
    public LayerMask wallLayer;

    [Header("Wall Mechanics")]
    public bool isGrabbingWall;
    public float wallJumpForce = 10f;
    public float wallJumpHorizontalForce = 8f;

    [Header("Anti-Climb System")]
    private float wallJumpCooldown;
    [SerializeField] private float wallJumpCooldownDuration = 0.4f;

    [Header("Object Check")]
    public float objectCheckDistance = 0.5f;
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

    void Update()
    {
        // Gestione timer per evitare il ri-attacco immediato
        if (wallJumpCooldown > 0)
            wallJumpCooldown -= Time.deltaTime;

        // INPUT (Tastiera + On-Screen Buttons)
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            bool left = keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
            bool right = keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;
            if (left && right) horizontalInput = 0;
            else if (left) horizontalInput = -1;
            else if (right) horizontalInput = 1;
            else horizontalInput = 0;
        }

        CheckGroundAndWall();
        Flip();
        HandleMovement();
        CheckForPushableObject();

        if (controls.Player.Jump.triggered) Jump();

        HandleAnimation();

        if (keyboard != null && keyboard.mKey.wasPressedThisFrame) PlayMeow();

        // Meccanica Spinta
        if (isObjectDetected && !isPushing && horizontalInput != 0)
        {
            if ((horizontalInput > 0 && FacingDirection == 1) || (horizontalInput < 0 && FacingDirection == -1))
            {
                PushObject();
            }
        }
    }

    public void Jump()
    {
        if (rb == null) return;

        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            PlayJumpSound();
        }
        else if (isGrabbingWall)
        {
            // --- LOGICA RESTRITTIVA RICHIESTA ---
            // Se premo verso il muro (es. muro a DX e premo DX), blocco il salto
            bool pressingTowardsWall = (FacingDirection == 1 && horizontalInput > 0.1f) || (FacingDirection == -1 && horizontalInput < -0.1f);

            if (pressingTowardsWall)
            {
                return; // Ignora il salto, rimani attaccato
            }

            // Se arriviamo qui, il giocatore sta saltando "neutro" o verso l'esterno
            wallJumpCooldown = wallJumpCooldownDuration;
            isGrabbingWall = false;
            rb.gravityScale = 1;

            int jumpDirection = -FacingDirection;
            rb.linearVelocity = new Vector2(jumpDirection * wallJumpHorizontalForce, wallJumpForce);

            // Si gira per guardare lontano dal muro
            FacingRight = !FacingRight;
            myRend.flipX = !myRend.flipX;
            FacingDirection = -FacingDirection;

            PlayJumpSound();
        }
    }

    void HandleMovement()
    {
        if (rb == null) return;

        if (isGrabbingWall)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;
        }
        else
        {
            rb.gravityScale = 1;
            // Se siamo in cooldown (appena saltato), riduciamo il controllo orizzontale per evitare di tornare subito indietro
            float currentSpeed = (wallJumpCooldown > 0) ? horizontalInput * (speed * 0.5f) : horizontalInput * speed;
            rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
        }
    }

    void CheckGroundAndWall()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround | pushableLayer);
        isWallDetected = Physics2D.Raycast(transform.position, Vector2.right * FacingDirection, wallCheckDistance, wallLayer);

        // Si attacca solo se in aria, se rileva un muro e se NON è in cooldown
        if (!isGrounded && isWallDetected && wallJumpCooldown <= 0)
        {
            isGrabbingWall = true;
        }

        if (isGrounded)
        {
            isGrabbingWall = false;
        }
    }

    void Flip()
    {
        if (isGrabbingWall) return; // Non girarsi mentre si è appesi

        if (horizontalInput > 0 && !FacingRight)
        {
            FacingRight = true;
            myRend.flipX = false;
            FacingDirection = 1;
        }
        else if (horizontalInput < 0 && FacingRight)
        {
            FacingRight = false;
            myRend.flipX = true;
            FacingDirection = -1;
        }
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
        if (anim == null || rb == null) return;
        anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool("IsGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("isGrabbingWall", isGrabbingWall);
        anim.SetBool("IsPushing", isPushing);
    }

    public void Die() => Destroy(gameObject);
    void PlayJumpSound() 
    { 
        if (audioSource != null && jumpSound != null) audioSource.PlayOneShot(jumpSound); 
    }
    void PlayMeow() 
    { 
        if (audioSource != null && meow != null) audioSource.PlayOneShot(meow); 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Snack"))
        {
            if (audioSource != null && snackCollectSound != null) audioSource.PlayOneShot(snackCollectSound);
        }
    }
}