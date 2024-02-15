using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private SpriteRenderer sprite;
    private Animator anim;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private LayerMask jumpableGround;

    private enum MovementState { idle, walking, jumping, falling, hitting }

    private float dirX = 0f;
    private float prevDirX = 0f;
    private int jumps = 0;
    private bool hit = false;
    private float facingRight = 0f;

    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 24f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 0.3f;

    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private TrailRenderer tr;
    [SerializeField] ParticleSystem dust;

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        jumps = maxJumps;
    }

    // Update is called once per frame
    private void Update()
    {
        if (isDashing)
        {
            return;
        }

        if (dirX != 0)
            facingRight = dirX;

        prevDirX = dirX;
        dirX = Input.GetAxisRaw("Horizontal");
        if (dirX != 0 && prevDirX != dirX && IsGrounded())
        {
            CreateDust();
        }

        rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);

        if (IsGrounded())
        {
            jumps = maxJumps;
        }

        if (Input.GetButtonDown("Jump") && jumps > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            // audioSource.Play();
            jumps--;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            hit = true;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }

        UpdateAnimationState();
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            return;
        }
    }

    private void UpdateAnimationState()
    {
        MovementState state;

        if (dirX > 0f)
        {
            state = MovementState.walking;
            sprite.flipX = true;
        }
        else if (dirX < 0f)
        {
            state = MovementState.walking;
            sprite.flipX = false;
        } else
        {
            state = MovementState.idle;
        }

        // Jumping animation comes after because it has more priority
        if (rb.velocity.y > .1f)
        {
            state = MovementState.jumping;
        }
        else if (rb.velocity.y < -.1f)
        {
            state = MovementState.falling;
        }

        if (hit)
        {
            state = MovementState.hitting;
        }

        anim.SetInteger("state", (int)state);
    }

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
    }

    private IEnumerator Dash()
    {
        print("Dash");
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(transform.localScale.x * dashingPower * facingRight, 0f);
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);

        // stop dashing
        tr.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    private void CreateDust()
    {
        dust.Play();
    }
}
