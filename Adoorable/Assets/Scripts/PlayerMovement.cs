using System;
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
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Sprite woodenDoor;
    GameObject hammer;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private LayerMask jumpableGround;
    [SerializeField] private Sprite ironDoor;
    [SerializeField] private TrailRenderer tr;
    [SerializeField] ParticleSystem dust;

    private enum MovementState
    { 
        idle,
        walking,
        jumping,
        falling,
        hitting,
        changeMat
    }
    private enum DoorState { wood, iron, glass}
    private DoorState doorType = DoorState.wood;

    private const float dashingPower = 24f;
    private const float dashingTime = 0.2f;
    private const float dashingCooldown = 0.3f;
    private const float ironNerf = 0.4f;
    private const float moveSpeed = 7f;
    private const float jumpForce = 7f;
    private const int maxJumps = 2;
    private const int maxDashs = 1;
    
    private float dirX = 0f;
    private float prevDirX = 0f;
    private float facingRight = 0f;
    private int jumps = 0;
    private int dashs = 0;
    private bool changeMat = false;
    private bool isIronDoor = false;
    private bool hit = false;
    private bool canDash = true;
    private bool isDashing = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        player = GetComponent<Transform>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        woodenDoor = GetComponent<Sprite>();
        hammer = GameObject.FindGameObjectWithTag("Hammer");

        jumps = maxJumps;
        dashs = maxDashs;
    }

    private void Update()
    {
        if (isDashing)
            return;

        if (dirX != 0)
        {
            facingRight = dirX;
            prevDirX = dirX;
        }

        dirX = Input.GetAxisRaw("Horizontal");

        if (IsGrounded() && dirX != 0 && dirX != prevDirX)
            CreateDust();

        rb.velocity = doorType == DoorState.iron?
            new Vector2(dirX * moveSpeed * ironNerf, rb.velocity.y):
            new Vector2(dirX * moveSpeed, rb.velocity.y);

        if (IsGrounded() && rb.velocity.y < .1f)
        {
            jumps = maxJumps;
            dashs = maxDashs;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            spriteRenderer.sprite = doorType == DoorState.iron? ironDoor: woodenDoor;

            changeMat = true;
            isIronDoor = true;
        }

        if (Input.GetButtonDown("Jump") && jumps > 0)
        {
            if (IsGrounded())
                CreateDust();

            if (rb.velocity.y < .1f)
            {
                rb.velocity = doorType == DoorState.iron?
                    new Vector2(rb.velocity.x, jumpForce * ironNerf):
                    new Vector2(rb.velocity.x, jumpForce);
            }
            else
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            jumps--;
        }

        if (Input.GetKeyDown(KeyCode.F))
            hit = true;

        if (Input.GetKeyDown(KeyCode.LeftShift) && doorType != DoorState.iron && dashs > 0 && canDash)
            StartCoroutine(Dash());

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
        MovementState prevState = (MovementState)anim.GetInteger("state");

        if (dirX > 0f) // Turn right
        {
            state = MovementState.walking;
            sprite.flipX = true;
            coll.offset = new Vector2(-Math.Abs(coll.offset.x), coll.offset.y);
            hammer.transform.position = new Vector2(player.position.x + 1.05f, hammer.transform.position.y);
            dust.transform.position = new Vector2(player.position.x - 1, dust.transform.position.y);
        }
        else if (dirX < 0f) // Turn left
        {
            state = MovementState.walking;
            sprite.flipX = false;
            coll.offset = new Vector2(Math.Abs(coll.offset.x), coll.offset.y);
            hammer.transform.position = new Vector2(player.position.x - 1.05f, hammer.transform.position.y);
            dust.transform.position = new Vector2(player.position.x + 1, dust.transform.position.y);
        } else
            state = MovementState.idle;

        // Jumping animation comes after because it has more priority
        if (rb.velocity.y > .1f)
            state = MovementState.jumping;
        else if (rb.velocity.y < -.1f)
            state = MovementState.falling;

        // Ended hitting animation
        if (IsAnimationFinished() && hit)
            hit = false;

        if (hit)
            state = MovementState.hitting;

        // Ended changing material animation
        if (changeMat && prevState == MovementState.changeMat && IsAnimationFinished())
        {
            changeMat = false;
            if (isIronDoor)
                doorType = (doorType == DoorState.wood) ? DoorState.iron : DoorState.wood;
            else
                doorType = (doorType == DoorState.wood) ? DoorState.glass : DoorState.wood;
            isIronDoor = false;
        }

        if (changeMat)
            state = MovementState.changeMat;

        anim.SetInteger("doorType", (int)doorType);
        anim.SetInteger("state", (int)state);
        anim.SetBool("hit", hit);
    }

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
    }

    private IEnumerator Dash()
    {
        dashs--;
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = doorType == DoorState.iron?
            new Vector2(transform.localScale.x * dashingPower * facingRight * ironNerf, 0f):
            new Vector2(transform.localScale.x * dashingPower * facingRight, 0f);
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


    
    private bool IsAnimationFinished()
    {
        return anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1;
    }
}
