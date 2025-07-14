using System.Numerics;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    [SerializeField] float rotationSpeed;
    [SerializeField] UnityEngine.Vector3 offset;
    [SerializeField] float downAngle;
    [SerializeField] float power;
    [SerializeField] GameObject cueStick;
    private float horizontalInput;
    private bool isTakingShot = false;
    [SerializeField] float maxDrawDistance;
    private float savedMousePosition;

    Transform cueBall;
    GameManager gameManager;
    [SerializeField] TMPro.TextMeshProUGUI powerText;
    [SerializeField] Slider powerBarSlider;
    [SerializeField] RectTransform powerBarTransform;
    [SerializeField] float shakeThreshold = 95f;
    [SerializeField] float shakeAmount = 3f;
    [SerializeField] float shakeSpeed = 20f;

    UnityEngine.Vector3 originalPowerBarPos;
    [SerializeField] Gradient powerGradient;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalPowerBarPos = powerBarTransform.anchoredPosition;
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        foreach (GameObject ball in GameObject.FindGameObjectsWithTag("Ball"))
        {
            if (ball.GetComponent<Ball>().IsClueBall())
            {
                cueBall = ball.transform;
                break;
            }
        }

        ResetCamera();
    }

    // Update is called once per frame
    void Update()
    {
        if (cueBall != null && !isTakingShot)
        {
            horizontalInput = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;

            transform.RotateAround(cueBall.position, UnityEngine.Vector3.up, horizontalInput);
        }
        Shoot();
    }

    public void ResetCamera()
    {
        cueStick.SetActive(true);
        transform.position = cueBall.position + offset;
        transform.LookAt(cueBall.position);
        transform.localEulerAngles = new UnityEngine.Vector3(downAngle, transform.localEulerAngles.y, 0);
    }

    void Shoot()
    {
        if (gameObject.GetComponent<Camera>().enabled)
        {
            if (Input.GetButtonDown("Fire1") && !isTakingShot)
            {
                isTakingShot = true;
                powerBarSlider.gameObject.SetActive(isTakingShot);
                savedMousePosition = 0f;
            }
            else if (isTakingShot)
            {
                if (savedMousePosition + Input.GetAxis("Mouse Y") <= 0)
                {
                    savedMousePosition += Input.GetAxis("Mouse Y");
                    if (savedMousePosition <= maxDrawDistance)
                    {
                        savedMousePosition = maxDrawDistance;
                    }
                    float powerValueNumber = ((savedMousePosition - 0) / (maxDrawDistance - 0)) * (100 - 0) + 0;
                    powerValueNumber = Mathf.Clamp(powerValueNumber, 0f, 100f); // prevent overshoot

                    powerBarSlider.value = powerValueNumber;
                    powerBarSlider.fillRect.GetComponent<Image>().color = powerGradient.Evaluate(powerValueNumber / 100f);

                    // Shake when near max
                    if (powerValueNumber >= shakeThreshold)
                    {
                        float shakeOffset = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
                        powerBarTransform.anchoredPosition = originalPowerBarPos + new UnityEngine.Vector3(shakeOffset, 0f, 0f);
                    }
                    else
                    {
                        powerBarTransform.anchoredPosition = originalPowerBarPos;
                    }

                }

                if (Input.GetButtonDown("Fire1"))
                {
                    UnityEngine.Vector3 hitDirection = transform.forward;
                    hitDirection = new UnityEngine.Vector3(hitDirection.x, 0, hitDirection.z).normalized;

                    cueBall.gameObject.GetComponent<Rigidbody>().AddForce(hitDirection * power * Mathf.Abs(savedMousePosition), ForceMode.Impulse);
                    FindFirstObjectByType<GameManager>().CueBallShotStarted();
                    cueStick.SetActive(false);
                    gameManager.SwitchCameras();
                    isTakingShot = false;
                    powerBarSlider.gameObject.SetActive(false);
                }
            }
        }
    }
}

