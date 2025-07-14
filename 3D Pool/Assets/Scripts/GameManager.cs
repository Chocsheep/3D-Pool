using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    enum CurrentPlayer
    {
        Player1,
        Player2
    }

    CurrentPlayer currentPlayer;
    bool isWinningShotForPlayer1 = false;
    bool isWinningShotForPlayer2 = false;
    int player1BallsRemaining = 7;
    int player2BallsRemaining = 7;
    bool isWaitingForBallMovementToStop = false;
    bool willSwapPlayers = false;
    bool isGameOver = false;
    bool ballPocketed = false;
    [SerializeField] float shotTimer = 3f;
    private float currentTimer;
    [SerializeField] float movementThreshold;
    [SerializeField] TextMeshProUGUI player1BallsText;
    [SerializeField] TextMeshProUGUI player2BallsText;
    [SerializeField] TextMeshProUGUI currentTurnText;
    [SerializeField] TextMeshProUGUI messageText;

    [SerializeField] GameObject restartButton;

    [SerializeField] Transform headPosition;

    [SerializeField] Camera cueStickCamera;
    [SerializeField] Camera overheadCamera;
    Camera currentCamera;
    bool isBallInHand = false;
    GameObject cueBall;
    int defaultCueBallLayer;
    private bool isCueBallPlaced = false;
    private bool DidBallHitCushionOrPocket = false;
    private bool breakingShot = false;
    private bool firstHit = true;
    private bool cueBallShotInProgress = false;
    public bool cueBallHitOtherBall = false;
    [SerializeField] Transform redBallPanel;
    [SerializeField] Transform blueBallPanel;
    [SerializeField] GameObject ballIconPrefab;

    private List<GameObject> redIcons = new List<GameObject>();
    private List<GameObject> blueIcons = new List<GameObject>();
    [SerializeField] Transform currentPlayerPanel;







    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentPlayer = CurrentPlayer.Player1;
        currentCamera = cueStickCamera;
        currentTimer = shotTimer;

        InitBallIcons();
        breakingShot = true;
        DidBallHitCushionOrPocket = false;

        currentPlayer = CurrentPlayer.Player1;
        currentCamera = cueStickCamera;
        currentTimer = shotTimer;

        foreach (GameObject ball in GameObject.FindGameObjectsWithTag("Ball"))
        {
            if (ball.GetComponent<Ball>().IsClueBall())
            {
                cueBall = ball;
                break;
            }
        }
        defaultCueBallLayer = cueBall.layer;
    }
    void InitBallIcons()
{
    for (int i = 0; i < 7; i++)
    {
        GameObject redIcon = Instantiate(ballIconPrefab, redBallPanel);
        redIcons.Add(redIcon);

        GameObject blueIcon = Instantiate(ballIconPrefab, blueBallPanel);
        blueIcon.GetComponent<Image>().color = Color.blue;
        blueIcons.Add(blueIcon);
    }
}


    // Update is called once per frame
    void Update()
    {
        // Step 1: Handle Ball-in-Hand placement
        if (isBallInHand)
        {
            HandleBallInHand();
            return;
        }

        // Step 2: Handle post-shot waiting logic
        if (isWaitingForBallMovementToStop && !isGameOver)
        {
            // Wait for the currentTimer buffer (e.g. 3 seconds)
            currentTimer -= Time.deltaTime;
            if (currentTimer > 0)
            {
                return;
            }

            // Check if all balls have stopped
            bool allStopped = true;
            foreach (GameObject ball in GameObject.FindGameObjectsWithTag("Ball"))
            {
                if (ball.GetComponent<Rigidbody>().linearVelocity.magnitude >= movementThreshold)
                {
                    allStopped = false;
                    break;
                }
            }

            if (allStopped)
            {
                if (cueBallShotInProgress)
                {
                    if (!cueBallHitOtherBall)
                    {
                        UnityEngine.Debug.Log("Foul: Cue ball did not hit any other ball");
                        HandleFoul();
                    }

                    cueBallShotInProgress = false;
                }

                firstHit = true; // Reset first hit for the next player
                CheckCushionFoul();
                isWaitingForBallMovementToStop = false;

                // Only skip to next player if necessary
                if (willSwapPlayers || !ballPocketed)
                {
                    NextPlayerTurn();
                }
                else
                {
                    SwitchCameras();
                }

                currentTimer = shotTimer;
                ballPocketed = false;
            }
        }
    }


    void HandleBallInHand()
    {
        cueBall.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        cueStickCamera.enabled = false;
        overheadCamera.enabled = true;
        currentCamera = overheadCamera;
        if (Input.GetMouseButton(0))
        {
            Ray ray = overheadCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f))
            {
                Vector3 newPosition = new Vector3(hit.point.x, cueBall.transform.position.y, hit.point.z);

                // Check for overlaps with other balls (excluding cue ball itself)
                Collider[] overlaps = Physics.OverlapSphere(newPosition, 0.05f);
                bool validPosition = true;

                foreach (var col in overlaps)
                {
                    if (col.gameObject != cueBall && col.CompareTag("Ball"))
                    {
                        validPosition = false;
                        break;
                    }
                }

                if (validPosition)
                {
                    cueBall.transform.position = newPosition;
                    cueBall.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                    cueBall.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    isCueBallPlaced = true;
                }
            }
        }

        if (Input.GetMouseButtonDown(1) && isCueBallPlaced) // Right-click to confirm
        {
            cueStickCamera.gameObject.SetActive(true);
            isBallInHand = false;
            isCueBallPlaced = false;
            messageText.gameObject.SetActive(false);
            cueBall.layer = defaultCueBallLayer;

            // Restore collisions
            cueBall.layer = defaultCueBallLayer;
            cueBall.GetComponent<Collider>().isTrigger = false;
            cueBall.GetComponent<Rigidbody>().useGravity = true;

            // Switch back to cue camera and start waiting for shot
            currentCamera = overheadCamera; // so SwitchCameras knows what to toggle
            SwitchCameras();
            firstHit = true;

            currentTimer = shotTimer; // restart movement check delay
        }
    }



    public void HandleFoul()
    {
        cueStickCamera.gameObject.SetActive(false);
        DidBallHitCushionOrPocket = false;

        // Switch players
        NextPlayerTurn();

        isBallInHand = true;
        isWaitingForBallMovementToStop = false;
        cueBall.GetComponent<Rigidbody>().useGravity = false; // Disable gravity

        cueBall.layer = LayerMask.NameToLayer("GhostBall"); // Disable collisions
        cueBall.GetComponent<Collider>().isTrigger = true;

        messageText.gameObject.SetActive(true);
        messageText.text = "Foul! Move the cue ball anywhere. Right-click to confirm.";
    }




    public void SwitchCameras()
    {
        if (currentCamera == cueStickCamera)
        {
            cueStickCamera.enabled = false;
            overheadCamera.enabled = true;
            currentCamera = overheadCamera;
            isWaitingForBallMovementToStop = true;
        }
        else
        {
            cueStickCamera.enabled = true;
            overheadCamera.enabled = false;
            currentCamera = cueStickCamera;
            currentCamera.gameObject.GetComponent<CameraController>().ResetCamera();
        }
    }
    public void RestartTheGame()
    {
        SceneManager.LoadScene(1);
    }

    bool Scratch()
    {
        if (currentPlayer == CurrentPlayer.Player1)
        {
            if (isWinningShotForPlayer1)
            {
                ScratchOnWinningShot("Player 1");
                return true;
            }
        }
        else
        {
            if (isWinningShotForPlayer2)
            {
                ScratchOnWinningShot("Player 2");
                return true;
            }
        }
        willSwapPlayers = true;
        HandleFoul();
        SwitchCameras();
        return false;
    }

    void EarlyEightBall()
    {
        if (currentPlayer == CurrentPlayer.Player1)
        {
            Lose("Player 1 Hit The Eight Ball in Too Early and Has Lost!");
        }
        else
        {
            Lose("Player 2 Hit The Eight Ball in Too Early and Has Lost!");
        }
    }

    void ScratchOnWinningShot(string player)
    {
        Lose(player + " Scratched on Winning Shot and Has Lost!");
    }

    bool CheckBall(Ball ball)
    {
        if (ball.IsClueBall())
        {
            if (Scratch())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (ball.IsEightBall())
        {
            if (currentPlayer == CurrentPlayer.Player1)
            {
                if (isWinningShotForPlayer1)
                {
                    Win("Player 1");
                    return true;
                }
            }
            else
            {
                if (isWinningShotForPlayer2)
                {
                    Win("Player 2");
                    return true;
                }
            }
            EarlyEightBall();
        }
        else
        {
            // If not Eight or Cue Ball, it is a red/blue ball
            if (ball.IsBallRed())
            {
                player1BallsRemaining--;


                // Hide one red icon
                if (player1BallsRemaining >= 0 && player1BallsRemaining < redIcons.Count)
                    redIcons[player1BallsRemaining].SetActive(false);

                if (player1BallsRemaining <= 0)
                    isWinningShotForPlayer1 = true;

                if (currentPlayer != CurrentPlayer.Player1)
                    willSwapPlayers = true;
            }
            else
            {
                player2BallsRemaining--;


                // Hide one blue icon
                if (player2BallsRemaining >= 0)
                    blueIcons[player2BallsRemaining].SetActive(false);

                if (player2BallsRemaining <= 0)
                    isWinningShotForPlayer2 = true;

                if (currentPlayer != CurrentPlayer.Player2)
                    willSwapPlayers = true;
            }
        }
        return true;
    }

    void Lose(string message)
    {
        isGameOver = true;
        messageText.gameObject.SetActive(true);
        messageText.text = message;
        restartButton.SetActive(true);
    }

    void Win(string player)
    {
        isGameOver = true;
        messageText.gameObject.SetActive(true);
        messageText.text = player + " Has Won!";
        restartButton.SetActive(true);
    }

    void NextPlayerTurn()
    {
        // Reset foul tracking at the start of the turn
        DidBallHitCushionOrPocket = false;

        if (currentPlayer == CurrentPlayer.Player1)
        {
            currentPlayer = CurrentPlayer.Player2;
            currentTurnText.text = "Current Turn: Player 2";
            currentPlayerPanel.GetComponent<Image>().color = new Color32(0, 0, 255, 200);
        }
        else
        {
            currentPlayer = CurrentPlayer.Player1;
            currentTurnText.text = "Current Turn: Player 1";
            currentPlayerPanel.GetComponent<Image>().color = new Color32(255, 0, 0, 200);
        }
        willSwapPlayers = false;
        SwitchCameras();
        UnityEngine.Debug.Log("Player's Turn: " + currentPlayer);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Ball")
        {
            ballPocketed = true;
            UnityEngine.Debug.Log("Ball Pocketed: " + other.gameObject.name);
            RegisterCushionOrPocket();
            if (CheckBall(other.gameObject.GetComponent<Ball>()))
            {
                Destroy(other.gameObject);
            }
            else
            {
                other.gameObject.transform.position = headPosition.position;
                other.gameObject.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                other.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            }
        }
    }

    public void RegisterFirstHit(GameObject hitBall)
    {
        Ball ball = hitBall.GetComponent<Ball>();

        if (ball == null || !firstHit)
        {
            return;
        }

        if (breakingShot)
        {
            breakingShot = false;
            firstHit = false;
            return;
        }

        bool foul = false;

        if (ball.IsEightBall())
        {
        if ((currentPlayer == CurrentPlayer.Player1 && !isWinningShotForPlayer1) ||
                (currentPlayer == CurrentPlayer.Player2 && !isWinningShotForPlayer2))
            {
                UnityEngine.Debug.Log("Foul: Hit the 8-ball too early");
                foul = true;
            }
        // Else: valid 8-ball shot (on winning shot), no foul
        }
        else if (ball.IsBallRed() && currentPlayer != CurrentPlayer.Player1)
        {
            // Player 2 hit a red ball first (wrong group)
            foul = true;
        }
        else if (!ball.IsBallRed() && !ball.IsEightBall() && !ball.IsClueBall() && currentPlayer != CurrentPlayer.Player2)
        {
            // Player 1 hit a blue ball first (wrong group)
            foul = true;
        }

        else if (ball.IsEightBall())
        {
            // Already handled above, but double-check for safety
            if ((currentPlayer == CurrentPlayer.Player1 && !isWinningShotForPlayer1) ||
            (currentPlayer == CurrentPlayer.Player2 && !isWinningShotForPlayer2))
            {
            foul = true;
            }
        }
        // You could add more cases here for custom rules, e.g. hitting no ball at all, etc.

        if (foul)
        {
            UnityEngine.Debug.Log("Foul: Wrong ball hit first");
            HandleFoul();
        }
        firstHit = false;
    }



    public void RegisterCushionOrPocket()
    {
        DidBallHitCushionOrPocket = true;
    }

    void CheckCushionFoul()
    {
        if (!DidBallHitCushionOrPocket)
        {
            UnityEngine.Debug.Log("Foul: No ball hit a cushion or pocket");
            HandleFoul();
        }
    }
    public void CueBallShotStarted()
    {
        cueBallShotInProgress = true;
        cueBallHitOtherBall = false;
    }


}
