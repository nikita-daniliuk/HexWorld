using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public float MoveSpeed;
    public float ScrollSpeed;
    public float MinFOV;
    public float MaxFOV;
    public float RotationSpeed;
    public float SmoothTime = 0.1f; // Добавлено время сглаживания для движения и вращения

    private Vector3 LastMousePosition;
    private bool IsDragging;
    private bool IsRotating;
    private Camera Camera;

    private Vector3 TargetPosition;
    private Quaternion TargetRotation;
    private Vector3 Velocity = Vector3.zero;

    private void Start()
    {
        Camera = GetComponent<Camera>();
        TargetPosition = transform.position;
        TargetRotation = transform.rotation;
    }

    private void Update()
    {
        HandleMouseDrag();
        ZoomCamera();
        RotateCamera();
        SmoothMovement();
    }

    private void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            IsDragging = true;
            LastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            IsDragging = false;
        }

        if (IsDragging)
        {
            Vector3 Delta = Input.mousePosition - LastMousePosition;
            LastMousePosition = Input.mousePosition;

            float MoveX = Delta.x * MoveSpeed * Time.deltaTime;
            float MoveZ = Delta.y * MoveSpeed * Time.deltaTime;

            Vector3 Right = transform.right;
            Vector3 Forward = transform.forward;

            Right.y = 0;
            Forward.y = 0;

            Right.Normalize();
            Forward.Normalize();

            Vector3 MoveDirection = (Right * -MoveX) + (Forward * -MoveZ);
            TargetPosition += MoveDirection;
        }
    }

    private void ZoomCamera()
    {
        float Scroll = Input.GetAxis("Mouse ScrollWheel");

        Camera.fieldOfView -= Scroll * ScrollSpeed;
        Camera.fieldOfView = Mathf.Clamp(Camera.fieldOfView, MinFOV, MaxFOV);
    }

    private void RotateCamera()
    {
        if (Input.GetMouseButtonDown(1))
        {
            IsRotating = true;
            LastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(1))
        {
            IsRotating = false;
        }

        if (IsRotating)
        {
            Vector3 Delta = Input.mousePosition - LastMousePosition;
            LastMousePosition = Input.mousePosition;

            float RotationAmount = Delta.x * RotationSpeed * Time.deltaTime;

            TargetRotation = Quaternion.AngleAxis(RotationAmount, Vector3.up) * TargetRotation;
        }
    }

    private void SmoothMovement()
    {
        transform.position = Vector3.SmoothDamp(transform.position, TargetPosition, ref Velocity, SmoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, TargetRotation, SmoothTime);
    }
}