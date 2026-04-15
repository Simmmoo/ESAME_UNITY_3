using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private float horizontalInput;

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
    public LayerMask pushableLayer; // Layer per gli oggetti spostabili

    [Header("Push Mechanics")]
    public bool isPushing;
    private GameObject objectToPush; // Oggetto da spostare

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
        horizontalInput = Input.GetAxisRaw("Horizontal");
        Flip();
        HandleMovement();
        CheckGroundAndWall();
        CheckForPushableObject();
        HandleJump();
        HandleAnimation();

        if (Input.GetKeyDown(KeyCode.E) && isObjectDetected)
        {
            PushObject();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            PlayMeow();
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

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                PlayJumpSound(); // Suono del salto normale
            }
            else if (isGrabbingWall)
            {
                isGrabbingWall = false;

                rb.gravityScale = 1;

                int jumpDirection = -FacingDirection;
                rb.linearVelocity = new Vector2(jumpDirection * wallJumpHorizontalForce, wallJumpForce);
                PlayJumpSound(); // Suono anche per wall jump
            }
        }
    }

    void CheckGroundAndWall()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround | pushableLayer);
        isWallDetected = Physics2D.Raycast(transform.position, Vector2.right * FacingDirection, wallCheckDistance, wallLayer);
        isGrabbingWall = isWallDetected && !isGrounded && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D));
    }

    void CheckForPushableObject()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            new Vector2(transform.position.x, transform.position.y - 0.2f),
            Vector2.right * FacingDirection,
            objectCheckDistance,
            pushableLayer // Controlla solo il Layer "PushableObjects"
        );

        isObjectDetected = hit.collider != null;

        if (isObjectDetected)
        {
            objectToPush = hit.collider.gameObject;
        }
        else
        {
            objectToPush = null;
        }
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
        float duration = 0.2f; // Tempo in secondi per completare lo spostamento
        float elapsedTime = 0f; // Tempo trascorso dall'inizio del movimento

        Vector3 startPos = obj.transform.position; // Posizione iniziale
        Vector3 targetPos = startPos + new Vector3(direction, 0, 0); // Posizione finale (sposta di 1 metro)

        while (elapsedTime < duration)
        {
            // Interpola la posizione tra startPos e targetPos in base al tempo trascorso
            obj.transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / duration);

            elapsedTime += Time.deltaTime; // Aggiunge il tempo trascorso in questo frame
            yield return null; // Aspetta il frame successivo prima di continuare
        }

        obj.transform.position = targetPos; // Assicura che l'oggetto arrivi esattamente alla posizione finale
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

    private void OnDrawGizmos()
    {
        // Linea per il controllo del terreno
        Gizmos.color = Color.red;
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround) ||
             Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, pushableLayer);


        // Linea per il controllo della parete
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + wallCheckDistance * FacingDirection, transform.position.y));

        // Linea per il controllo degli oggetti
        Gizmos.color = Color.green;
        Vector2 startPosition = new Vector2(transform.position.x, transform.position.y - 0.2f);
        Vector2 endPosition = new Vector2(transform.position.x + objectCheckDistance * FacingDirection, transform.position.y - 0.2f);
        Gizmos.DrawLine(startPosition, endPosition);
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    void PlayJumpSound()
    {
        if (audioSource != null && jumpSound != null)
        {
            audioSource.PlayOneShot(jumpSound);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Snack"))
        {
            AddSnacks snackScript = collision.GetComponent<AddSnacks>();
            if (snackScript != null)
            {
                PlaySnackCollectSound();
            }
        }
    }

    private void PlaySnackCollectSound()
    {
        if (audioSource != null && snackCollectSound != null)
        {
            audioSource.PlayOneShot(snackCollectSound); 
        }
    }

    void PlayMeow()
    {
        if (audioSource != null && meow != null)
        {
            audioSource.PlayOneShot(meow);
        }
    }
}
