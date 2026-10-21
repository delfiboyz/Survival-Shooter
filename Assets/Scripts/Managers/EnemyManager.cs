using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

//Spawner berbasis wave: setiap wave memiliki lebih banyak enemy, jeda spawn lebih cepat,
//dan jenis enemy yang lebih kuat terbuka seiring bertambahnya wave
public class EnemyManager : MonoBehaviour
{
    public PlayerHealth playerHealth;

    [Tooltip("Jeda spawn awal (detik) pada wave pertama")]
    public float spawnTime = 3f;
    public Transform[] spawnPoints;

    [Header("Wave")]
    public int enemiesInFirstWave = 5;
    public int extraEnemiesPerWave = 3;
    [Range(0.5f, 1f)]
    public float spawnTimeMultiplierPerWave = 0.88f;
    public float minSpawnTime = 0.6f;
    public float timeBetweenWaves = 4f;
    [Tooltip("Batas jumlah enemy hidup bersamaan")]
    public int maxAliveEnemies = 25;

    [SerializeField]
    MonoBehaviour factory;
    IFactory Factory => factory as IFactory;

    //Event untuk UI: nomor wave yang dimulai
    public static event Action<int> WaveStarted;
    //Event untuk UI: nomor wave yang selesai
    public static event Action<int> WaveCleared;

    public static int CurrentWave { get; private set; }

    readonly List<EnemyHealth> aliveEnemies = new List<EnemyHealth>();

    void Start()
    {
        CurrentWave = 0;

        if (Factory == null)
        {
            Debug.LogError("EnemyManager: field 'factory' harus berisi komponen yang mengimplementasikan IFactory.", this);
            enabled = false;
            return;
        }

        StartCoroutine(WaveLoop());
    }

    IEnumerator WaveLoop()
    {
        //Jeda awal sebelum wave pertama
        yield return new WaitForSeconds(spawnTime);

        while (!playerHealth.IsDead)
        {
            CurrentWave++;
            WaveStarted?.Invoke(CurrentWave);

            int enemiesToSpawn = enemiesInFirstWave + extraEnemiesPerWave * (CurrentWave - 1);
            float interval = Mathf.Max(minSpawnTime, spawnTime * Mathf.Pow(spawnTimeMultiplierPerWave, CurrentWave - 1));

            for (int i = 0; i < enemiesToSpawn; i++)
            {
                //Tunggu jika terlalu banyak enemy di arena
                while (CountAlive() >= maxAliveEnemies)
                {
                    yield return null;
                }

                if (playerHealth.IsDead) yield break;

                Spawn();
                yield return new WaitForSeconds(interval);
            }

            //Tunggu sampai semua enemy di wave ini dikalahkan
            while (CountAlive() > 0)
            {
                if (playerHealth.IsDead) yield break;
                yield return null;
            }

            WaveCleared?.Invoke(CurrentWave);
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    void Spawn()
    {
        //Jika player telah mati maka tidak membuat enemy baru
        if (playerHealth.currentHealth <= 0)
        {
            return;
        }

        GameObject enemy = Factory.FactoryMethod(PickEnemyType());

        //Spawn point milik EnemyManager (opsional) menimpa posisi dari factory
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            enemy.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        }

        if (enemy.TryGetComponent(out EnemyHealth enemyHealth))
        {
            aliveEnemies.Add(enemyHealth);
        }
    }

    //Pilih jenis enemy: jenis yang lebih kuat terbuka setiap wave
    int PickEnemyType()
    {
        int unlocked = Mathf.Clamp(CurrentWave, 1, Factory.Count);

        //Bobot: enemy terlemah paling sering muncul, enemy terkuat makin sering di wave tinggi
        float totalWeight = 0f;
        for (int i = 0; i < unlocked; i++) totalWeight += Weight(i);

        float roll = Random.value * totalWeight;
        for (int i = 0; i < unlocked; i++)
        {
            roll -= Weight(i);
            if (roll <= 0f) return i;
        }
        return unlocked - 1;
    }

    float Weight(int type)
    {
        return 1f + CurrentWave * 0.25f * type / Mathf.Max(1, Factory.Count - 1) + (type == 0 ? 1f : 0f);
    }

    int CountAlive()
    {
        aliveEnemies.RemoveAll(e => e == null || e.IsDead);
        return aliveEnemies.Count;
    }
}
