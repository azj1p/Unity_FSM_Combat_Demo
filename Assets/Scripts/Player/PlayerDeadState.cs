using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "PlayerDeadState", menuName = "FSM/Player/DeadState")]
public class PlayerDeadState : State<PlayerController>
{
    [Header("Settings")]
    public float delayBeforePause = 1.5f;

    private float timer;
    private bool isPaused;

    public override void OnEnter(PlayerController player)
    {
        timer = delayBeforePause;
        isPaused = false;

        if (player != null)
        {
            player.isDead = true;
            player.HideUI();
        }

        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        var anim = player.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        Debug.Log("【玩家死亡】播放死亡动作，1.5 秒后暂停游戏...");
    }

    public override void LogicUpdate(PlayerController player)
    {
        if (!isPaused)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                isPaused = true;
                Time.timeScale = 0f;
                Debug.LogWarning("【GAME OVER】游戏已暂停！按下 [R] 键重新开始关卡。");
            }
        }

        // 使用新输入系统检测 R 键重开
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public override void TransitionChecks(PlayerController player) { }

    public override void OnExit(PlayerController player)
    {
        Time.timeScale = 1f;
    }
}