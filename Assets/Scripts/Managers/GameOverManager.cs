using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
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
            if(!isDead)
            {
                anim.SetTrigger("GameOver");
                isDead = true;
            }
            restartTimer += Time.deltaTime;

            if(restartTimer >= restartDelay)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    public void ShowWarning(float enemyDistance)
    {
        warningText.text = string.Format("! {0} m", Mathf.RoundToInt(enemyDistance));
        anim.SetTrigger("Warning");
    }
}