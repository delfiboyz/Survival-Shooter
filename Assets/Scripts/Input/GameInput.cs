using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

//Lapisan input terpusat: mendukung keyboard + mouse dan gamepad (twin-stick).
//Memakai Input System baru jika aktif, dan fallback ke Input Manager lama jika tidak.
public static class GameInput
{
    const float StickDeadZone = 0.2f;

    //Arah gerak di bidang XZ (x = horizontal, y = vertical), panjang maksimal 1
    public static Vector2 Move
    {
        get
        {
            Vector2 move = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) move.x += 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) move.x -= 1f;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) move.y += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) move.y -= 1f;
            }
            Gamepad pad = Gamepad.current;
            if (pad != null)
            {
                Vector2 stick = pad.leftStick.ReadValue();
                if (stick.sqrMagnitude > StickDeadZone * StickDeadZone) move += stick;
            }
#else
            move.x = Input.GetAxisRaw("Horizontal");
            move.y = Input.GetAxisRaw("Vertical");
#endif
            return Vector2.ClampMagnitude(move, 1f);
        }
    }

    //Arah bidik dari stick kanan gamepad; false jika stick tidak digunakan
    public static bool TryGetStickAim(out Vector2 aim)
    {
        aim = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
        Gamepad pad = Gamepad.current;
        if (pad != null)
        {
            aim = pad.rightStick.ReadValue();
            return aim.sqrMagnitude > StickDeadZone * StickDeadZone;
        }
#endif
        return false;
    }

    //Posisi pointer (mouse) di layar
    public static Vector2 PointerPosition
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
            return Input.mousePosition;
#endif
        }
    }

    //Tombol tembak sedang ditahan
    public static bool FireHeld
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            bool mouse = Mouse.current != null && Mouse.current.leftButton.isPressed;
            bool pad = Gamepad.current != null &&
                       (Gamepad.current.rightTrigger.isPressed || Gamepad.current.rightShoulder.isPressed);
            return mouse || pad;
#else
            return Input.GetButton("Fire1");
#endif
        }
    }

    //Tombol undo gerakan (pola Command)
    public static bool UndoHeld
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return (Keyboard.current != null && Keyboard.current.zKey.isPressed) ||
                   (Gamepad.current != null && Gamepad.current.leftShoulder.isPressed);
#else
            return Input.GetKey(KeyCode.Z);
#endif
        }
    }

    //Tombol pause ditekan pada frame ini
    public static bool PausePressed
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                   (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }
    }

    //Getar gamepad singkat (haptic feedback)
    public static void Rumble(float low, float high, float duration)
    {
#if ENABLE_INPUT_SYSTEM
        Gamepad pad = Gamepad.current;
        if (pad == null) return;
        pad.SetMotorSpeeds(low, high);
        RumbleStopper.StopAfter(pad, duration);
#endif
    }

#if ENABLE_INPUT_SYSTEM
    class RumbleStopper : MonoBehaviour
    {
        static RumbleStopper instance;
        Gamepad pad;
        float stopTime;

        public static void StopAfter(Gamepad pad, float duration)
        {
            if (instance == null)
            {
                GameObject go = new GameObject("RumbleStopper");
                go.hideFlags = HideFlags.HideAndDontSave;
                DontDestroyOnLoad(go);
                instance = go.AddComponent<RumbleStopper>();
            }
            instance.pad = pad;
            instance.stopTime = Time.unscaledTime + duration;
            instance.enabled = true;
        }

        void Update()
        {
            if (Time.unscaledTime >= stopTime)
            {
                Stop();
            }
        }

        void OnApplicationQuit()
        {
            Stop();
        }

        void Stop()
        {
            if (pad != null) pad.ResetHaptics();
            enabled = false;
        }
    }
#endif
}
