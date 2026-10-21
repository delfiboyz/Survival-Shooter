using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    static readonly int PlayerDeadHash = Animator.StringToHash("PlayerDead");

    public float timeBetweenAttacks = 0.5f;
    public int attackDamage = 10;


    Animator anim;
    GameObject player;
    PlayerHealth playerHealth;
    EnemyHealth enemyHealth;
    bool playerInRange;
    bool playerDeadTriggered;
    float timer;


    void Awake()
    {
        //Mencari game object dengan tag "Player"
        player = GameObject.FindGameObjectWithTag("Player");

        //mendapatkan komponen player health
        playerHealth = player.GetComponent<PlayerHealth>();

        //mendapatkan komponen Animator
        anim = GetComponent<Animator>();

        //mendapatkan Enemy health
        enemyHealth = GetComponent<EnemyHealth>();
    }

    //Callback jika ada suatu object masuk kedalam trigger
    void OnTriggerEnter(Collider other)
    {
        //Set player in range
        if (other.gameObject == player && !other.isTrigger)
        {
            playerInRange = true;
        }
    }

    //Callback jika ada object yang keluar dari trigger
    void OnTriggerExit(Collider other)
    {
        //Set player not in range
        if (other.gameObject == player && !other.isTrigger)
        {
            playerInRange = false;
        }
    }


    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= timeBetweenAttacks && playerInRange && enemyHealth.currentHealth > 0)
        {
            Attack();
        }

        //mentrigger animasi PlayerDead sekali jika darah player habis
        if (!playerDeadTriggered && playerHealth.currentHealth <= 0)
        {
            anim.SetTrigger(PlayerDeadHash);
            playerDeadTriggered = true;
        }
    }


    void Attack()
    {
        //Reset timer
        timer = 0f;

        //Taking Damage
        if (playerHealth.currentHealth > 0)
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }
}
