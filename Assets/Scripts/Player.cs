using System;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{

    public static Player p;
    //some private variables for convenience
    private Transform _t;
    private Rigidbody2D _rb;
    private Keyboard _k;
    private Dictionary<KeyControl, bool> _inputStates;
    
    //this is to keep track of currently active coyoteTimes;
    private Coroutine _coyoteTime;
    private Coroutine _jumpBuffer;
    private Coroutine _jumpHold;
    
    private bool _hasJumped;

    [FormerlySerializedAs("_state")] public PlayerState state;
    public GameObject contactind;
    //terminal velocity downwards
    [Tooltip("terminal velocity")]
    public float maxFallingSpeed;

    [Tooltip("buffers")]
    public float coyoteTime;
    public float jumpBuffer;
    public float jumpExtendTime;
    
    
    [Tooltip("speed")]
    public float walkingSpeed;
    public float jumpSpeed;
    
    [Tooltip("gravity")]
    public float normalGravity;
    public float holdGravity;

    
    [Tooltip("acceleration")]
    public float airborneAccTime;
    public float groundedAccTime;
    [Tooltip("decceleration")] 
    //essentially, how long it takes for the player to naturally deccelerate from full speed
    public float airborneDecTime;
    public float groundedDecTime;
    
    
    void Start()
    {
        _t = transform;
        p = this;
        _k = Keyboard.current;
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = normalGravity;
        _coyoteTime = null;
        _hasJumped = true;
        _jumpBuffer = null;
        _jumpHold = null;
        _inputStates = new Dictionary<KeyControl, bool>();

        _inputStates[_k.aKey] = false;
        _inputStates[_k.dKey] = false;
        _inputStates[_k.spaceKey] = false;
    }

    // Update is called once per frame
    void Update()
    {
        //restructure so that updates only collect inputs
        _inputStates[_k.aKey] = _k.aKey.isPressed;
        _inputStates[_k.dKey] = _k.dKey.isPressed;
        _inputStates[_k.spaceKey] = _k.spaceKey.isPressed;
        
    }

    void FixedUpdate()
    {
        int lateralMovement = 0;
        if (_inputStates[_k.aKey]) lateralMovement = -1;
        if (_inputStates[_k.dKey]) lateralMovement += 1;
        
        float xSpeed = _rb.linearVelocity.x;

        if (lateralMovement == 0)
        {
            //the goal is to decelerate in specified amount of time
            //with each FixedUpdate(dt) and goal time for full speed T, 
            float fullSpeedDecTime = (state == PlayerState.grounded) ? groundedDecTime : airborneDecTime;
            float decceleratedSpeed = xSpeed - Math.Sign(xSpeed) * walkingSpeed * (Time.fixedDeltaTime/fullSpeedDecTime);

            xSpeed = (Math.Sign(decceleratedSpeed) != Math.Sign(xSpeed)) ? 
                0 : decceleratedSpeed;
        }
        else
        {
            
            float fullSpeedAccTime =  (state == PlayerState.grounded) ? groundedAccTime : airborneAccTime;
            float acceleratedSpeed = xSpeed + lateralMovement * walkingSpeed * (Time.fixedDeltaTime / fullSpeedAccTime);
            //print(acceleratedSpeed);
            xSpeed = Math.Clamp(acceleratedSpeed, -walkingSpeed, walkingSpeed);
        }
        
        _rb.linearVelocity = new Vector2(xSpeed,_rb.linearVelocityY);
        

        if (_inputStates[_k.spaceKey])
        {
            if(!_hasJumped &&
               (
                   state == PlayerState.grounded ||
                   _coyoteTime != null
               )
              )
            {
                //with some other condition, permit the jump
                Jump();
            }
            else
            {
                _jumpBuffer = StartCoroutine(JumpBuffer());
            }
        }
        if (_rb.linearVelocity.y < -maxFallingSpeed)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, -maxFallingSpeed);
        }

        _rb.gravityScale = (_jumpHold != null) ? holdGravity : normalGravity;
        
    }

    void Jump()
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x,jumpSpeed);
        _hasJumped = true;
        _jumpHold = StartCoroutine(HoldJump());
    }
    
    void OnCollisionEnter2D(Collision2D collision){
        print(_rb.linearVelocity);
        
        if (collision.gameObject.CompareTag("Ground"))
        {
            state = PlayerState.grounded;
            _hasJumped = false;
            CeaseIfActive(ref _coyoteTime);
            
            if (_jumpBuffer != null)
            {
                Jump();
                CeaseIfActive(ref _jumpBuffer);
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        print(_rb.linearVelocity);
        if (collision.gameObject.CompareTag("Ground"))
        {
            state = PlayerState.airborne;
            CeaseIfActive(ref _coyoteTime);
            _coyoteTime = StartCoroutine(CoyoteTime());
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        contactind.transform.position = collision.contacts[0].point;
        if(_rb.linearVelocity.magnitude != 0)
            print(_rb.linearVelocity);
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

    IEnumerator JumpBuffer()
    {
        yield return new WaitForSeconds(jumpBuffer);
        _jumpBuffer = null;
    }

    IEnumerator HoldJump()
    {
        for (float t = 0; t < jumpExtendTime; t += Time.fixedDeltaTime)
        {
            if (!_k.spaceKey.isPressed) break;
            yield return new WaitForFixedUpdate();
        }
        _jumpHold = null;
    }

    public void Death()
    {
        
    }


    public enum PlayerState
    {
        grounded,
        airborne,
        phasing
    }
}
