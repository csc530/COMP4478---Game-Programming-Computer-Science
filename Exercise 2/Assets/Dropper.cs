using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.FilePathAttribute;
using Random = UnityEngine.Random;

public class Dropper : MonoBehaviour
{
	public new Camera camera;

	/// <summary>
	/// Prefab sprite to drop into camera area
	/// </summary>

	public GameObject SpritePrefab;
	/// <summary>
	/// The initial time between drops in milliseconds
	/// </summary>


	public ushort InitialDelay = 5;
	/// <summary>
	/// The highest (longest) set of time in milliseconds to delay before dropping another prefab sprite
	/// </summary>


	public ushort MaxDelay = 60 * 5;
	/// <summary>
	/// The lowest (shortest) amount of time between prefab drops
	/// </summary>

	public ushort MinDelay = 1;
	/// <summary>
	/// List of alternate delay intervals, only used when using an alternating <see cref="DelayType"/>
	/// </summary>

	public ushort[] AlternateDelay;
	/// <summary>
	/// The step when increasing or decresing delay amount
	/// </summary>

	public ushort DelayStep = 100;

	public DelayType Type;

	/// <summary>
	/// Continue dropping the prefab, restarting (true -> false -> true) will reset the delay to to its initial
	/// </summary>
	public bool alive = false;


	/// <summary>
	/// The # of times a drop has occured, used for tracking alternateDelay element index and calculating 
	/// </summary>
	private ushort delayIndex = 0;
	private Coroutine dropCoroutine;

	public enum DelayType
	{
		@static,
		increasing,
		decresing,
		alternating
	}

	private void Start()
	{
		if(dropCoroutine == null && alive)
			dropCoroutine = StartCoroutine(DropPrefab());
	}

	private void LateUpdate()
	{
		if(droppedPrefabs.Count > 0)
			RemoveOffScreenPrefabs();
		if(!alive)
		{
			droppedPrefabs.FindAll(prefab => prefab != null).ForEach(prefab => Destroy(prefab));
			droppedPrefabs.Clear();
			StopAllCoroutines();
			dropCoroutine = null;
		}
	}

	private void RemoveOffScreenPrefabs()
	{
		droppedPrefabs.FindAll(prefab => prefab != null).FindAll(prefab =>
		{
			var position = Camera.main.WorldToViewportPoint(prefab.transform.position);
			(float x, float y) = (position.x, position.y);
			return x < 0 || x > 1 || y < 0;
		}).ForEach(prefab => Remove(prefab));
	}

	private void Remove(GameObject gameObject)
	{
		droppedPrefabs.Remove(gameObject);
		gameObject.SetActive(false);
		Object.Destroy(gameObject);
	}

	private int GetDelay()
	{
		int delay = Type switch
		{
			DelayType.@static => InitialDelay,
			DelayType.increasing => (InitialDelay + (delayIndex * DelayStep)),
			DelayType.decresing => (InitialDelay - delayIndex * DelayStep),
			DelayType.alternating => AlternateDelay[delayIndex % AlternateDelay.Length],
			_ => throw new System.ArgumentException($"Invalid delay type: {Type}", nameof(DelayType)),
		};
		delayIndex++;
		return math.clamp(delay, MinDelay, MaxDelay);
	}

	private List<GameObject> droppedPrefabs = new();

	/// <summary>
	/// Drop a prefab into the camer after set time
	/// </summary>
	IEnumerator DropPrefab()
	{
		int i = 0;
		while(alive)
		{
			var delay = GetDelay();
			yield return new WaitForSecondsRealtime(delay / 1000 + Time.deltaTime);
			var prefab = Instantiate(SpritePrefab);
			prefab.transform.position = RandomDropperLocation();
			//Debug.Log($"fishform\n({fish.transform.position.x}, {fish.transform.position.y})", fish);
			//print($"Delayed: {delay}\n");
			print(i++ + "_-Alive: " + alive);
			droppedPrefabs.Add(prefab);
			prefab.name = prefab.name.Replace("(Clone)", droppedPrefabs.Count.ToString());
		}
		yield return null;
	}

	private Vector3 RandomDropperLocation()
	{
		float x = Random.Range(0, 1.0f);
		float y = Random.Range(1, 1.5f);
		Vector3 location = new Vector3(x, y, Camera.main.nearClipPlane);
		//Debug.Log($"Pre-Transform\n({location.x}, {location.y})");
		location = Camera.main.ViewportToWorldPoint(location);
		//Debug.Log($"Post-form\n({location.x}, {location.y})");

		return location;
	}
}