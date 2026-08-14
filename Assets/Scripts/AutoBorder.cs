using UnityEngine;

// 【通用工具】地面自动空气墙生成器，根据地面(Plane)实际尺寸自动贴合生成隐形碰撞挡板
[ExecuteAlways] // 允许在 Unity 编辑器模式下实时生效（无需点运行也能看到效果）
//自动空气墙
public class AutoBorder : MonoBehaviour
{
    [Header("空气墙参数")]
    public float wallHeight = 10f;     // 墙体高度（防止跳过去）
    public float wallThickness = 1f;   // 墙体厚度

    private void Update()
    {
        UpdateBorders();
    }

    private void UpdateBorders()
    {
        Renderer r = GetComponent<Renderer>();
        if (r == null) return;

        // 1. 实时获取地面的世界坐标中心点与实际尺寸
        Vector3 center = r.bounds.center;
        Vector3 size = r.bounds.size;

        // 2. 自动生成并更新东南西北 4 面空气墙
        SetWall("Wall_North", new Vector3(center.x, center.y + wallHeight / 2f, center.z + size.z / 2f), new Vector3(size.x, wallHeight, wallThickness));
        SetWall("Wall_South", new Vector3(center.x, center.y + wallHeight / 2f, center.z - size.z / 2f), new Vector3(size.x, wallHeight, wallThickness));
        SetWall("Wall_East", new Vector3(center.x + size.x / 2f, center.y + wallHeight / 2f, center.z), new Vector3(wallThickness, wallHeight, size.z));
        SetWall("Wall_West", new Vector3(center.x - size.x / 2f, center.y + wallHeight / 2f, center.z), new Vector3(wallThickness, wallHeight, size.z));
    }

    private void SetWall(string wallName, Vector3 worldPos, Vector3 worldSize)
    {
        Transform wallTrans = transform.Find(wallName);
        if (wallTrans == null)
        {
            GameObject go = new GameObject(wallName);
            go.transform.SetParent(transform);
            wallTrans = go.transform;
            go.AddComponent<BoxCollider>();
        }

        // 设置世界坐标位置
        wallTrans.position = worldPos;
        wallTrans.rotation = Quaternion.identity;

        // 计算缩放，消除父级 Plane 缩放对子物体的变形影响
        Vector3 parentScale = transform.lossyScale;
        wallTrans.localScale = new Vector3(
            worldSize.x / (parentScale.x != 0 ? parentScale.x : 1f),
            worldSize.y / (parentScale.y != 0 ? parentScale.y : 1f),
            worldSize.z / (parentScale.z != 0 ? parentScale.z : 1f)
        );
    }
}