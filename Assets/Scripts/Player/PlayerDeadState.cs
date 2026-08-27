using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "SO_PlayerDead", menuName = "FSM/Player States/Dead")]
public class PlayerDeadState : State<PlayerController>
{
    [SerializeField] private float delayBeforePause = 1.5f;
    private float timer;
    private bool isPaused;

    public override void OnEnter(PlayerController runner)
    {
        timer = delayBeforePause;
        isPaused = false;

        // 停止物理运动
        var rb = runner.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // 禁用碰撞体，避免受击穿模
        var col = runner.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        Debug.Log("【玩家死亡】播放死亡动作，1.5 秒后暂停游戏...");
    }

    public override void LogicUpdate(PlayerController runner)
    {
        if (!isPaused)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                isPaused = true;
                Time.timeScale = 0f;
                Debug.LogWarning("【GAME OVER】游戏已暂停！按下 [R] 键重新开始关卡。");
            }
        }
        else
        {
            // P1-2 防御性重载
            if (Keyboard.current != null && Keyboard.current[Key.R].wasPressedThisFrame)
            {
                try
                {
                    Time.timeScale = 1f;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
                catch (System.Exception e)
                {
                    Time.timeScale = 1f;
                    Debug.LogError($"【场景重载失败】: {e.Message}");
                }
            }
        }
    }
}