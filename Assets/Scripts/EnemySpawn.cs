using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
	[SerializeField] Enemy _enemyPrefab;
	[SerializeField] float _range;
	Enemy enemyReference;
	
    void Update()
    {
		if (enemyReference == null)
		{
			Spawn();
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(transform.position, new Vector3(_range * 2, 1, 1));
	}

	void Spawn()
	{
		enemyReference = Instantiate(_enemyPrefab.gameObject).GetComponent<Enemy>();
		enemyReference.transform.position = this.transform.position + Random.Range(-_range, _range) * Vector3.right;
		
		if (Random.value > 0.5f)
		{
			enemyReference.FirstTarget = transform.position - _range * Vector3.right;
			enemyReference.LastTarget = transform.position + _range * Vector3.right;
		}
		else
		{

			enemyReference.FirstTarget = transform.position + _range * Vector3.right;
			enemyReference.LastTarget = transform.position - _range * Vector3.right;
		}
	}
}
