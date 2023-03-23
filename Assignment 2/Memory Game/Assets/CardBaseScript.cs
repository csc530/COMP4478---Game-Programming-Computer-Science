using UnityEngine;

public class CardBaseScript : MonoBehaviour
{
    public Sprite Face;
    public Sprite Back;
    public bool IsFlipped = false;

    private SpriteRenderer SpriteRenderer;

    // Start is called before the first frame update
    void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        var sprite = IsFlipped ? Face : Back;
        SpriteRenderer.sprite = sprite;
    }
}