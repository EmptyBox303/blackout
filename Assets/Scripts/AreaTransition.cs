using UnityEngine;
using UnityEngine.Serialization;

public class AreaTransition : MonoBehaviour
{
    public GameObject nextArea;

    public bool verticalEntry;

    public Area lesserArea;
    public Area greaterArea;

    private bool _axisCross;
    private bool _axisCrossUpdate;
    private Player p;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        p = Player.p;
        if (nextArea != null)
        { 
            Area nextA = Instantiate(nextArea).GetComponent<Area>();
            Area prevA = transform.parent.gameObject.GetComponent<Area>();
            bool comp = (verticalEntry)
                ? (nextA.transform.position.y > prevA.transform.position.y)
                : (nextA.transform.position.x > prevA.transform.position.x);
            //axisCross evaluates if player has crossed into higher value(x or y)
            _axisCross = (verticalEntry)
                ? (p.transform.position.y > transform.position.y)
                : (p.transform.position.x > transform.position.x);
            greaterArea = (comp) ? nextA : prevA;
            lesserArea = (comp) ? prevA : nextA;
            
        }
        
    }
    //

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerStay2D(Collider2D other)
    {
        //on player entry
        //depending on vertical entry
        //
        if (other.CompareTag("Player"))
        {
            bool newAxisCross = (verticalEntry)
                ? (p.transform.position.y > transform.position.y)
                : (p.transform.position.x > transform.position.x);
            if (newAxisCross != _axisCross)
            {
                _axisCross = newAxisCross;
                if (!verticalEntry)
                {
                    Player.p.currentArea = 
                        (transform.position.x > Player.p.transform.position.x) ? 
                            lesserArea : greaterArea;
                }
                else
                {
                    Player.p.currentArea = 
                        (transform.position.y > Player.p.transform.position.y) ? 
                            lesserArea : greaterArea;
                    //some function that enforces transition
                }

                StartCoroutine(Player.p.TransitionPause());
            }
            
        }
        
    }
}
