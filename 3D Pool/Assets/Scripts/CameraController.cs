using System.Numerics;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [SerializeField] float rotationSpeed;
    [SerializeField] UnityEngine.Vector3 offset;
    [SerializeField] float downAngle;
    [SerializeField] float power;
    private float horizontalInput;

    Transform cueBall;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
        if (cueBall != null)
        {
            horizontalInput = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;

            transform.RotateAround(cueBall.position, UnityEngine.Vector3.up, horizontalInput);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetCamera();
        }

        if (Input.GetButtonDown("Fire1"))
        {
            UnityEngine.Vector3 hitDirection = transform.forward;
            hitDirection = new UnityEngine.Vector3(hitDirection.x, 0, hitDirection.z).normalized;

            cueBall.gameObject.GetComponent<Rigidbody>().AddForce(hitDirection * power, ForceMode.Impulse);
        }
    }

    public void ResetCamera()
    {
        transform.position = cueBall.position + offset;
        transform.LookAt(cueBall.position);
        transform.localEulerAngles = new UnityEngine.Vector3(downAngle, transform.localEulerAngles.y, 0);
    }
}
