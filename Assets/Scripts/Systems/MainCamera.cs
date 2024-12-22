using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public float MoveSpeed;
    public float ScrollSpeed;
    public float MinFOV;
    public float MaxFOV;
    public float RotationSpeed;

    private Vector3 LastMousePosition;
    private bool IsDragging;
    private bool IsRotating;
    private Camera Camera;

    private void Start()
    {
        Camera = GetComponent<Camera>();
    }

    private void Update()
    {
        HandleMouseDrag();
        ZoomCamera();
        RotateCamera();
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
            transform.position += MoveDirection;
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

            Vector3 CameraDirection = transform.forward;
            Vector3 CameraPosition = transform.position;

            float T = CameraPosition.y / -CameraDirection.y;
            Vector3 TargetPoint = CameraPosition + CameraDirection * T;

            transform.RotateAround(TargetPoint, Vector3.up, RotationAmount);
        }
    }
}