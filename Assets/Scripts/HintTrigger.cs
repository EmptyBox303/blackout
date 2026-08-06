using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class HintTrigger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public HintText hintText;
    public Collider2D trigger;

    public string hint;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            hintText.ProduceHintText(hint);
            trigger.enabled = false;
        }
        
    }
    
    
    
    
    
}
