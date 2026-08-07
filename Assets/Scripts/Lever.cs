using UnityEngine;

public class Lever : InputSwitch
{

    public Collider2D interactTrigger;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player.p.nearInteractable = this;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (Player.p.nearInteractable == this)
            {
                Player.p.nearInteractable = null;
            }
        }
    }
    
    public override void Interact()
    {
        Toggle();
        if (active)
        {
            sr.color = Color.green;
        }
        else
        {
            sr.color = Color.red;
        }
    }

}
