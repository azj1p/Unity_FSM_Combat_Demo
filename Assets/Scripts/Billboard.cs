using UnityEngine;
//血条韧性条面向镜头
// 【通用工具】UI 看板组件，使 3D 场景中的 Canvas/UI 始终保持绝对面向主摄像机
public class Billboard : MonoBehaviour
{
    private Transform mainCameraTransform;

    private void Start()
    {
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    // 使用 LateUpdate 确保在相机位置/角度更新完成后，再调整 UI 的朝向
    private void LateUpdate()
    {
        if (mainCameraTransform == null)
        {
            if (Camera.main != null)
            {
                mainCameraTransform = Camera.main.transform;
            }
            return;
        }

        // 让当前 UI 的旋转方向始终与摄像机的旋转绝对一致（平面面向屏幕）
        transform.rotation = mainCameraTransform.rotation;
    }
}