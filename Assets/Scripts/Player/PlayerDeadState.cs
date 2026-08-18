using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "PlayerDeadState", menuName = "FSM/Player/DeadState")]
public class PlayerDeadState : State
{
    [Header("Settings")]
    [Tooltip("倒地动画播放时长（秒），结束后暂停游戏")]
    public float delayBeforePause = 1.5f;

    private float timer;
    private bool isPaused;

    public override void OnEnter(StateMachine stateMachine)
    {
        timer = delayBeforePause;
        isPaused = false;

        var controller = stateMachine.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.isDead = true;
            controller.HideUI();
        }

        // 1. 冻结玩家刚体物理
        var rb = stateMachine.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // 2. 触发死亡倒地动画
        var animator = stateMachine.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        Debug.Log("【玩家死亡】播放死亡动作，1.5 秒后暂停游戏...");
    }

    public override void LogicUpdate(StateMachine stateMachine)
    {
        // 倒计时阶段（使用常规时间）
        if (!isPaused)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                isPaused = true;
                Time.timeScale = 0f; // 冻结游戏时间（暂停场景所有物体）
                Debug.LogWarning("【GAME OVER】游戏已暂停！按下 [R] 键重新开始关卡。");
            }
        }

        // 暂停后检测重开按键（Input 依然生效）
        if (Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f; // 重置时间缩放，防止新场景继续卡住
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public override void TransitionChecks(StateMachine stateMachine) { }

    public override void OnExit(StateMachine stateMachine)
    {
        Time.timeScale = 1f; // 退出状态时恢复时间流速
    }
}