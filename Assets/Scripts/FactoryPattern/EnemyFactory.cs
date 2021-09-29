using System;
using UnityEngine;

public class EnemyFactory : MonoBehaviour, IFactory
{

    [SerializeField]
    public GameObject[] enemyPrefab;
    public Transform[] spawnPoints;

    public GameObject FactoryMethod(int tag)
    {
        GameObject enemy = Instantiate(enemyPrefab[tag]);
        return enemy;
    }
}