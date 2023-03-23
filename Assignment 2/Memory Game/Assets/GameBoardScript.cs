using System;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class GameBoardScript : MonoBehaviour
{
    public AssetReference[] SpriteCardFaces;
    public AssetReference SpriteCardBack;
    public int NumberOfCards = 0;
    private GameObject[,] Cards;
    private GameObject CardPrefab;
    private Grid Grid;
    private bool Win { get; set; } = false;
    private CardBaseScript PreviousCard { get; set; } = null;
    private CardBaseScript CurrentCard { get; set; } = null;
    private bool pauseScreen = false;
    private float elapsedPause = 0;

    private void Awake()
    {
        if(NumberOfCards % 2 != 0)
            throw new DataException("The number of cards must be even\nReceived: " + NumberOfCards);

        Grid = this.GetComponent<Grid>();
        CardPrefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/CardBase.prefab").WaitForCompletion();

        if(Math.Sqrt(NumberOfCards) % 1 == 0)
            InitCards();
        else
            //jaggedarray tings
            throw new DataException("The number of cards must be have a square root\nReceived: " + NumberOfCards);

        GenerateCardFaces();
        //place cards on screen
        PlaceCards();
    }

    private bool EmptySpaces(bool[,] arr)
    {
        for(var i = 0; i < arr.GetLength(0); i++)
            for(var j = 0; j < arr.GetLength(1); j++)
                if(!arr[i, j])
                    return true;
        return false;
    }

    private void GenerateCardFaces()
    {
        //set card faces
        var hasCardFace = new bool[Cards.GetLength(0), Cards.GetLength(1)];

        var index = 0;
        while(index < SpriteCardFaces.Length)
            for(var i = 0; i < Cards.GetLength(0); i++)
                for(var j = 0; j < Cards.GetLength(1); j++)
                {
                    if(!EmptySpaces(hasCardFace) || index >= SpriteCardFaces.Length)
                        return;
                    else if(hasCardFace[i, j])
                        continue;

                    var cardFace = Addressables.LoadAssetAsync<Sprite>(SpriteCardFaces[index]).WaitForCompletion();

                    var card = Cards[i, j].GetComponent<CardBaseScript>();
                    card.Face = cardFace;
                    Cards[i, j].name += $" - {cardFace.name}";
                    hasCardFace[i, j] = true;

                    PlaceMatchingCard(hasCardFace, cardFace);

                    index++;
                }
    }


    private void PlaceMatchingCard(bool[,] occupiedPlaces, Sprite cardFace)
    {
        while(true)
        {
            var xPos = Random.Range(0, Cards.GetLength(0));
            var yPos = Random.Range(0, Cards.GetLength(1));
            if(occupiedPlaces[xPos, yPos])
                continue;
            occupiedPlaces[xPos, yPos] = true;

            Cards[xPos, yPos].GetComponent<CardBaseScript>().Face = cardFace;
            Cards[xPos, yPos].name += $" -  {cardFace.name}";
            return;
        }
    }

    private void InitCards()
    {
        Sprite cardBack = Addressables.LoadAssetAsync<Sprite>(SpriteCardBack).WaitForCompletion();
        var size = (int)Math.Sqrt(NumberOfCards);
        Cards = new GameObject[size, size];
        for(var i = 0; i < Cards.GetLength(0); i++)
            for(var j = 0; j < Cards.GetLength(1); j++)
            {
                Cards[i, j] = Instantiate(CardPrefab, transform);
                Cards[i, j].GetComponent<CardBaseScript>().Back = cardBack;
                Cards[i, j].GetComponent<CardBaseScript>().IsFlipped = false;
                Cards[i, j].name = $"Card {i.ToString()},{j.ToString()}";
            }
    }

    private void PlaceCards()
    {
        for(int i = 0; i < Cards.GetLength(0); i++)
            for(int j = 0; j < Cards.GetLength(1); j++)
                Cards[i, j].transform.position = Grid.CellToWorld(new Vector3Int(i, j));
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(pauseScreen)
        {
            elapsedPause += Time.deltaTime;
            if(elapsedPause > 2.5)
            {
                print($"Paused for: {PreviousCard.name}, {CurrentCard.name}");
                pauseScreen = false;
                PreviousCard.IsFlipped = false;
                CurrentCard.IsFlipped = false;
                PreviousCard.IsFlipped = false;
                PreviousCard = null;
            }
        }

        if(!Input.GetMouseButtonDown((int)MouseButton.Left) || Input.touchSupported && Input.touchCount == 0)
            return;

        var (x, y) = GetClickPosition();

        if(x >= Cards.GetLength(0) || y >= Cards.GetLength(1) || x < 0 || y < 0)
            return;

        CurrentCard = Cards[x, y].GetComponent<CardBaseScript>();
        CurrentCard.IsFlipped = true;

        //if no prior card has been flipped over
        //keep this one exposed/flipped
        if(PreviousCard == null)
        {
            print($"Displaying first card: {CurrentCard.name}");
            PreviousCard = CurrentCard;
        }
        //if they flipped a matching card keep both flipped over
        else if(CurrentCard.Face == PreviousCard.Face && CurrentCard != PreviousCard)
        {
            print($"Matched Cards: {CurrentCard.name}, {PreviousCard.name}");
            PreviousCard = null;

            //check if they won/correctly matched all cards
            Win = true;
            foreach(var item in Cards)
                if(!item.GetComponent<CardBaseScript>().IsFlipped)
                    Win = false;
        }
        else
        {
            pauseScreen = true;
            PreviousCard.IsFlipped = true;
            CurrentCard.IsFlipped = true;
        }
    }

    private (int x, int y) GetClickPosition()
    {
        Vector3 selectPos = new();
        if(Input.GetMouseButtonDown((int)MouseButton.Left))
            selectPos = Input.mousePosition;
        else if(Input.touchSupported && Input.touchCount > 0)
            selectPos = Input.GetTouch(0).position;
        selectPos = Camera.main.ScreenToWorldPoint(selectPos);

        var gridPos = Grid.WorldToCell(selectPos);
        var (x, y) = (gridPos.x, gridPos.y);
        print($"grid clicked: ({x},{y})");

        for(int i = 0; i < Cards.GetLength(0); i++)
            for(int j = 0; j < Cards.GetLength(1); j++)
                print(Grid.CellToWorld(new Vector3Int(i, j)));

        return (x, y);
    }
}