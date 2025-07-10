using System;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    Rigidbody2D body;
    public Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.freezeRotation = true; // Prevent tipping over
    }

    // Update is called once per frame
    void Update()
    {
        
        Vector2 velocity = body.linearVelocity;

        // Jumping
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)) && Mathf.Abs(body.linearVelocity.y) < 0.01f)
        {
            velocity.y = 7f; // Jump force
        }
        else
        {
            // Only apply gravity if not jumping
            velocity.y += Physics2D.gravity.y * Time.deltaTime;
        }

        var speed = 0;
        // Horizontal movement
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            // Flip scale to face right
            this.transform.localScale = new Vector3(Mathf.Abs(this.transform.localScale.x) * 1, this.transform.localScale.y);
            speed = 5;
            
        }
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            // Flip scale to face left
            this.transform.localScale = new Vector3(Mathf.Abs(this.transform.localScale.x) * -1, this.transform.localScale.y);
            speed = -5;
        }
        
        velocity.x = speed;
        animator.SetFloat("speed", Math.Abs(speed));

        body.linearVelocity = velocity;
    }
}
