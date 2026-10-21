using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    static readonly int DeadHash = Animator.StringToHash("Dead");

    public int startingHealth = 100;
    public int currentHealth;
    public float sinkSpeed = 2.5f;
    public int scoreValue = 10;
    public AudioClip deathClip;

    [Range(0f, 1f)]
    public float healthDropChance = 0.15f;
    public int healthDropAmount = 20;


    Animator anim;
    AudioSource enemyAudio;
    ParticleSystem hitParticles;
    CapsuleCollider capsuleCollider;
    bool isDead;
    bool isSinking;

    public bool IsDead => isDead;


    void Awake()
    {
        //Mendapatkan reference komponen
        anim = GetComponent<Animator>();
        enemyAudio = GetComponent<AudioSource>();
        hitParticles = GetComponentInChildren<ParticleSystem>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        //Set current health
        currentHealth = startingHealth;
    }


    void Update()
    {
        //Check jika sinking
        if (isSinking)
        {
            //memindahkan object kebawah
            transform.Translate(-Vector3.up * sinkSpeed * Time.deltaTime);
        }
    }


    public void TakeDamage(int amount, Vector3 hitPoint)
    {
        //Check jika dead
        if (isDead)
            return;

        //play audio
        enemyAudio.Play();

        //kurangi health
        currentHealth -= amount;

        //Ganti posisi particle
        hitParticles.transform.position = hitPoint;

        //Play particle system
        hitParticles.Play();

        //Tampilkan angka damage
        DamagePopup.Spawn(hitPoint, amount);

        //Dead jika health <= 0
        if (currentHealth <= 0)
        {
            Death();
        }
    }


    void Death()
    {
        //set isdead
        isDead = true;

        //Set capsule collider ke trigger
        capsuleCollider.isTrigger = true;

        //trigger play animation Dead
        anim.SetTrigger(DeadHash);

        //Play Sound Dead
        enemyAudio.clip = deathClip;
        enemyAudio.Play();

        //Tambah skor (dengan combo multiplier)
        ScoreManager.RegisterKill(scoreValue);

        //Kemungkinan menjatuhkan health pickup
        if (Random.value < healthDropChance)
        {
            HealthPickup.Spawn(transform.position, healthDropAmount);
        }
    }

    //Dipanggil oleh animation event di akhir animasi Death
    public void StartSinking()
    {
        //disable Navmesh Component
        GetComponent<NavMeshAgent>().enabled = false;
        //Set rigidbody ke kinematic
        GetComponent<Rigidbody>().isKinematic = true;
        isSinking = true;
        Destroy(gameObject, 2f);
    }
}
