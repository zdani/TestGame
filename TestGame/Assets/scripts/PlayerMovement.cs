using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    Rigidbody2D body;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
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

        // Horizontal movement
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            var xscale = this.transform.localScale.x > 0.01 ? 1   : - 1;
            this.transform.localScale = new Vector3(1, this.transform.localScale.y);
            velocity.x = 5;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            var xscale = this.transform.localScale.x > 0.01 ? -1 : 1;
            this.transform.localScale = new Vector3(-1, this.transform.localScale.y);
            velocity.x = -5;
        }
        else
            velocity.x = 0;

        body.linearVelocity = velocity;
    }
}
