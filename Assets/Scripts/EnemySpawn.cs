using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
	[SerializeField] Enemy _enemyPrefab;
	Enemy enemyReference;

    void Start()
    {
        
    }
	
    void Update()
    {
		if (enemyReference == null)
		{
			Spawn();
		}
	}

	void Spawn()
	{
		enemyReference = Instantiate(_enemyPrefab.gameObject).GetComponent<Enemy>();

		enemyReference.transform.position = this.transform.position;

	}
}
