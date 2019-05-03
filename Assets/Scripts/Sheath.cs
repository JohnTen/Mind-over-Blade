using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sheath : MonoBehaviour
{
	[SerializeField] int numberOfKnife;
	[SerializeField] Transform launchPosition;
	[SerializeField] Transform[] sheathPositions;
	[SerializeField] Queue<Knife> knifesInSheath;
	[SerializeField] Queue<Knife> knifesInAir;
	[SerializeField] List<Knife> allKnifes;
	[SerializeField] Knife knifePrefab;

	public float ReloadSpeed { get; set; } = 3;
	public Transform LaunchPosition => launchPosition;

	bool reloading;

	private void Awake()
	{
		knifesInSheath = new Queue<Knife>();
		knifesInAir = new Queue<Knife>();
		allKnifes = new List<Knife>();

		for (int i = 0; i < numberOfKnife; i++)
		{
			var knife = Instantiate(knifePrefab.gameObject).GetComponent<Knife>();
			knife.transform.parent = sheathPositions[i];
			knife.transform.position = sheathPositions[i].position;
			knife.transform.rotation = transform.rotation;
			knife.SetSheath(this);
			knifesInSheath.Enqueue(knife);
			allKnifes.Add(knife);
		}
	}

	public void UpdateFacingDirection(bool right)
	{
		if (right && this.transform.eulerAngles.y != 0)
		{
			this.transform.rotation = Quaternion.Euler(0, 0, 0);
			foreach (var item in knifesInSheath)
			{
				item.transform.rotation = this.transform.rotation;
			}
		}
		else if(!right && this.transform.eulerAngles.y == 0)
		{
			this.transform.rotation = Quaternion.Euler(0, 180, 0);
			foreach (var item in knifesInSheath)
			{
				item.transform.rotation = this.transform.rotation;
			}
		}
	}

	public Knife TakeKnife()
	{
		if (reloading || knifesInSheath.Count <= 0)
			return null;

		StartCoroutine(ReloadKnife());
		knifesInAir.Enqueue(knifesInSheath.Peek());
		knifesInSheath.Peek().transform.parent = null;
		return knifesInSheath.Dequeue();
	}

	public void PutBackKnife(Knife knife)
	{
		knife.transform.parent = sheathPositions[knifesInSheath.Count];
		knife.transform.position = sheathPositions[knifesInSheath.Count].position;
		knife.transform.rotation = transform.rotation;
		knifesInSheath.Enqueue(knife);
	}

	public void WithdrawLastKnife()
	{
		if (knifesInAir.Count <= 0) return;
		knifesInAir.Dequeue().Withdraw();
	}

	IEnumerator ReloadKnife()
	{
		reloading = true;

		float timer = 0;

		while (timer < 1)
		{
			yield return null;
			int index = 0;
			timer += Time.deltaTime * ReloadSpeed;
			foreach (var item in knifesInSheath)
			{
				item.transform.parent = sheathPositions[index];
				item.transform.position = Vector3.Lerp(sheathPositions[index + 1].position, sheathPositions[index].position, timer);
				index++;
				if (index >= 2) break;
			}
		}

		reloading = false;
	}
}
