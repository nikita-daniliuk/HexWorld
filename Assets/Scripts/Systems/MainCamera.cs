using UnityEngine;
using Zenject;

public class MainCamera : MonoBehaviour
{
    [Inject] EventBus EventBus;

    [Header("Horizontal Movement / Rotation")]
    public float MoveSpeed = 10f;
    public float RotationSpeed = 5f;
    public float RotationSmoothSpeed = 5f;
    public float HorizontalSmoothSpeed = 5f;

    [Header("Vertical Movement")]
    public float MinDistance = 5f;
    public float MaxDistance = 50f;
    public float ScrollSpeed = 5f;
    public float VerticalSmoothSpeed = 5f;

    [Header("Raycast Settings")]
    public LayerMask CollisionMask;
    public float HeightThreshold = 0.1f;

    private Player Player;
    private Camera Cam;

    private Vector3 TargetPosition;
    private float TargetRotationY;
    private float CurrentDistance;
    private float BaseHeight = 0f;
    private float TargetBaseHeight = 0f;
    private float CurrentRotationY;

    private Vector3 LastMousePosition;
    private bool IsDragging;
    private bool IsRotating;

    void Awake()
    {
        EventBus.Subscribe(SignalBox);

        Cam = GetComponent<Camera>();
        TargetPosition = transform.position;
        TargetRotationY = transform.eulerAngles.y;
        CurrentDistance = (MaxDistance + MinDistance) / 2f;
    }

    void SignalBox(object Obj)
    {
        switch (Obj)
        {
            case PickUnitSignal PickUnitSignal :
                Player = PickUnitSignal.Unit as Player;
                TargetPosition = new Vector3(Player.transform.position.x, transform.position.y, Player.transform.position.z) - new Vector3(transform.forward.x, 0, transform.forward.z) * 15f;
                break;
            default: break;
        }
    }

    void Update()
    {
        if(!Player) return;

        HandleMouseInput();
        HandleZoom();
        UpdateSpringArm();
    }

    private void HandleMouseInput()
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

        if (Input.GetMouseButtonDown(1))
        {
            IsRotating = true;
            LastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(1))
        {
            IsRotating = false;
        }

        if (IsDragging)
        {
            Vector3 delta = Input.mousePosition - LastMousePosition;
            LastMousePosition = Input.mousePosition;

            float moveX = delta.x * MoveSpeed * Time.deltaTime;
            float moveZ = delta.y * MoveSpeed * Time.deltaTime;

            Vector3 right = transform.right;
            Vector3 forward = transform.forward;

            right.y = 0;
            forward.y = 0;

            right.Normalize();
            forward.Normalize();

            Vector3 moveDirection = (right * -moveX) + (forward * -moveZ);
            TargetPosition += moveDirection;
        }

        if (IsRotating)
        {
            Vector3 delta = Input.mousePosition - LastMousePosition;
            LastMousePosition = Input.mousePosition;

            TargetRotationY += delta.x * RotationSpeed * Time.fixedDeltaTime;
        }

        CurrentRotationY = Mathf.Lerp(CurrentRotationY, TargetRotationY, Time.fixedDeltaTime * RotationSmoothSpeed);
    }

    private void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        CurrentDistance -= scrollInput * ScrollSpeed;
        CurrentDistance = Mathf.Clamp(CurrentDistance, MinDistance, MaxDistance);
    }

    private void UpdateSpringArm()
    {
        Ray ray = Cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

        RaycastHit hit;
        float newHeight = Player ? Player.transform.position.y : 0f;

        if (Physics.Raycast(ray, out hit, 2000f, CollisionMask))
        {
            newHeight = Mathf.Max(hit.point.y, Player.transform.position.y);
        }

        if (Mathf.Abs(newHeight - TargetBaseHeight) > HeightThreshold)
        {
            TargetBaseHeight = newHeight;
        }

        BaseHeight = Mathf.Lerp(BaseHeight, TargetBaseHeight, Time.fixedDeltaTime * VerticalSmoothSpeed);

        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            new Vector3(TargetPosition.x, BaseHeight + CurrentDistance, TargetPosition.z),
            Time.fixedDeltaTime * HorizontalSmoothSpeed
        );
        transform.position = smoothedPosition;

        transform.rotation = Quaternion.Euler(60f, TargetRotationY, 0f);
    }
    
    void OnDestroy()
    {
        EventBus.Unsubscribe(SignalBox);
    }
}