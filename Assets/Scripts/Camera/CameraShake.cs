using UnityEngine;

//Camera shake berbasis trauma: getaran meluruh secara halus seiring waktu
public static class CameraShake
{
    const float MaxOffset = 0.6f;
    const float Frequency = 25f;

    static float trauma;
    static float decayPerSecond = 1f;
    static float lastUpdateTime;
    static Vector3 cachedOffset;
    static int cachedFrame = -1;

    public static bool Enabled = true;

    //amount: 0..1 kekuatan getaran, duration: lama getaran (detik)
    public static void Shake(float amount, float duration)
    {
        if (!Enabled) return;

        UpdateDecay();
        trauma = Mathf.Clamp01(Mathf.Max(trauma, amount));
        decayPerSecond = trauma / Mathf.Max(duration, 0.01f);
    }

    public static Vector3 Offset
    {
        get
        {
            if (cachedFrame == Time.frameCount) return cachedOffset;
            cachedFrame = Time.frameCount;

            UpdateDecay();
            if (trauma <= 0f || !Enabled)
            {
                cachedOffset = Vector3.zero;
                return cachedOffset;
            }

            //Kuadrat trauma memberikan getaran yang lebih alami
            float strength = trauma * trauma * MaxOffset;
            float t = Time.time * Frequency;
            cachedOffset = new Vector3(
                (Mathf.PerlinNoise(t, 0f) * 2f - 1f) * strength,
                0f,
                (Mathf.PerlinNoise(0f, t) * 2f - 1f) * strength);
            return cachedOffset;
        }
    }

    static void UpdateDecay()
    {
        float now = Time.time;
        trauma = Mathf.Max(0f, trauma - decayPerSecond * (now - lastUpdateTime));
        lastUpdateTime = now;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetState()
    {
        trauma = 0f;
        lastUpdateTime = 0f;
        cachedFrame = -1;
    }
}
