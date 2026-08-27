using UnityEngine;
using UnityEngine.UI;

public class ResonanceUIController : MonoBehaviour
{
    [Header("UI 引用")]
    [SerializeField] private Slider threatSlider;         // 威胁进度条
    [SerializeField] private Text countdownText;          // 倒计时数字
    [SerializeField] private Text stackText;              // 层数文本
    [SerializeField] private Text buffText;               // 增益文本
    [SerializeField] private Image warningOverlay;        // 全屏预警遮罩
    [SerializeField] private GameObject worldAOEIndicator; // 世界空间 AOE 范围

    [Header("引导系统")]
    [SerializeField] private Text guidanceText;           // 玩家引导文字
    [SerializeField] private float guidanceDisplayDuration = 2f;

    private float guidanceTimer;
    private int lastStacks;
    private bool isAOEWarningActive;                      // 标记当前是否处于预警状态

    private void OnEnable()
    {
        // 仅在 OnEnable 中统一绑定事件
        BindEvents();
    }

    private void Start()
    {
        // Start 中不再调用 BindEvents，仅主动拉取一次单例当前数据做初始刷新
        var inst = EnvironmentalResonance.Instance;
        if (inst != null)
        {
            HandleStacksChanged(inst.resonanceStacks, inst.maxResonanceStacks);
            HandleTimerUpdated(inst.currentTimer, inst.resonanceInterval);
        }
    }

    private void OnDisable()
    {
        if (EnvironmentalResonance.Instance == null) return;
        EnvironmentalResonance.Instance.OnResonanceStacksChanged -= HandleStacksChanged;
        EnvironmentalResonance.Instance.OnTimerUpdated -= HandleTimerUpdated;
        EnvironmentalResonance.Instance.OnAOEWarningState -= HandleAOEWarningState;
        EnvironmentalResonance.Instance.OnAOETriggered -= HandleAOETriggered;
    }

    private void BindEvents()
    {
        var inst = EnvironmentalResonance.Instance;
        if (inst == null) return;

        // 防御性解绑：先减后加，彻底避免重复注册
        inst.OnResonanceStacksChanged -= HandleStacksChanged;
        inst.OnTimerUpdated -= HandleTimerUpdated;
        inst.OnAOEWarningState -= HandleAOEWarningState;
        inst.OnAOETriggered -= HandleAOETriggered;

        inst.OnResonanceStacksChanged += HandleStacksChanged;
        inst.OnTimerUpdated += HandleTimerUpdated;
        inst.OnAOEWarningState += HandleAOEWarningState;
        inst.OnAOETriggered += HandleAOETriggered;
    }

    private void Update()
    {
        // 引导提示文字倒计时隐藏
        if (guidanceTimer >= 0)
        {
            guidanceTimer -= Time.deltaTime;
            if (guidanceTimer <= 0 && guidanceText != null)
            {
                guidanceText.gameObject.SetActive(false);
            }
        }

        // P0-2 修复：将 PingPong 动画放到 Update 中持续执行
        if (isAOEWarningActive)
        {
            float alpha = Mathf.PingPong(Time.time * 4f, 0.3f) + 0.2f;
            if (warningOverlay != null)
            {
                warningOverlay.color = new Color(1f, 0f, 0f, alpha);
            }
        }
    }

    private void HandleTimerUpdated(float current, float max)
    {
        if (countdownText != null)
        {
            countdownText.text = $"{current:00.0}s";
        }

        if (threatSlider != null)
        {
            threatSlider.maxValue = max;
            threatSlider.value = max - current;
        }
    }

    private void HandleStacksChanged(int current, int max)
    {
        if (stackText != null)
        {
            stackText.text = $"共鸣层数: {current} / {max}";
        }

        if (buffText != null)
        {
            buffText.text = $"全场敌方伤害: +{current * 2}%";
        }

        // 玩家引导颜色分级
        if (current < lastStacks)
        {
            ShowGuidance("【环境反制】共鸣已成功重置/削减！", Color.green);
        }
        else if (current >= max - 1 && current < max)
        {
            ShowGuidance("【警告】环境共鸣濒临满层！请迅速攻击敌人进行破韧！", Color.yellow);
        }

        lastStacks = current;
    }

    private void HandleAOEWarningState(bool isWarning, float duration)
    {
        isAOEWarningActive = isWarning;

        if (isWarning)
        {
            ShowGuidance($"【危险】共鸣满层爆发倒计时: {duration:F1}s！立即撤离范围！", Color.red);
            if (warningOverlay != null) warningOverlay.gameObject.SetActive(true);
            if (worldAOEIndicator != null) worldAOEIndicator.SetActive(true);
        }
        else
        {
            if (warningOverlay != null) warningOverlay.gameObject.SetActive(false);
            if (worldAOEIndicator != null) worldAOEIndicator.SetActive(false);
        }
    }

    private void HandleAOETriggered()
    {
        ShowGuidance("【爆发命中】环境共鸣冲击释放！", Color.red);
    }

    private void ShowGuidance(string message, Color color)
    {
        if (guidanceText == null) return;
        guidanceText.text = message;
        guidanceText.color = color;
        guidanceText.gameObject.SetActive(true);
        guidanceTimer = guidanceDisplayDuration;
    }
}