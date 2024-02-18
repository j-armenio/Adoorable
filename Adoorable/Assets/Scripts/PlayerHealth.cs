using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerMovement pm;
    private Animator anim;
    [SerializeField] private AudioSource audioSource;

    public int life = 10;

    void Start()
    {
        pm = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        life -= damage;
        Debug.Log(life);

        if (life <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        pm.enabled = false;
        rb.bodyType = RigidbodyType2D.Static;
        anim.SetTrigger("death");
        audioSource.Play();
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
