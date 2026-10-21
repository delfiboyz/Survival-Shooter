using UnityEngine;

public class EnemyFactory : MonoBehaviour, IFactory
{
    public GameObject[] enemyPrefab;
    public Transform[] spawnPoints;

    public int Count => enemyPrefab.Length;

    public GameObject FactoryMethod(int tag)
    {
        tag = Mathf.Clamp(tag, 0, enemyPrefab.Length - 1);
        GameObject prefab = enemyPrefab[tag];

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            //Setiap jenis enemy punya spawn point sendiri jika jumlahnya sama,
            //selain itu pilih spawn point secara acak
            Transform spawnPoint = spawnPoints.Length == enemyPrefab.Length
                ? spawnPoints[tag]
                : spawnPoints[Random.Range(0, spawnPoints.Length)];
            return Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        }

        return Instantiate(prefab);
    }
}
