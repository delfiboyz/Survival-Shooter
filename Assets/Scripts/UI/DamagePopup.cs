using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

//Angka damage yang melayang di atas enemy, memakai object pool agar tidak membebani GC
public class DamagePopup : MonoBehaviour
{
    const float Lifetime = 0.7f;
    const float RiseSpeed = 1.5f;

    static ObjectPool<DamagePopup> pool;

    Text text;
    Vector3 worldPosition;
    float age;
    Camera mainCamera;

    public static void Spawn(Vector3 worldPosition, int amount)
    {
        GameHud hud = GameHud.Instance;
        if (hud == null) return;

        if (pool == null)
        {
            pool = new ObjectPool<DamagePopup>(
                () => Create(hud),
                popup => popup.gameObject.SetActive(true),
                popup => popup.gameObject.SetActive(false),
                popup => { if (popup != null) Destroy(popup.gameObject); },
                collectionCheck: false,
                defaultCapacity: 16,
                maxSize: 64);
        }

        DamagePopup instance = pool.Get();
        instance.worldPosition = worldPosition + Random.insideUnitSphere * 0.3f;
        instance.age = 0f;
        instance.text.text = amount.ToString();
        instance.UpdateVisual();
    }

    static DamagePopup Create(GameHud hud)
    {
        GameObject go = new GameObject("DamagePopup", typeof(RectTransform), typeof(Text), typeof(Outline));
        go.transform.SetParent(hud.Root, false);

        DamagePopup popup = go.AddComponent<DamagePopup>();
        popup.text = go.GetComponent<Text>();
        popup.text.font = hud.Font;
        popup.text.fontSize = 40;
        popup.text.alignment = TextAnchor.MiddleCenter;
        popup.text.raycastTarget = false;
        popup.text.horizontalOverflow = HorizontalWrapMode.Overflow;
        popup.text.verticalOverflow = VerticalWrapMode.Overflow;
        go.GetComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.8f);

        //Popup selalu digambar di belakang menu pause
        go.transform.SetAsFirstSibling();
        return popup;
    }

    void Update()
    {
        age += Time.deltaTime;
        if (age >= Lifetime)
        {
            pool.Release(this);
            return;
        }

        worldPosition += Vector3.up * RiseSpeed * Time.deltaTime;
        UpdateVisual();
    }

    void UpdateVisual()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector3 screen = mainCamera.WorldToScreenPoint(worldPosition);
        transform.position = screen;

        float t = age / Lifetime;
        text.color = new Color(1f, Mathf.Lerp(1f, 0.4f, t), 0.2f, 1f - t * t);
        transform.localScale = Vector3.one * Mathf.Lerp(1.3f, 0.9f, t);
    }

    //Dipanggil saat HUD dihancurkan (scene berganti) karena semua popup ikut hancur
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void ResetPool()
    {
        pool = null;
    }
}
