using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateScore(float2 newScore) {
        var text = GetComponent<Text>();
        text.text = "Score: ";
        text.text += newScore.ToString();
    }
    void AddScore(float2 newScore) {
        var text = GetComponent<Text>();
        text.text = "Score: ";
        text.text += newScore.ToString();
    }
}
