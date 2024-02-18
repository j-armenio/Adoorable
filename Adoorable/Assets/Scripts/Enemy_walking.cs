using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_walking : StateMachineBehaviour
{
    [SerializeField] LayerMask playerMask;
    private GameObject player;
    private GameObject hammer;
    private Rigidbody2D rbPlayer;
    private Rigidbody2D rb;
    private BoxCollider2D bxcoll;
    private BoxCollider2D hammerbxcoll;
    private Enemy enemy;

    private float speed = 2.5f;
    private float attackRange = 1.5f;
    private int dano = 0;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player");
        hammer = GameObject.FindGameObjectWithTag("Hammer");

        rbPlayer = player.GetComponent<Rigidbody2D>();

        rb = animator.GetComponent<Rigidbody2D>();
        bxcoll = animator.GetComponent<BoxCollider2D>();
        hammerbxcoll = hammer.GetComponent<BoxCollider2D>();
        enemy = animator.GetComponent<Enemy>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemy.LookAtPlayer();

        Vector2 target = new(rbPlayer.position.x, rb.position.y);
        Vector2 newpos = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
        rb.MovePosition(newpos);

        // Hit the player if he's too close
        if (IsCollidingPlayer(Vector2.left) || IsCollidingPlayer(Vector2.right))
        {
            animator.SetTrigger("hit");
        }

        if (IsPlayerHitting() && bxcoll.IsTouching(hammerbxcoll))
        {
            // enemy take damage
            dano++;
            Debug.Log("enemy: " + dano);
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger("hit");
    }

    private bool IsCollidingPlayer(Vector2 direction)
    {
        return Physics2D.BoxCast(bxcoll.bounds.center, bxcoll.bounds.size, 0f, direction, attackRange, playerMask);
    }

    private bool IsPlayerHitting()
    {
        return player.GetComponent<Animator>().GetBool("hit");
    }
}
