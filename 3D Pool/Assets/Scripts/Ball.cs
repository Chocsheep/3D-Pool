using UnityEngine;

public class Ball : MonoBehaviour
{
    private bool isRed;
    private bool is8ball = false;
    private bool isCueBall = false;

    Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        if (rb.linearVelocity.magnitude < 0.02f)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        else
        {
            Vector3 newVelocity = rb.linearVelocity;
            newVelocity.y = 0f;
            rb.linearVelocity = newVelocity;
        }
    }

    public bool IsBallRed()
    {
        return isRed;
    }

    public bool IsClueBall()
    {
        return isCueBall;
    }

    public bool IsEightBall()
    {
        return is8ball;
    }


    public void BallSetup(bool red)
    {
        isRed = red;
        if (isRed)
        {
            GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.blue;
        }
    }

    public void MakeCueBall()
    {
        isCueBall = true;
    }

    public void MakeEightBall()
    {
        is8ball = true;
        GetComponent<Renderer>().material.color = Color.black;
    }

    void OnCollisionEnter(Collision collision)
    {
        Ball self = GetComponent<Ball>();
        if (!self.IsClueBall()) return;

        // Register cushion or pocket contact
        if (collision.collider.CompareTag("Cushion"))
        {
            FindFirstObjectByType<GameManager>().RegisterCushionOrPocket();
        }

        // If we hit another ball
        if (collision.gameObject.CompareTag("Ball"))
        {
            Ball other = collision.gameObject.GetComponent<Ball>();
            if (other != null && !other.IsClueBall())
            {
                // Foul detection: wrong ball first
                FindFirstObjectByType<GameManager>().RegisterFirstHit(collision.gameObject);

                // Cue ball made valid contact
                FindFirstObjectByType<GameManager>().cueBallHitOtherBall = true;
            }
        }
    }
}
