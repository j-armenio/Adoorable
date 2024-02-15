using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private int maxJumps = 2;
    private float dirX = 0f;
    private int jumps = 0;
    private bool hit = false;


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
        dirX = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);

        if (IsGrounded() && rb.velocity.y < .1f)
        {
            jumps = maxJumps;
        }

        if (Input.GetButtonDown("Jump") && jumps > 0)
        {
            Debug.Log(jumps);
            if (rb.velocity.y > .1f) {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            } else
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
            //audioSource.Play();
            jumps--;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            hit = true;
        }

        UpdateAnimationState();
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

        // MovementState.hitting == 4
        if (IsAnimationFinished())
        {
            hit = false;
        }

        anim.SetInteger("state", (int)state);
    }

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
    }

    private bool IsAnimationFinished()
    {
        return anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1;
    }
}
