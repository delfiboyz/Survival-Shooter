using UnityEngine;
using UnityEngine.UI;

//Mengelola skor, combo multiplier, dan high score yang tersimpan (PlayerPrefs)
public class ScoreManager : MonoBehaviour
{
    const string HighScoreKey = "SurvivalShooter.HighScore";

    public static int score;
    public static int HighScore { get; private set; }
    public static int Combo { get; private set; }
    public static int Multiplier => Mathf.Clamp(1 + Combo / ComboPerMultiplier, 1, MaxMultiplier);
    public static bool IsNewHighScore { get; private set; }

    //Waktu (detik) maksimal antar kill agar combo tidak putus
    public float comboWindow = 2.5f;

    const int ComboPerMultiplier = 5;
    const int MaxMultiplier = 5;

    static float lastKillTime;
    static float comboWindowStatic = 2.5f;

    Text text;
    int shownScore = -1;
    int shownMultiplier = -1;


    void Awake()
    {
        text = GetComponent<Text>();
        score = 0;
        Combo = 0;
        IsNewHighScore = false;
        lastKillTime = float.NegativeInfinity;
        comboWindowStatic = comboWindow;
        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
    }


    void Update()
    {
        //Combo putus jika terlalu lama tidak membunuh enemy
        if (Combo > 0 && Time.time - lastKillTime > comboWindowStatic)
        {
            Combo = 0;
        }

        //Hanya perbarui teks jika nilainya berubah (menghindari alokasi string setiap frame)
        if (score != shownScore || Multiplier != shownMultiplier)
        {
            shownScore = score;
            shownMultiplier = Multiplier;
            text.text = Multiplier > 1 ? $"Score: {score}  x{Multiplier}" : $"Score: {score}";
        }
    }

    //Dipanggil saat enemy mati; mengembalikan poin yang didapat
    public static int RegisterKill(int baseValue)
    {
        Combo = Time.time - lastKillTime <= comboWindowStatic ? Combo + 1 : 1;
        lastKillTime = Time.time;

        int points = baseValue * Multiplier;
        score += points;
        return points;
    }

    //Simpan high score; mengembalikan true jika rekor baru
    public static bool SaveHighScore()
    {
        if (score <= HighScore) return false;

        HighScore = score;
        IsNewHighScore = true;
        PlayerPrefs.SetInt(HighScoreKey, HighScore);
        PlayerPrefs.Save();
        return true;
    }
}
