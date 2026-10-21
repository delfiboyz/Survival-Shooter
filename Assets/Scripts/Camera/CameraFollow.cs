using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothing = 5f;
    Vector3 offset;
    Vector3 followPosition;

    private void Start()
    {
        //mendapatkan offset antara target dan camera
        offset = transform.position - target.position;
        followPosition = transform.position;
    }

    //LateUpdate: kamera bergerak setelah semua object di-update sehingga tidak bergetar (jitter)
    private void LateUpdate()
    {
        //mendapat posisi untuk camera
        Vector3 targetCamPos = target.position + offset;

        //smoothing yang tidak bergantung pada frame rate
        float t = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
        followPosition = Vector3.Lerp(followPosition, targetCamPos, t);

        //Camera shake ditambahkan di atas posisi follow tanpa ikut terakumulasi
        transform.position = followPosition + CameraShake.Offset;
    }
}
