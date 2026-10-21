using System;
using UnityEngine;
using UnityEngine.SceneManagement;

//Status global permainan (pause / resume / restart)
public static class GameState
{
    public static bool IsPaused { get; private set; }

    public static event Action<bool> PauseChanged;

    public static void SetPaused(bool paused)
    {
        if (IsPaused == paused) return;

        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        AudioListener.pause = paused;
        PauseChanged?.Invoke(paused);
    }

    public static void TogglePause()
    {
        SetPaused(!IsPaused);
    }

    public static void Restart()
    {
        SetPaused(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    //Pastikan game tidak tertinggal dalam keadaan pause saat scene dimuat / Play Mode dimulai
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetState()
    {
        IsPaused = false;
        PauseChanged = null;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }
}
