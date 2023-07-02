using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Spawner : MonoBehaviour
{

    [SerializeField] private float spawnenRate = 1f;

    [SerializeField] private GameObject[] enemyPrefabs;

    [SerializeField] private bool canSpawn = true;

    private void Start()
    {
        StartCoroutine( Spawner() );
    }

    private IEnumerator Spawner()
    {
        spawnenRate += Random.Range(0, 4);
        WaitForSeconds wait = new WaitForSeconds(spawnenRate);

        while (canSpawn)
        {
            yield return wait;
            int rand = Random.Range(0, enemyPrefabs.Length);
            GameObject enemyToSpawn = enemyPrefabs[rand];
            Instantiate(enemyToSpawn, transform.position, Quaternion.identity);
        }

    }

}
