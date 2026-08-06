using System.Collections;
using UnityEngine;
using TMPro;

public class HintText : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TextMeshPro text;
    public float characterDelay;
    public static HintText ht;

    private Coroutine _printActive;
    void Awake()
    {
        _printActive = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClearText()
    {
        text.text = "";
    }

    public void ProduceHintText(string s)
    {
        if (_printActive == null)
        {
            _printActive = StartCoroutine(Hint(s));
        }
    }
    
    IEnumerator Hint(string s)
    {
        ClearText();
        foreach (char c in s)
        {
            text.text += c;
            yield return new WaitForSeconds(characterDelay);
        }
        _printActive = null;
    }
}
