using UnityEngine;
using UnityEngine.UI;

public class ResonanceUIController : MonoBehaviour
{
    [Header("Core Countdown & Action Value UI (核心行动条/倒计时)")]
    public Slider threatSlider;
    public Text countdownText;

    [Header("Resonance Stacks UI (共鸣层数指示)")]
    public Text stackText;
    public Text buffText;

    [Header("AOE Warning & Screen Feedback (预警与屏幕反馈)")]
    public Image warningOverlay;
    public GameObject worldAOEIndicator;

    [Header("Player Guidance Prompts (玩家引导反馈)")]
    public Text guidanceText;
    public float guidanceDisplayDuration = 2.0f;
    private float guidanceTimer;

    // 记录上一次层数（-1 代表尚未初始化，避免开局误弹提示）
    private int lastStacks = -1;

    private void OnEnable()
    {
        if (EnvironmentalResonance.Instance != null)
        {
            BindEvents();
        }
    }

    private void Start()
    {
        BindEvents();
    }

    private void OnDisable()
    {
        if (EnvironmentalResonance.Instance != null)
        {
            EnvironmentalResonance.Instance.OnResonanceStacksChanged -= HandleStacksChanged;
            EnvironmentalResonance.Instance.OnTimerUpdated -= HandleTimerUpdated;
            EnvironmentalResonance.Instance.OnAOEWarningState -= HandleAOEWarningState;
            EnvironmentalResonance.Instance.OnAOETriggered -= HandleAOETriggered;
        }
    }

    private void BindEvents()
    {
        var res = EnvironmentalResonance.Instance;
        if (res == null) return;

        res.OnResonanceStacksChanged -= HandleStacksChanged;
        res.OnResonanceStacksChanged += HandleStacksChanged;

        res.OnTimerUpdated -= HandleTimerUpdated;
        res.OnTimerUpdated += HandleTimerUpdated;

        res.OnAOEWarningState -= HandleAOEWarningState;
        res.OnAOEWarningState += HandleAOEWarningState;

        res.OnAOETriggered -= HandleAOETriggered;
        res.OnAOETriggered += HandleAOETriggered;

        // 初始化显示
        HandleStacksChanged(res.resonanceStacks, res.maxResonanceStacks);
        HandleTimerUpdated(res.currentTimer, res.resonanceInterval);
    }

    private void Update()
    {
        if (guidanceTimer > 0f)
        {
            guidanceTimer -= Time.deltaTime;
            if (guidanceTimer <= 0f && guidanceText != null)
            {
                guidanceText.gameObject.SetActive(false);
            }
        }
    }

    private void HandleTimerUpdated(float currentTimer, float maxInterval)
    {
        if (threatSlider != null)
        {
            threatSlider.maxValue = maxInterval;
            threatSlider.value = maxInterval - currentTimer;
        }

        if (countdownText != null)
        {
            countdownText.text = $"{currentTimer:00.0}s";
        }
    }

    private void HandleStacksChanged(int currentStacks, int maxStacks)
    {
        if (stackText != null)
        {
            stackText.text = $"共鸣层数: {currentStacks} / {maxStacks}";
        }

        if (buffText != null)
        {
            buffText.text = $"全场敌方伤害: +{currentStacks * 8}%";
        }

        // 首次初始化：只同步数据，不显示引导文字
        if (lastStacks == -1)
        {
            lastStacks = currentStacks;
            if (guidanceText != null) guidanceText.gameObject.SetActive(false);
            return;
        }

        // 只有层数真正被玩家削减时，才弹出反制成功提示
        if (currentStacks < lastStacks)
        {
            ShowGuidance("【环境反制】共鸣已成功重置/削减！", Color.green);
        }
        // 临界满层引导警报（层数达到 2 层时）
        else if (currentStacks >= maxStacks - 1 && currentStacks < maxStacks)
        {
            ShowGuidance("【警告】环境共鸣濒临满层！请迅速攻击敌人进行破韧！", Color.yellow);
        }

        lastStacks = currentStacks;
    }

    private void HandleAOEWarningState(bool isWarning, float remainingTime)
    {
        if (isWarning)
        {
            ShowGuidance($"【危险】共鸣满层爆发倒计时: {remainingTime:F1}s！立即撤离范围！", Color.red);

            if (worldAOEIndicator != null)
            {
                worldAOEIndicator.SetActive(true);
            }

            if (warningOverlay != null)
            {
                warningOverlay.gameObject.SetActive(true);
                float alpha = Mathf.PingPong(Time.time * 4f, 0.45f) + 0.1f;
                warningOverlay.color = new Color(1f, 0f, 0f, alpha);
            }
        }
        else
        {
            if (worldAOEIndicator != null)
            {
                worldAOEIndicator.SetActive(false);
            }
            if (warningOverlay != null)
            {
                warningOverlay.gameObject.SetActive(false);
            }
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