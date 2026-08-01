using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    
    //some private variables for convenience
    private Transform _t;
    private Rigidbody2D _rb;
    private Keyboard _k;
    
    //terminal velocity downwards
    [Tooltip("terminal velocity")]
    public float maxFallingSpeed;
    
    public float walkingSpeed;
    public float jumpSpeed;
    
    
    
    
    
    
    void Start()
    {
        _t = transform;
        _k = Keyboard.current;
        _rb=GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //Input.GetKey is not really working in Unity's new input system
        
        //go left when press left
        if (_k.aKey.wasPressedThisFrame)
        {
            print("akey");
        }
        if (_k.aKey.isPressed)
        {
            _t.position += walkingSpeed * Time.deltaTime * Vector3.left;
        }

        //go right when press right
        if (_k.dKey.isPressed)
        {
            _t.position += walkingSpeed * Time.deltaTime * Vector3.right;
        }
        
        //as of now, Jump whenever press space
        if (_k.spaceKey.isPressed)
        {
            //with some other condition, permit the jump
            _rb.linearVelocity = new Vector2(0,jumpSpeed);
        }
        
        
    }
}
