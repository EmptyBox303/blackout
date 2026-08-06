using UnityEngine;

public class DeathTrigger : MonoBehaviour
{

    public Collider2D coll;
    public Area belongingArea;
    public bool playDeathAnimation;

    private Player _p;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _p = Player.p;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player.p.Death(playDeathAnimation);
        }
    }
}
