# Survival-Shooter

Mainkan dan menangkan permainan ini! Bertahanlah dari serbuan Zombunny, ZomBear, dan Hellephant selama mungkin.

## Teknologi

| | Sebelumnya | Sekarang |
|---|---|---|
| Engine | Unity 2018.3 | **Unity 6 LTS** (`6000.0.x`) |
| Input | Input Manager lama, hanya keyboard + mouse | **Input System** (keyboard + mouse **dan gamepad**), dengan fallback ke input lama |
| Paket | bawaan editor | `Packages/manifest.json` (Input System, uGUI 2.0, Timeline, Test Framework) |
| C# | gaya C# 4 | C# 9 (expression-bodied members, `out var`, string interpolation, `TryGetComponent`, `UnityEngine.Pool`) |
| CI | – | GitHub Actions + [GameCI](https://game.ci) (Windows, Linux, WebGL) |

## Fitur baru

- **Sistem wave**: tiap wave lebih banyak enemy dan spawn lebih cepat; ZomBear terbuka di wave 2, Hellephant di wave 3. Ada banner "WAVE N" dan "WAVE N CLEARED!".
- **Combo & multiplier skor**: bunuh enemy secara beruntun (jeda < 2,5 detik) untuk multiplier hingga x5.
- **High score tersimpan** (PlayerPrefs) dan tulisan "NEW HIGH SCORE!" saat game over.
- **Menu pause** (Esc / Start): Resume, Restart, pengatur volume, toggle screen shake, Quit. Bisa dinavigasi dengan mouse, keyboard, atau gamepad.
- **Dukungan gamepad twin-stick**: stick kiri untuk bergerak, stick kanan untuk membidik, RT/RB untuk menembak, plus getaran (rumble).
- **Angka damage melayang** di atas enemy (memakai object pool).
- **Health pickup**: enemy punya peluang 15% menjatuhkan health (+20 HP) yang hilang setelah 10 detik.
- **Camera shake** saat menembak dan terkena serangan.
- Gerakan diagonal dan analog pada pola Command (`InputHandler`).

HUD baru dibuat otomatis lewat kode (`GameHud`), jadi scene tidak perlu diubah secara manual.

## Perbaikan bug

- Player bergerak **dua kali per frame** karena `PlayerMovement` dan `InputHandler` sama-sama aktif.
- Klik pertama menembak dua kali dan melewati fire rate (`PlayerShooting` + `InputHandler`).
- Undo (Z) membatalkan gerakan **paling lama**, bukan yang terakhir (Queue → LIFO), dan riwayatnya tumbuh tanpa batas.
- Player yang sudah mati masih bisa digerakkan lewat `InputHandler`.
- Enemy tidak muncul di spawn point (`EnemyFactory` mengabaikan `spawnPoints`).
- Scene `Level_01` belum terdaftar di Build Settings.
- Kamera jitter (follow dipindah ke `LateUpdate`, smoothing tidak bergantung frame rate, interpolasi Rigidbody).
- Trigger animasi `PlayerDead` dipanggil setiap frame.

## Kontrol

| Aksi | Keyboard + Mouse | Gamepad |
|---|---|---|
| Bergerak | WASD / tombol panah | Stick kiri |
| Membidik | Mouse | Stick kanan |
| Menembak | Klik kiri (tahan) | RT / RB |
| Undo gerakan | Z | LB |
| Pause | Esc | Start |

## Cara membuka proyek

1. Install **Unity 6 LTS** (`6000.0.x` atau lebih baru) lewat Unity Hub.
2. Pilih *Add project from disk* lalu buka folder repositori ini.
3. Unity akan meng-upgrade aset lama secara otomatis. Jika muncul dialog untuk mengaktifkan backend Input System, pilih **Yes** lalu restart editor.
4. Buka `Assets/Scenes/Level_01.unity` dan tekan Play.

> Catatan: folder `Build Survival Shooter/` berisi build Windows lama dari Unity 2018 dan belum memuat perubahan di atas. Buat build baru lewat *File → Build Profiles* atau melalui CI.

## CI (GitHub Actions)

Workflow `.github/workflows/build.yml` mem-build game untuk Windows, Linux, dan WebGL setiap kali ada push atau pull request. Hasilnya diunggah sebagai artifact. Tambahkan secret repositori berikut (lihat [panduan aktivasi GameCI](https://game.ci/docs/github/activation)):

- `UNITY_LICENSE`
- `UNITY_EMAIL`
- `UNITY_PASSWORD`

Tanpa secret tersebut, workflow tetap berjalan tetapi build dilewati dengan peringatan.
