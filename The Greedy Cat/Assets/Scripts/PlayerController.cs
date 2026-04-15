using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private float horizontalInput;

    // --- NUOVE VARIABILI PER MOBILE ---
    private float mobileHorizontalInput;
    private bool mobileJumpPressed;
    // ----------------------------------

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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        myRend = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        // LOGICA IBRIDA: Tastiera + Mobile
        float keyboardInput = Input.GetAxisRaw("Horizontal");
        horizontalInput = (keyboardInput != 0) ? keyboardInput : mobileHorizontalInput;

        Flip();
        HandleMovement();
        CheckGroundAndWall();
        CheckForPushableObject();
        HandleJump();
        HandleAnimation();

        // Se rileva un oggetto mentre cammini, prova a spingerlo (ottimo per mobile)
        if (isObjectDetected && horizontalInput != 0 && !isPushing)
        {
            PushObject();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            PlayMeow();
        }
    }

    // --- FUNZIONI PUBBLICHE PER MOBILE ---
    public void MobileMove(float direction) => mobileHorizontalInput = direction;
    public void MobileJump() => mobileJumpPressed = true;
    public void MobileMeow() => PlayMeow();
    // -------------------------------------

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

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) || mobileJumpPressed)
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
            mobileJumpPressed = false; // Reset input mobile
        }
    }

    void CheckGroundAndWall()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround | pushableLayer);
        isWallDetected = Physics2D.Raycast(transform.position, Vector2.right * FacingDirection, wallCheckDistance, wallLayer);

        // Accetta input sia da tastiera che da touch per il wall grab
        isGrabbingWall = isWallDetected && !isGrounded && (horizontalInput != 0);
    }

    void CheckForPushableObject()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            new Vector2(transform.position.x, transform.position.y - 0.2f),
            Vector2.right * FacingDirection,
            objectCheckDistance,
            pushableLayer
        );

        isObjectDetected = hit.collider != null;
        objectToPush = isObjectDetected ? hit.collider.gameObject : null;
    }

    void PushObject()
    {
        if (objectToPush != null)
        {
            isPushing = true;
            if (audioSource != null && pushSound != null)
            {
                audioSource.PlayOneShot(pushSound);
            }
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
            if (obj == null) yield break; // Sicurezza se l'oggetto viene distrutto
            obj.transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (obj != null) obj.transform.position = targetPos;
        isPushing = false;
    }

    void HandleAnimation()
    {
        if (anim == null) return;
        anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool("IsGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("isGrabbingWall", isGrabbingWall);
        anim.SetBool("IsPushing", isPushing);
    }

    public void Die() => Destroy(gameObject);

    void PlayJumpSound()
    {
        if (audioSource != null && jumpSound != null)
            audioSource.PlayOneShot(jumpSound);
    }

    void PlayMeow()
    {
        if (audioSource != null && meow != null)
            audioSource.PlayOneShot(meow);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Snack"))
        {
            if (audioSource != null && snackCollectSound != null)
                audioSource.PlayOneShot(snackCollectSound);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector2.down * groundCheckDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, Vector2.right * FacingDirection * wallCheckDistance);
    }
}