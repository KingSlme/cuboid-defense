using UnityEngine;
using Unity.Cinemachine;

public class CameraSystem : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _cinemachineCamera;

    [Header("Movement")]
    [SerializeField] private bool _enableKeyboardMovement = true;
    [SerializeField] private bool _enableDragPanning = true;
    private const float MOVE_SPEED = 100.0f;
    private const float DRAG_PAN_SPEED = 1.0f;

    [Header("Zoom Settings")]
    [SerializeField] [Tooltip("Amount of zoom per scroll")] [Range(1.0f, 5.0f)] private float _zoomAmount = 3.0f;
    [SerializeField] [Tooltip("Transition speed of the zoom")] [Range(10.0f, 50.0f)] private float _zoomSpeed = 10.0f;
    [SerializeField] [Range(10.0f, 100.0f)] private float _followOffsetMinY = 10.0f;
    [SerializeField] [Range(10.0f, 70.0f)] private float _followOffsetMaxY = 70.0f;

    [Header("Camera Bounds")]
    [SerializeField] private Vector3 _cameraOrigin = Vector3.zero; // rem
    [SerializeField] [Range(10.0f, 1000.0f)] private float _cameraBoundX = 100.0f;
    [SerializeField] [Range(10.0f, 1000.0f)] private float _cameraBoundZ = 100.0f;

    private CinemachineFollow _cinemachineFollow;
    private bool _isDragging = false;
    private Vector2 _lastMousePosition;
    private Vector3 _followOffset;

    private void Awake()
    {
        _cinemachineFollow = _cinemachineCamera.GetComponent<CinemachineFollow>();
    }

    private void Start()
    {
        InitializeCameraSystem();
    }

    private void Update()
    {
        HandleMovement();
        HandleYZoom();
    }

    private void OnDrawGizmos()
    { 
        Gizmos.color =  Color.green;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 5.0f); // Camera System Location
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(-_cinemachineCamera.GetComponent<CinemachineFollow>().FollowOffset / 2.0f, new Vector3(_cameraBoundX, 0.0f, _cameraBoundZ));
    }

    private void InitializeCameraSystem()
    {
        _followOffset = _cinemachineFollow.FollowOffset;
    }

    private void HandleMovement()
    {
        if (_enableKeyboardMovement)
            HandleKeyboardMovement();
        if (_enableDragPanning)
            HandleMovementDragPanning();
        ClampCamera();
    }

    private void HandleKeyboardMovement()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");
        Vector3 moveDirection = (inputX * _cinemachineCamera.transform.right + inputZ * _cinemachineCamera.transform.forward).normalized;
        moveDirection.y = 0.0f;
        transform.Translate(moveDirection * MOVE_SPEED * Time.deltaTime, Space.World);
    }

    private void HandleMovementDragPanning()
    {   
        Vector3 moveDirection = Vector3.zero;
        if (Input.GetMouseButtonDown(0))
        {
            _isDragging = true;
            _lastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
        }
        if (_isDragging)
        {
            Vector2 mouseMovementDelta = (Vector2)Input.mousePosition - _lastMousePosition;
            moveDirection = mouseMovementDelta.x * -DRAG_PAN_SPEED * _cinemachineCamera.transform.right + mouseMovementDelta.y * -DRAG_PAN_SPEED * _cinemachineCamera.transform.forward;
            _lastMousePosition = Input.mousePosition;
        }
        moveDirection.y = 0.0f;
        transform.Translate(moveDirection * MOVE_SPEED * Time.deltaTime, Space.World);
    }

    private void HandleYZoom()
    {
        if (Input.mouseScrollDelta.y > 0.0f)
            _followOffset.y -= _zoomAmount;
        if (Input.mouseScrollDelta.y < 0.0f)
            _followOffset.y += _zoomAmount;

        _followOffset.y = Mathf.Clamp(_followOffset.y, _followOffsetMinY, _followOffsetMaxY);
        _cinemachineFollow.FollowOffset = Vector3.Lerp(_cinemachineFollow.FollowOffset, _followOffset, _zoomSpeed * Time.deltaTime);
    }

    private void ClampCamera()
    {
        transform.position = new Vector3
        (
            Mathf.Clamp(transform.position.x, -_cinemachineFollow.FollowOffset.x / 2.0f - (_cameraBoundX / 2.0f), -_cinemachineFollow.FollowOffset.x / 2.0f + (_cameraBoundX / 2.0f)),
            transform.position.y,
            Mathf.Clamp(transform.position.z, -_cinemachineFollow.FollowOffset.z / 2.0f - (_cameraBoundZ / 2.0f), -_cinemachineFollow.FollowOffset.z / 2.0f + (_cameraBoundZ / 2.0f))
        );
    }
}
