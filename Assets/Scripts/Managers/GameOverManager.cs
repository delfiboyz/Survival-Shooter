using UnityEngine;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    static readonly int GameOverHash = Animator.StringToHash("GameOver");
    static readonly int WarningHash = Animator.StringToHash("Warning");

    public PlayerHealth playerHealth;
    public Text warningText;
    public float restartDelay = 5f;
    bool isDead = false;

    Animator anim;
    float restartTimer;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (playerHealth.currentHealth <= 0)
        {
            if (!isDead)
            {
                anim.SetTrigger(GameOverHash);
                isDead = true;

                //Simpan high score saat game over
                ScoreManager.SaveHighScore();
            }
            restartTimer += Time.deltaTime;

            if (restartTimer >= restartDelay)
            {
                GameState.Restart();
            }
        }
        else if (GameInput.PausePressed)
        {
            GameState.TogglePause();
        }
    }

    public void ShowWarning(float enemyDistance)
    {
        warningText.text = $"! {Mathf.RoundToInt(enemyDistance)} m";
        anim.SetTrigger(WarningHash);
    }
}
