using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    
    //some private variables for convenience
    private Transform _t;
    private Rigidbody2D _rb;
    private Keyboard _k;
    private playerState _state;
    
    //this is to keep track of currently active coyoteTimes;
    private Coroutine _coyoteTime;
    private bool _hasJumped;
    
    
    //terminal velocity downwards
    [Tooltip("terminal velocity")]
    public float maxFallingSpeed;

    [Tooltip("Amount of time that a jump input can be received after leaving solid ground")]
    public float coyoteTime;
    
    public float walkingSpeed;
    public float jumpSpeed;
    
    
    void Start()
    {
        _t = transform;
        _k = Keyboard.current;
        _rb = GetComponent<Rigidbody2D>();
        _coyoteTime = null;
        _hasJumped = true;
    }

    // Update is called once per frame
    void Update()
    {
        //Input.GetKey is not really working in Unity's new input system
        
        //go left when press left
        if (_k.aKey.isPressed)
        {
            _t.position += walkingSpeed * Time.deltaTime * Vector3.left;
        }

        //go right when press right
        if (_k.dKey.isPressed)
        {
            _t.position += walkingSpeed * Time.deltaTime * Vector3.right;
        }
        
        
        if (_k.spaceKey.wasPressedThisFrame && 
            !_hasJumped &&
            (
                _state == playerState.grounded ||
                _coyoteTime != null
                )
            )
        {
            //with some other condition, permit the jump
            _rb.linearVelocity = new Vector2(0,jumpSpeed);
            _hasJumped = true;
        }
        
        
        
    }
    
    void OnCollisionEnter2D(Collision2D collision){
        if (collision.gameObject.CompareTag("Ground"))
        {
            _state = playerState.grounded;
            _hasJumped = false;
            CeaseIfActive(ref _coyoteTime);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        
        if (collision.gameObject.CompareTag("Ground"))
        {
            _state = playerState.airborne;
            CeaseIfActive(ref _coyoteTime);
            _coyoteTime = StartCoroutine(CoyoteTime());
        }
    }

    void CeaseIfActive(ref Coroutine c)
    {
        if (c != null)
        {
            StopCoroutine(c);
            c = null;
        }
    }

    //coyote timer
    //build in delay
    IEnumerator CoyoteTime()
    {
        yield return new WaitForSeconds(coyoteTime);
        _coyoteTime = null;
    }


    enum playerState
    {
        grounded,
        airborne,
        phasing
    }
}
