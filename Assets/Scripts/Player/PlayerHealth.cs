using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    static readonly int DieHash = Animator.StringToHash("Die");

    public int startingHealth = 100;
    public int currentHealth;
    public Slider healthSlider;
    public Image damageImage;
    public AudioClip deathClip;
    public float flashSpeed = 5f;
    public Color flashColour = new Color(1f, 0f, 0f, 0.1f);

    //Event untuk sistem lain (UI, kamera, dsb.)
    public event Action<int> Damaged;
    public event Action<int> Healed;
    public event Action Died;

    public bool IsDead => isDead;

    Animator anim;
    AudioSource playerAudio;
    PlayerMovement playerMovement;
    PlayerShooting playerShooting;
    bool isDead;
    bool damaged;

    void Awake()
    {
        //Mendapatkan reference komponen
        anim = GetComponent<Animator>();
        playerAudio = GetComponent<AudioSource>();
        playerMovement = GetComponent<PlayerMovement>();

        playerShooting = GetComponentInChildren<PlayerShooting>();
        currentHealth = startingHealth;
    }


    void Update()
    {
        //Jika terkena damage
        if (damaged)
        {
            //Merubah warna gambar menjadi value dari flashColour
            damageImage.color = flashColour;
        }
        else
        {
            //Fade out damage image
            damageImage.color = Color.Lerp(damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
        }

        //Set damage to false
        damaged = false;
    }

    //fungsi untuk mendapatkan damage
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        damaged = true;

        //mengurangi health
        currentHealth = Mathf.Max(currentHealth - amount, 0);

        //Merubah tampilan dari health slider
        healthSlider.value = currentHealth;

        //Memainkan suara ketika terkena damage
        playerAudio.Play();

        //Feedback: kamera bergetar dan getaran gamepad
        CameraShake.Shake(0.2f, 0.25f);
        GameInput.Rumble(0.4f, 0.6f, 0.15f);

        Damaged?.Invoke(amount);

        //Memanggil method Death() jika darahnya habis dan belum mati
        if (currentHealth <= 0)
        {
            Death();
        }
    }

    //fungsi untuk memulihkan health (misalnya dari health pickup)
    public void Heal(int amount)
    {
        if (isDead || currentHealth >= startingHealth) return;

        currentHealth = Mathf.Min(currentHealth + amount, startingHealth);
        healthSlider.value = currentHealth;

        Healed?.Invoke(amount);
    }

    void Death()
    {
        isDead = true;

        playerShooting.DisableEffects();

        //mentrigger animasi Die
        anim.SetTrigger(DieHash);

        //Memainkan suara ketika mati
        playerAudio.clip = deathClip;
        playerAudio.Play();

        //mematikan script player movement
        playerMovement.enabled = false;

        //mematikan script player shooting
        playerShooting.enabled = false;

        GameInput.Rumble(0.8f, 1f, 0.5f);

        Died?.Invoke();
    }

    public void RestartLevel()
    {
        //meload ulang scene yang sedang aktif
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
