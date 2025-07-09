using System;
using UnityEngine;

public class GameSetup : MonoBehaviour
{
    int redBallsRemaining = 7;
    int blueBallsRemaining = 7;
    float ballRadius;
    float ballDiameter;

    [SerializeField] GameObject ballPrefab;
    [SerializeField] Transform cueBallPosition;
    [SerializeField] Transform headBallPosition;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        ballRadius = ballPrefab.GetComponent<SphereCollider>().radius * 1.5f;
        ballDiameter = ballRadius * 2f;
        PlaceAllBalls();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void PlaceAllBalls()
    {
        PlaceCueBall();
        PlaceRandomBalls();
    }

    void PlaceCueBall()
    {
        GameObject ball = Instantiate(ballPrefab, cueBallPosition.position, Quaternion.identity);
        ball.GetComponent<Ball>().MakeCueBall();
    }

    void PlaceEightBall(Vector3 position)
    {
        GameObject ball = Instantiate(ballPrefab, position, Quaternion.identity);
        ball.GetComponent<Ball>().MakeEightBall();
    }

    void PlaceRandomBalls()
    {
        int NumInThisRow = 1;
        int rand;
        Vector3 firstInRowPosition = headBallPosition.position;
        Vector3 currentPosition = firstInRowPosition;

        void PlaceRedBall(Vector3 position)
        {
            GameObject ball = Instantiate(ballPrefab, position, Quaternion.identity);
            ball.GetComponent<Ball>().BallSetup(true);
            redBallsRemaining--;
        }

        void PlaceBlueBall(Vector3 position)
        {
            GameObject ball = Instantiate(ballPrefab, position, Quaternion.identity);
            ball.GetComponent<Ball>().BallSetup(false);
            blueBallsRemaining--;
        }

        // 5 rows of balls, tip at headBallPosition (on the right), base to the left
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < NumInThisRow; j++)
            {
                if (i == 2 && j == 1)
                {
                    PlaceEightBall(currentPosition);
                }
                else if (redBallsRemaining > 0 && blueBallsRemaining > 0)
                {
                    rand = UnityEngine.Random.Range(0, 2);
                    if (rand == 0)
                    {
                        PlaceRedBall(currentPosition);
                    }
                    else
                    {
                        PlaceBlueBall(currentPosition);
                    }
                }
                else if (redBallsRemaining > 0)
                {
                    PlaceRedBall(currentPosition);
                }
                else
                {
                    PlaceBlueBall(currentPosition);
                }

                // Move down for each ball in the row (z-axis)
                currentPosition += Vector3.back * ballDiameter;
            }

            // Move up and left to start the next row
            firstInRowPosition += Vector3.forward * ballRadius + Vector3.left * (Mathf.Sqrt(3) * ballRadius);
            currentPosition = firstInRowPosition;
            NumInThisRow++;
        }
    }
}
