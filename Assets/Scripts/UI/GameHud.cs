using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

//HUD modern yang dibuat otomatis saat runtime (tidak perlu mengubah scene):
//- info wave & high score
//- banner wave
//- menu pause (resume, restart, volume, screen shake, quit)
//- tulisan "NEW HIGH SCORE" saat game over
public class GameHud : MonoBehaviour
{
    const string VolumeKey = "SurvivalShooter.Volume";
    const string ShakeKey = "SurvivalShooter.ScreenShake";

    static readonly Color Accent = new Color(1f, 0.82f, 0.25f);
    static readonly Color PanelColor = new Color(0.05f, 0.05f, 0.08f, 0.85f);
    static readonly Color ButtonColor = new Color(0.18f, 0.18f, 0.24f, 1f);

    public static GameHud Instance { get; private set; }

    public Font Font { get; private set; }
    public RectTransform Root { get; private set; }

    PlayerHealth playerHealth;
    Text waveText;
    Text bestText;
    Text bannerText;
    Text newHighScoreText;
    GameObject pausePanel;
    Button resumeButton;
    Text shakeButtonLabel;
    Coroutine bannerRoutine;
    bool gameOverShown;
    int shownWave = -1;
    int shownBest = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        AudioListener.volume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        CameraShake.Enabled = PlayerPrefs.GetInt(ShakeKey, 1) == 1;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        CreateForScene();
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CreateForScene();
    }

    static void CreateForScene()
    {
        //Hanya dibuat pada scene gameplay (yang memiliki ScoreManager)
        if (Instance != null || FindAnyObjectByType<ScoreManager>() == null) return;

        new GameObject("GameHud").AddComponent<GameHud>();
    }

    void Awake()
    {
        Instance = this;
        playerHealth = FindAnyObjectByType<PlayerHealth>();

        //Gunakan font yang sama dengan HUD bawaan (LuckiestGuy)
        ScoreManager scoreManager = FindAnyObjectByType<ScoreManager>();
        Text scoreText = scoreManager != null ? scoreManager.GetComponent<Text>() : null;
        Font = scoreText != null && scoreText.font != null
            ? scoreText.font
            : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        EnsureEventSystem();
        BuildCanvas();

        EnemyManager.WaveStarted += OnWaveStarted;
        EnemyManager.WaveCleared += OnWaveCleared;
        GameState.PauseChanged += OnPauseChanged;
    }

    void OnDestroy()
    {
        EnemyManager.WaveStarted -= OnWaveStarted;
        EnemyManager.WaveCleared -= OnWaveCleared;
        GameState.PauseChanged -= OnPauseChanged;
        DamagePopup.ResetPool();
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        //Hanya perbarui teks jika nilainya berubah
        int wave = EnemyManager.CurrentWave;
        if (wave != shownWave)
        {
            shownWave = wave;
            waveText.text = wave > 0 ? $"Wave {wave}" : string.Empty;
        }

        int best = Mathf.Max(ScoreManager.HighScore, ScoreManager.score);
        if (best != shownBest)
        {
            shownBest = best;
            bestText.text = $"Best: {best}";
        }

        if (!gameOverShown && playerHealth != null && playerHealth.IsDead)
        {
            gameOverShown = true;
            StartCoroutine(ShowGameOverInfo());
        }
    }

    IEnumerator ShowGameOverInfo()
    {
        //Tunggu GameOverManager menyimpan high score
        yield return null;

        if (ScoreManager.IsNewHighScore)
        {
            newHighScoreText.text = $"NEW HIGH SCORE!  {ScoreManager.HighScore}";
            newHighScoreText.gameObject.SetActive(true);
        }
    }

    void OnWaveStarted(int wave)
    {
        ShowBanner($"WAVE {wave}");
    }

    void OnWaveCleared(int wave)
    {
        ShowBanner($"WAVE {wave} CLEARED!");
    }

    void ShowBanner(string message)
    {
        if (bannerRoutine != null) StopCoroutine(bannerRoutine);
        bannerRoutine = StartCoroutine(BannerRoutine(message));
    }

    IEnumerator BannerRoutine(string message)
    {
        bannerText.text = message;
        const float fade = 0.35f, hold = 1.4f;

        for (float t = 0f; t < fade; t += Time.deltaTime)
        {
            SetAlpha(bannerText, t / fade);
            bannerText.rectTransform.localScale = Vector3.one * Mathf.Lerp(1.4f, 1f, t / fade);
            yield return null;
        }
        SetAlpha(bannerText, 1f);
        bannerText.rectTransform.localScale = Vector3.one;

        yield return new WaitForSeconds(hold);

        for (float t = 0f; t < fade; t += Time.deltaTime)
        {
            SetAlpha(bannerText, 1f - t / fade);
            yield return null;
        }
        SetAlpha(bannerText, 0f);
        bannerRoutine = null;
    }

    void OnPauseChanged(bool paused)
    {
        pausePanel.SetActive(paused);
        if (paused && EventSystem.current != null)
        {
            //Fokus ke tombol Resume agar menu bisa dinavigasi dengan gamepad / keyboard
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        }
    }

    // ---------------------------------------------------------------- UI builder

    static void EnsureEventSystem()
    {
        EventSystem eventSystem = FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
        }

#if ENABLE_INPUT_SYSTEM
        //Ganti StandaloneInputModule lama dengan module Input System
        if (eventSystem.TryGetComponent(out StandaloneInputModule legacyModule))
        {
            DestroyImmediate(legacyModule);
        }
        if (!eventSystem.TryGetComponent(out InputSystemUIInputModule _))
        {
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
#else
        if (!eventSystem.TryGetComponent(out BaseInputModule _))
        {
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }
#endif
    }

    void BuildCanvas()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();
        Root = (RectTransform)transform;

        //Info kanan atas
        waveText = CreateText("WaveText", Root, 48, TextAnchor.UpperRight, Color.white);
        Anchor(waveText.rectTransform, new Vector2(1f, 1f), new Vector2(-40f, -30f), new Vector2(500f, 60f));

        bestText = CreateText("BestText", Root, 36, TextAnchor.UpperRight, Accent);
        Anchor(bestText.rectTransform, new Vector2(1f, 1f), new Vector2(-40f, -90f), new Vector2(500f, 50f));

        //Banner tengah
        bannerText = CreateText("WaveBanner", Root, 110, TextAnchor.MiddleCenter, Accent);
        Anchor(bannerText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 220f), new Vector2(1600f, 160f));
        AddOutline(bannerText);
        SetAlpha(bannerText, 0f);

        //New high score (di bawah tulisan Game Over bawaan)
        newHighScoreText = CreateText("NewHighScore", Root, 64, TextAnchor.MiddleCenter, Accent);
        Anchor(newHighScoreText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -140f), new Vector2(1600f, 100f));
        AddOutline(newHighScoreText);
        newHighScoreText.gameObject.SetActive(false);

        BuildPauseMenu();
    }

    void BuildPauseMenu()
    {
        pausePanel = new GameObject("PauseMenu", typeof(RectTransform), typeof(Image));
        RectTransform panelRect = (RectTransform)pausePanel.transform;
        panelRect.SetParent(Root, false);
        Stretch(panelRect);
        pausePanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

        GameObject box = new GameObject("Box", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        RectTransform boxRect = (RectTransform)box.transform;
        boxRect.SetParent(panelRect, false);
        Anchor(boxRect, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620f, 720f));
        box.GetComponent<Image>().color = PanelColor;

        VerticalLayoutGroup layout = box.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(50, 50, 40, 40);
        layout.spacing = 22f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Text title = CreateText("Title", boxRect, 90, TextAnchor.MiddleCenter, Accent);
        title.text = "PAUSED";
        SetHeight(title.gameObject, 120f);

        resumeButton = CreateButton(boxRect, "Resume", () => GameState.SetPaused(false));
        CreateButton(boxRect, "Restart", GameState.Restart);

        //Volume
        Text volumeLabel = CreateText("VolumeLabel", boxRect, 34, TextAnchor.MiddleLeft, Color.white);
        volumeLabel.text = "Volume";
        SetHeight(volumeLabel.gameObject, 44f);

        Slider volumeSlider = CreateSlider(boxRect);
        volumeSlider.value = AudioListener.volume;
        volumeSlider.onValueChanged.AddListener(value =>
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat(VolumeKey, value);
        });

        //Screen shake on/off
        Button shakeButton = CreateButton(boxRect, ShakeLabel(), () =>
        {
            CameraShake.Enabled = !CameraShake.Enabled;
            PlayerPrefs.SetInt(ShakeKey, CameraShake.Enabled ? 1 : 0);
            shakeButtonLabel.text = ShakeLabel();
        });
        shakeButtonLabel = shakeButton.GetComponentInChildren<Text>();

        CreateButton(boxRect, "Quit", GameState.Quit);

        Text hint = CreateText("Hint", boxRect, 24, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.6f));
        hint.text = "WASD / Stick kiri: gerak  •  Mouse / Stick kanan: bidik\nKlik / RT: tembak  •  Z / LB: undo  •  Esc / Start: pause";
        SetHeight(hint.gameObject, 70f);

        pausePanel.SetActive(false);
    }

    static string ShakeLabel()
    {
        return CameraShake.Enabled ? "Screen Shake: ON" : "Screen Shake: OFF";
    }

    Text CreateText(string name, RectTransform parent, int size, TextAnchor alignment, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        Text text = go.GetComponent<Text>();
        text.font = Font;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    Button CreateButton(RectTransform parent, string label, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        SetHeight(go, 80f);

        Image image = go.GetComponent<Image>();
        image.color = Color.white;

        Button button = go.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = ButtonColor;
        colors.highlightedColor = Color.Lerp(ButtonColor, Accent, 0.35f);
        colors.selectedColor = Color.Lerp(ButtonColor, Accent, 0.35f);
        colors.pressedColor = Accent;
        button.colors = colors;
        button.onClick.AddListener(onClick);

        Text text = CreateText("Label", (RectTransform)go.transform, 40, TextAnchor.MiddleCenter, Color.white);
        text.text = label;
        Stretch(text.rectTransform);

        return button;
    }

    Slider CreateSlider(RectTransform parent)
    {
        GameObject go = DefaultControls.CreateSlider(new DefaultControls.Resources());
        go.name = "VolumeSlider";
        go.transform.SetParent(parent, false);
        SetHeight(go, 40f);

        foreach (Image image in go.GetComponentsInChildren<Image>())
        {
            image.color = image.name == "Fill" ? Accent : image.name == "Handle" ? Color.white : ButtonColor;
        }

        Slider slider = go.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        return slider;
    }

    static void AddOutline(Text text)
    {
        Outline outline = text.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
        outline.effectDistance = new Vector2(3f, -3f);
    }

    static void SetHeight(GameObject go, float height)
    {
        if (!go.TryGetComponent(out LayoutElement element))
        {
            element = go.AddComponent<LayoutElement>();
        }
        element.preferredHeight = height;
        element.minHeight = height;
    }

    static void Anchor(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static void SetAlpha(Graphic graphic, float alpha)
    {
        Color color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }
}
