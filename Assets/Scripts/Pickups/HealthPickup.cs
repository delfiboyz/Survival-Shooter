using UnityEngine;

//Health pickup yang dijatuhkan enemy secara acak; dibuat secara prosedural tanpa prefab
public class HealthPickup : MonoBehaviour
{
    static readonly Color PickupColor = new Color(0.3f, 1f, 0.45f);

    public int healAmount = 20;
    public float lifetime = 10f;

    const float BobHeight = 0.25f;
    const float BobSpeed = 3f;
    const float SpinSpeed = 120f;
    const float BlinkTime = 3f;

    Vector3 basePosition;
    float age;
    Renderer pickupRenderer;
    bool collected;

    public static HealthPickup Spawn(Vector3 position, int amount)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "HealthPickup";
        go.transform.position = new Vector3(position.x, 0.6f, position.z);
        go.transform.localScale = Vector3.one * 0.5f;

        SphereCollider sphere = go.GetComponent<SphereCollider>();
        sphere.isTrigger = true;
        sphere.radius = 1.5f;

        Renderer renderer = go.GetComponent<Renderer>();
        renderer.material.color = PickupColor;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        Light glow = go.AddComponent<Light>();
        glow.type = LightType.Point;
        glow.color = PickupColor;
        glow.range = 3f;
        glow.intensity = 2f;

        HealthPickup pickup = go.AddComponent<HealthPickup>();
        pickup.healAmount = amount;
        return pickup;
    }

    void Start()
    {
        basePosition = transform.position;
        pickupRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        age += Time.deltaTime;
        if (age >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        //Animasi melayang dan berputar
        transform.position = basePosition + Vector3.up * Mathf.Sin(age * BobSpeed) * BobHeight;
        transform.Rotate(0f, SpinSpeed * Time.deltaTime, 0f, Space.World);

        //Berkedip saat hampir hilang
        if (pickupRenderer != null && lifetime - age < BlinkTime)
        {
            pickupRenderer.enabled = Mathf.Repeat(age * 8f, 1f) > 0.3f;
        }
    }

    //OnTriggerStay: pickup tetap bisa diambil jika player sudah berdiri di atasnya lalu terkena damage
    void OnTriggerStay(Collider other)
    {
        if (collected || other.isTrigger) return;

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null || playerHealth.IsDead || playerHealth.currentHealth >= playerHealth.startingHealth)
        {
            return;
        }

        collected = true;
        playerHealth.Heal(healAmount);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        //Bersihkan instance material yang dibuat oleh renderer.material
        if (pickupRenderer != null) Destroy(pickupRenderer.sharedMaterial);
    }
}
