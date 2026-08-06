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
    private Collider2D _coll;
    private Keyboard _k;
    private Dictionary<KeyControl, bool> _inputStates;
    
    //this is to keep track of currently active coyoteTimes;
    private Coroutine _coyoteTime;
    private Coroutine _jumpBuffer;
    private Coroutine _jumpHold;
    private Coroutine _dashActive;
    
    private bool _hasJumped;
    private bool _hasDashed;
    public Vector2 _faceDirection;
    private bool _recentFaceRight;

    [Header("Debug")]
    public PlayerState state;
    public GameObject contactind;
    //terminal velocity downwards
    [Header ("Falling")]
    [Tooltip("terminal velocity")]
    public float maxFallingSpeed;

    public float maxLateralSpeed;
    public float maxUpSpeed;

    [Header("buffers")]
    [Tooltip("buffers")]
    public float coyoteTime;
    public float jumpBuffer;
    public float jumpExtendTime;
    
    [Header("Speed")]
    [Tooltip("speed")]
    public float walkingSpeed;
    public float jumpSpeed;
    
    
    [Header("Gravity")]
    [Tooltip("gravity")]
    public float normalGravity;
    public float holdGravity;

    [Header("Accelerations")]
    [Tooltip("acceleration")]
    public float airborneAccTime;
    public float groundedAccTime;
    [Tooltip("decceleration")] 
    //essentially, how long it takes for the player to naturally deccelerate from full speed
    public float airborneDecTime;
    public float groundedDecTime;

    [Tooltip("Dash")] 
    public float dashSpeed;
    public float dashDuration;
    public float dashPause;
    public ParticleSystem dashParticles;

    [Header("Transition")] 
    public float transitionTime;
    

    [Header("Area")] 
    public Area currentArea;
    
    
    void Awake()
    {
        _t = transform;
        p = this;
        _k = Keyboard.current;
        _rb = GetComponent<Rigidbody2D>();
        _coll = GetComponent<Collider2D>();
        _rb.gravityScale = normalGravity;
        _coyoteTime = null;
        _hasJumped = true;
        _hasDashed = true;
        _jumpBuffer = null;
        _jumpHold = null;
        _dashActive = null;
        _inputStates = new Dictionary<KeyControl, bool>();
        dashParticles.Stop();

        _faceDirection = new Vector2(1, 0);
        _recentFaceRight = true;

        _inputStates[_k.aKey] = false;
        _inputStates[_k.dKey] = false;
        _inputStates[_k.wKey] = false;
        _inputStates[_k.sKey] = false;
        _inputStates[_k.spaceKey] = false;
        
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Default"), LayerMask.NameToLayer("Dash"), true);
    }

    // Update is called once per frame
    void Update()
    {
        //restructure so that updates only collect inputs
        _inputStates[_k.aKey] = _k.aKey.isPressed;
        _inputStates[_k.dKey] = _k.dKey.isPressed;
        _inputStates[_k.wKey] = _k.wKey.isPressed;
        _inputStates[_k.sKey] = _k.sKey.isPressed;
        _inputStates[_k.spaceKey] = _k.spaceKey.isPressed;

        if (_k.rightShiftKey.wasPressedThisFrame && _dashActive == null)
        {
            PhaseDash();
        }
    }

    void FixedUpdate()
    {
        if (_dashActive != null || Time.timeScale == 0)
        {
            return;
        }
        
        int lateralMovement = 0;
        if (_inputStates[_k.aKey])
        {
            lateralMovement = -1;
            _recentFaceRight = false;
        }

        if (_inputStates[_k.dKey])
        {
            lateralMovement += 1;
            _recentFaceRight = true;
        }

        int verticalFacing = 0;
        if (_inputStates[_k.wKey]) verticalFacing = 1;
        if (_inputStates[_k.sKey]) verticalFacing -= 1;
        
        Vector2 newFaceDirection = new Vector2(lateralMovement,verticalFacing);
        if (newFaceDirection.magnitude == 0)
        {
            _faceDirection = new Vector2((_recentFaceRight) ? 1 : -1, 0);
        }
        else
        {
            _faceDirection = newFaceDirection.normalized;
        }
        
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

        if (_rb.linearVelocity.y > maxUpSpeed)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, maxUpSpeed);
        }

        if (Mathf.Abs(_rb.linearVelocityX) > maxLateralSpeed)
        {
            _rb.linearVelocity = new Vector2(Math.Sign(_rb.linearVelocityX) * maxLateralSpeed, _rb.linearVelocity.y);
            
        }

        _rb.gravityScale = (_jumpHold != null) ? holdGravity : normalGravity;
        
    }

    void Jump()
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x,jumpSpeed);
        _hasJumped = true;
        _jumpHold = StartCoroutine(HoldJump());
    }

    void PhaseDash()
    {
        _dashActive = StartCoroutine(Dash());
    }

    public bool IsDashing()
    {
        return (_dashActive != null);
    }

    IEnumerator Dash()
    {
        gameObject.layer = LayerMask.NameToLayer("Dash");
        _rb.linearVelocity = dashSpeed * _faceDirection;
        _rb.gravityScale = 0;
        dashParticles.Play();
        state = PlayerState.phasing;
        yield return new WaitForSeconds(dashDuration);
        _rb.linearVelocity = Vector2.zero;
        yield return  new WaitForSeconds(dashPause);
        gameObject.layer = LayerMask.NameToLayer("Default");
        _rb.gravityScale = normalGravity;
        dashParticles.Stop();
        _dashActive = null;
    }
    
    void OnCollisionEnter2D(Collision2D collision){
        
        if (collision.gameObject.CompareTag("Ground") &&
            collision.contacts[0].normal.x == 0)
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
        if (collision.gameObject.CompareTag("Ground") &&
             collision.contacts[0].normal.x == 0)
        {
            state = PlayerState.grounded;
            _hasJumped = false;
            CeaseIfActive(ref _coyoteTime);
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
        
        state = PlayerState.death;
        _rb.linearVelocity = Vector2.zero;
        state =  PlayerState.airborne;
        //invariant: player must spawn in in the air
        Vector3 respawnPos = currentArea.RespawnPos();
        transform.position = respawnPos;
        state = PlayerState.airborne;
        
    }

    public IEnumerator TransitionPause()
    {
        Time.timeScale = 0;
        CeaseIfActive(ref _dashActive);
        dashParticles.Stop();
        gameObject.layer = LayerMask.NameToLayer("Default");
        _hasDashed = false;
        yield return new WaitForSecondsRealtime(transitionTime);
        Time.timeScale = 1;
    }


    public enum PlayerState
    {
        grounded,
        airborne,
        phasing,
        death,
        transition
    }
}
