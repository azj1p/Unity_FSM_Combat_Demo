using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitCameraController : MonoBehaviour
{
    [Header("Target & Distance")]
    public Transform target;
    public float distance = 5.0f;
    public float minDistance = 2.0f;
    public float maxDistance = 10.0f;

    [Header("Sensitivity & Smoothing (视角灵敏度)")]
    [Tooltip("鼠标水平旋转灵敏度")]
    public float xSpeed = 0.2f;
    [Tooltip("鼠标垂直旋转灵敏度")]
    public float ySpeed = 0.2f;

    [Header("Angle Limits (角度限制)")]
    public float yMinLimit = -20f;
    public float yMaxLimit = 75f;

    public bool clampHorizontal = false;
    public float xMinLimit = -90f;
    public float xMaxLimit = 90f;

    private float x = 0.0f;
    private float y = 0.0f;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 新输入系统读取每帧像素偏移（不再错误乘以 Time.deltaTime）
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            x += mouseDelta.x * xSpeed;
            y -= mouseDelta.y * ySpeed;
        }

        // 垂直仰角限制
        y = Mathf.Clamp(y, yMinLimit, yMaxLimit);

        if (clampHorizontal)
        {
            x = Mathf.Clamp(x, xMinLimit, xMaxLimit);
        }

        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * negDistance + target.position + Vector3.up * 1.5f;

        transform.rotation = rotation;
        transform.position = position;
    }
}