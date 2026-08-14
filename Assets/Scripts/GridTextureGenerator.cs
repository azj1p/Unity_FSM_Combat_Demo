// 【通用工具】网格材质生成器，为地面自动生成程序化网格线条，提供参照物解决移动无感问题
using UnityEngine;

[ExecuteAlways] // 编辑器模式下实时生效，无需点运行
public class GridTextureGenerator : MonoBehaviour
{
    [Header("网格外观设置")]
    public int textureResolution = 256;              // 贴图分辨率
    public int lineThickness = 4;                     // 粗细像素
    public Color backgroundColor = new Color(0.2f, 0.2f, 0.2f); // 背景深灰色
    public Color lineColor = new Color(0.35f, 0.35f, 0.35f);   // 网格线浅灰色

    [Header("密度设置")]
    [Tooltip("每米有多少个网格格子")]
    public float gridPerUnit = 1.0f;

    private void OnValidate()
    {
        GenerateGrid();
    }

    private void Start()
    {
        GenerateGrid();
    }

    public void GenerateGrid()
    {
        Renderer r = GetComponent<Renderer>();
        if (r == null || r.sharedMaterial == null) return;

        // 1. 程序化生成一张网格 Texture2D
        Texture2D texture = new Texture2D(textureResolution, textureResolution);
        Color[] pixels = new Color[textureResolution * textureResolution];

        for (int y = 0; y < textureResolution; y++)
        {
            for (int x = 0; x < textureResolution; x++)
            {
                bool isBorder = (x < lineThickness || x >= textureResolution - lineThickness ||
                                y < lineThickness || y >= textureResolution - lineThickness);
                pixels[y * textureResolution + x] = isBorder ? lineColor : backgroundColor;
            }
        }

        texture.SetPixels(pixels);
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;
        texture.Apply();

        // 2. 根据 Plane 的真实尺寸（Plane 1Scale = 10米）自动计算平铺数量 (Tiling)
        Vector3 worldScale = transform.lossyScale;
        Vector2 tiling = new Vector2(worldScale.x * 10f * gridPerUnit, worldScale.z * 10f * gridPerUnit);

        // 3. 兼容 URP 与 Built-in 渲染管线赋值
        Material mat = r.sharedMaterial;
        if (mat.HasProperty("_BaseMap"))
        {
            mat.SetTexture("_BaseMap", texture);
            mat.SetTextureScale("_BaseMap", tiling);
        }
        else
        {
            mat.mainTexture = texture;
            mat.mainTextureScale = tiling;
        }
    }
}