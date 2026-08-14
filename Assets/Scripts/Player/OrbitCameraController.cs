using UnityEngine;

// 【玩家视角】第三人称自由轨道相机，实现鼠标无缝旋转视角、锁屏及跟随玩家
public class OrbitCameraController : MonoBehaviour
{
    [Header("跟随目标 (Player)")]
    public Transform target;

    [Header("第三人称视角设置")]
    public float distance = 5.0f;          // 相机距离玩家的距离
    public float targetHeight = 1.5f;      // 相机看向玩家的高度（胸部/头部）
    public float mouseSensitivity = 3.0f;  // 鼠标灵敏度
    public float minVerticalAngle = -20f;  // 最小俯角
    public float maxVerticalAngle = 70f;   // 最大仰角

    private float currentX = 0.0f;
    private float currentY = 20.0f;

    private void Start()
    {
        if (target != null)
        {
            Vector3 angles = transform.eulerAngles;
            currentX = angles.y;
            currentY = angles.x;
        }

        LockCursor();
    }

    private void Update()
    {
        // 按 ESC 键临时解锁光标，点击画面重新锁定
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursor();
        }

        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            LockCursor();
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 鼠标直接控制视角旋转
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            currentX += Input.GetAxis("Mouse X") * mouseSensitivity;
            currentY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
        }

        // 计算相机的旋转与位置
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 targetPos = target.position + Vector3.up * targetHeight;
        Vector3 position = targetPos - (rotation * Vector3.forward * distance);

        transform.rotation = rotation;
        transform.position = position;
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}