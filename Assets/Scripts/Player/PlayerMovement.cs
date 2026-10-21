using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

    public float speed = 6f;

    //True jika gerakan dikendalikan oleh InputHandler (pola Command),
    //sehingga script ini hanya mengurus rotasi agar tidak bergerak dua kali
    [System.NonSerialized]
    public bool externallyDriven;

    Vector3 movement;
    Animator anim;
    Rigidbody playerRigidbody;
    Camera mainCamera;
    int floorMask;
    const float camRayLength = 100f;

    private void Awake()
    {
        //mendapatkan nilai mask dari layer yang bernama Floor
        floorMask = LayerMask.GetMask("Floor");

        //Mendapatkan komponen Animator
        anim = GetComponent<Animator>();

        //Mendapatkan komponen Rigidbody
        playerRigidbody = GetComponent<Rigidbody>();

        //Interpolasi agar gerakan terlihat halus di frame rate tinggi
        playerRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

        //Cache kamera utama (Camera.main melakukan pencarian setiap dipanggil)
        mainCamera = Camera.main;
    }

    private void FixedUpdate()
    {
        if (!externallyDriven)
        {
            //Mendapatkan nilai input (keyboard / stick kiri gamepad)
            Vector2 input = GameInput.Move;
            Move(input.x, input.y);
            Animating(input.x, input.y);
        }

        Turning();
    }

    //Method player dapat berjalan
    public void Move(float h, float v)
    {
        //Set nilai x dan y
        movement.Set(h, 0f, v);

        //Batasi panjang vector maksimal 1 (gerak diagonal tidak lebih cepat, stick analog tetap halus)
        movement = Vector3.ClampMagnitude(movement, 1f) * speed * Time.fixedDeltaTime;

        //Move to position
        playerRigidbody.MovePosition(playerRigidbody.position + movement);
    }

    void Turning()
    {
        //Bidik memakai stick kanan gamepad jika digunakan
        if (GameInput.TryGetStickAim(out Vector2 aim))
        {
            playerRigidbody.MoveRotation(Quaternion.LookRotation(new Vector3(aim.x, 0f, aim.y)));
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        //Buat Ray dari posisi mouse di layar
        Ray camRay = mainCamera.ScreenPointToRay(GameInput.PointerPosition);

        //Lakukan raycast
        if (Physics.Raycast(camRay, out RaycastHit floorHit, camRayLength, floorMask))
        {
            //Mendapatkan vector dari posisi player dan posisi floorHit
            Vector3 playerToMouse = floorHit.point - transform.position;
            playerToMouse.y = 0f;

            if (playerToMouse.sqrMagnitude < 0.0001f) return;

            //Rotasi player ke arah hit position
            playerRigidbody.MoveRotation(Quaternion.LookRotation(playerToMouse));
        }
    }

    public void Animating(float h, float v)
    {
        bool walking = h != 0f || v != 0f;
        anim.SetBool(IsWalkingHash, walking);
    }
}
