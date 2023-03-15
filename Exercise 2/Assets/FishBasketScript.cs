using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FishBasketScript : MonoBehaviour
{
	private GameState gameState = GameState.loading;
	public Text Text;
	public GameObject[] droppers;
	private int score;
	private const float maxspeed = .88f;
	private float speed = .1f;
	private const string horizontalAxis = "Horizontal";
	private const string verticalAxis = "Vertical";

	public enum GameState
	{
		loading,
		playing,
		paused,
		ended,
	}

	// Start is called before the first frame update
	void Start()
	{
		gameState = GameState.playing;
		Debug.Log("Fish basket created");
	}

	// Update is called once per frame
	void Update()
	{
		if(gameState == GameState.playing)
			Move();
	}

	void Move()
	{
		float x = Input.GetAxis(horizontalAxis);
		float y = Input.GetAxis(verticalAxis);
		Vector2 direction = new Vector2(x, y);
		transform.Translate(Vector2.ClampMagnitude(direction * speed, maxspeed));
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if(collision.isTrigger)
		{
			gameState = GameState.ended;
			Text.alignment = TextAnchor.MiddleCenter;
			Text.fontSize = 18;
			Text.color = Color.red;
			foreach(var item in droppers)
				item.GetComponent<Dropper>().alive = false;
			Text.text = ("Game Over\n") + Text.text;

		}
		else
			Text.text = $"Score: {++score}";
		collision.gameObject.SetActive(false);
		Destroy(collision.gameObject);
	}


}
