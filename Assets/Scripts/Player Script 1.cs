using UnityEngine;

public class PlayerScript1 : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            //move left at a rate of 0.3 units per 1/60th of a second
        }
        
        // if press right:
        // go right
        // if press jump,
        // provide upward boost
    }
}
