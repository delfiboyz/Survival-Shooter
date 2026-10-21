using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public int damagePerShot = 20;
    public float timeBetweenBullets = 0.15f;
    public float range = 100f;

    //True jika input tembak dikendalikan oleh InputHandler (pola Command)
    [System.NonSerialized]
    public bool externallyDriven;

    float timer;
    Ray shootRay;
    int shootableMask;
    ParticleSystem gunParticles;
    LineRenderer gunLine;
    AudioSource gunAudio;
    Light gunLight;
    const float effectsDisplayTime = 0.2f;

    //Senjata siap menembak (fire rate terpenuhi)
    public bool CanShoot => timer >= timeBetweenBullets && enabled && !GameState.IsPaused;

    void Awake()
    {
        shootableMask = LayerMask.GetMask("Shootable");
        gunParticles = GetComponent<ParticleSystem>();
        gunLine = GetComponent<LineRenderer>();
        gunAudio = GetComponent<AudioSource>();
        gunLight = GetComponent<Light>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (!externallyDriven && GameInput.FireHeld && CanShoot)
        {
            Shoot();
        }

        if (timer >= timeBetweenBullets * effectsDisplayTime)
        {
            DisableEffects();
        }
    }

    public void DisableEffects()
    {
        gunLine.enabled = false;
        gunLight.enabled = false;
    }

    public void Shoot()
    {
        timer = 0f;

        gunAudio.Play();

        gunLight.enabled = true;

        gunParticles.Stop();
        gunParticles.Play();

        gunLine.enabled = true;
        gunLine.SetPosition(0, transform.position);

        shootRay.origin = transform.position;
        shootRay.direction = transform.forward;

        if (Physics.Raycast(shootRay, out RaycastHit shootHit, range, shootableMask))
        {
            if (shootHit.collider.TryGetComponent(out EnemyHealth enemyHealth))
            {
                enemyHealth.TakeDamage(damagePerShot, shootHit.point);
            }

            gunLine.SetPosition(1, shootHit.point);
        }
        else
        {
            gunLine.SetPosition(1, shootRay.origin + shootRay.direction * range);
        }

        CameraShake.Shake(0.05f, 0.05f);
    }
}
