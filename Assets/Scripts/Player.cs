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
    private Transform _t;
    private Rigidbody2D _rb;
    private Collider2D _coll;
    private Keyboard _k;
    private Dictionary<KeyControl, bool> _inputStates;
    public SpriteRenderer sr;

    private AudioSource _as;

    public static readonly Vector2 Dimensions = new Vector2(1.75f, 2);
    
    //this is to keep track of currently active coyoteTimes;
    private Coroutine _coyoteTime;
    private Coroutine _jumpBuffer;
    private Coroutine _jumpHold;
    private Coroutine _jumpActive;
    private Coroutine _dashActive;
    private Coroutine _deathActive;

    public List<GameObject> _groundContact;
    
    private bool _hasDashed;
    public Vector2 _faceDirection;
    private bool _recentFaceRight;

    [SerializeField] 
    private int consecutiveAirborneZero;

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

    [Header("Death")] 
    public float deathDuration;
    public ParticleSystem deathParticles;
    public float deathPause;
    public ParticleSystem respawnParticles;
    public float deathReformDuration;

    [Header("Transition")] 
    public float transitionTime;
    

    [Header("Area")] 
    public Area currentArea;
    public InputSwitch nearInteractable;

    [Header("Impact")] 
    public float impactTime;
    public float impactSquish;
    
    public Animator anim;
    
    
    
    void Awake()
    {
        _t = transform;
        p = this;
        _k = Keyboard.current;
        _rb = GetComponent<Rigidbody2D>();
        _coll = GetComponent<Collider2D>();
        _rb.gravityScale = normalGravity;
        _coyoteTime = null;
        _hasDashed = true;
        _jumpBuffer = null;
        _jumpHold = null;
        _dashActive = null;
        _jumpActive = null;
        _deathActive = null;
        _inputStates = new Dictionary<KeyControl, bool>();
        dashParticles.Stop();
        deathParticles.Stop();
        respawnParticles.Stop();
        _as = GetComponent<AudioSource>();

        consecutiveAirborneZero = 0;

        _faceDirection = new Vector2(1, 0);
        _recentFaceRight = true;

        _inputStates[_k.aKey] = false;
        _inputStates[_k.dKey] = false;
        _inputStates[_k.wKey] = false;
        _inputStates[_k.sKey] = false;
        _inputStates[_k.spaceKey] = false;
        _groundContact = new List<GameObject>();
        nearInteractable = null;

        transform.position = currentArea.respawnMarker.transform.position;
        
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Default"), LayerMask.NameToLayer("Dash"), true);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsDashing() || IsDying()) return;
        //restructure so that updates only collect inputs
        _inputStates[_k.aKey] = _k.aKey.isPressed;
        _inputStates[_k.dKey] = _k.dKey.isPressed;
        _inputStates[_k.wKey] = _k.wKey.isPressed;
        _inputStates[_k.sKey] = _k.sKey.isPressed;
        _inputStates[_k.spaceKey] = _k.spaceKey.isPressed;

        if (_k.rightShiftKey.wasPressedThisFrame && _dashActive == null && !_hasDashed)
        {
            PhaseDash();
        }

        if (Math.Sign(transform.localScale.x) != Math.Sign(_faceDirection.x) && _faceDirection.x != 0)
        {
            print("Hi");
            Vector3 newScale = transform.localScale;
            newScale.x *= -1;
            transform.localScale = newScale;
        }

        if (_k.eKey.wasPressedThisFrame && _dashActive == null && state == PlayerState.grounded && nearInteractable != null)
        {
            //TODO: add trigger for interact animation;
            nearInteractable.Interact();
        }

        if (_rb.linearVelocityY == 0 && state == PlayerState.airborne && Time.timeScale > 0)
        {
            consecutiveAirborneZero++;
        }
        else
        {
            consecutiveAirborneZero = 0;
        }

        if (consecutiveAirborneZero > 10)
        {
            consecutiveAirborneZero = 0;
            print("anomalous airbone state detected; setting to grounded");
            state = PlayerState.grounded;
            _hasDashed = false;
            
        }
       
    }

    void FixedUpdate()
    {
        UpdateAnimation();

        if (_dashActive != null || IsDying() || Time.timeScale == 0)
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

        if (_groundContact.Count > 0)
        {
            state = PlayerState.grounded;
        }

        if (_inputStates[_k.spaceKey])
        {
            if( _groundContact.Count > 0 &&
                _jumpActive == null && 
               (
                   state == PlayerState.grounded ||
                   _coyoteTime != null
               )
              )
            {
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

    void UpdateAnimation()
    {
        //sr.flipX = !_recentFaceRight;
        anim.SetFloat("speed", Mathf.Abs(_rb.linearVelocityX));
        anim.SetFloat("verticalSpeed", _rb.linearVelocityY);
        anim.SetBool("grounded", state == PlayerState.grounded);
        anim.SetBool("dashing", state == PlayerState.phasing);
    }

    void Jump()
    {
        _jumpActive = StartCoroutine(JumpActive());
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x,jumpSpeed);
        _jumpHold = StartCoroutine(HoldJump());
        anim.SetTrigger("jump");
    }

    IEnumerator JumpActive()
    {
        yield return new WaitForSeconds(0.1f);
        _jumpActive = null;
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
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z);
        _groundContact.Clear();
        _hasDashed = true;
        gameObject.layer = LayerMask.NameToLayer("Dash");
        _rb.linearVelocity = dashSpeed * _faceDirection;
        _rb.gravityScale = 0;
        dashParticles.Play();
        sr.color = Color.black;
        
        state = PlayerState.phasing;
        yield return new WaitForSeconds(dashDuration);
        _rb.linearVelocity = Vector2.zero;
        yield return  new WaitForSeconds(dashPause);
        gameObject.layer = LayerMask.NameToLayer("Default");
        _rb.gravityScale = normalGravity;
        dashParticles.Stop();
        sr.color = Color.white;
        state = PlayerState.airborne;
        
        _dashActive = null;
    }
    
    void OnCollisionEnter2D(Collision2D collision){
        
        if (collision.gameObject.CompareTag("Ground") &&
            collision.contacts[0].normal.x == 0 && transform.position.y >= collision.gameObject.transform.position.y)
        {
            state = PlayerState.grounded;
            if (collision.contacts[0].relativeVelocity.y > 0)
            {
                StartCoroutine(Squish(collision.contacts[0].relativeVelocity.y));
            }

            if (!_groundContact.Contains(collision.gameObject))
            {
                _groundContact.Add(collision.gameObject);
            }
            _hasDashed = false;
            CeaseIfActive(ref _coyoteTime);
            
            if (_jumpBuffer != null)
            {
                print("jump buffer");
                Jump();
                CeaseIfActive(ref _jumpBuffer);
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (_groundContact.Contains(collision.gameObject))
            {
                _groundContact.Remove(collision.gameObject);
                
            }

            if (_groundContact.Count == 0)
            {
                state = PlayerState.airborne;
                CeaseIfActive(ref _coyoteTime);
                _coyoteTime = StartCoroutine(CoyoteTime());
            }
            
        }
    }

    /*private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") &&
             collision.contacts[0].normal.x == 0 && transform.position.y >= collision.gameObject.transform.position.y)
        {
            state = PlayerState.grounded;
            SetJump(false);
            _hasDashed = false;
            CeaseIfActive(ref _coyoteTime);
        }
            
    }*/

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
    //can't have explicit bias toward _hasJumped = false or = true
    //for example, On---Enter triggers before On---Exit, meaning exitting 1 collider will make priority over entering touch of one
    //what if...
    //at all times Player keeps track of a GameObject that has "groundContact"
    //No need for OnCollisionStay, just OnCollisionEnter
    //each valid ground Enter(correct y value, contact normal non-horizontal, etc) updates groundContact
    //each groundExit sets groudnContact to null IF they are the same GameObject
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

    public void Death(bool playAnimation = false)
    {
        
        _deathActive = StartCoroutine(Die(playAnimation));
        //invariant: player must spawn in in the air
        
    }

    public bool IsDying()
    {
        return (_deathActive != null);
    }

    IEnumerator Die(bool playAnimation)
    {
        print("dead");
        _as.Play();
        state = PlayerState.death;
        _rb.linearVelocity = Vector2.zero;
        _rb.gravityScale = 0;
        _coll.enabled = false;
        _groundContact.Clear();
        if (playAnimation)
        {
            anim.SetTrigger("death");
            yield return new WaitForSeconds(deathDuration);
        }
        
        
        
        
        Vector3 respawnPos = currentArea.RespawnPos();
        deathParticles.Play();
        sr.gameObject.SetActive(false);
        
        yield return new WaitForSecondsRealtime(deathPause);
        
        transform.position = respawnPos;
        _coll.enabled = true;
        state = PlayerState.airborne;
        respawnParticles.Play();
        sr.gameObject.SetActive(true);

        yield return new WaitForSeconds(deathReformDuration);
        
        
        _rb.gravityScale = normalGravity;
        _deathActive = null;
    }

    public IEnumerator TransitionPause()
    {
        Time.timeScale = 0;
        CeaseIfActive(ref _dashActive);
        dashParticles.Stop();
        sr.color = Color.white;
        gameObject.layer = LayerMask.NameToLayer("Default");
        yield return new WaitForSecondsRealtime(transitionTime);
        Time.timeScale = 1;
    }

    IEnumerator Squish(float verticalV)
    {
        float impactCoeff = impactSquish * verticalV / maxFallingSpeed;
        for (float t = 0; t < impactTime; t += Time.deltaTime)
        {
            float r = 1 - t / impactTime;
            Vector3 newScale = transform.localScale;
            newScale.y = 2 - impactCoeff * r;
            newScale.x = Math.Sign(newScale.x) * (2 + impactCoeff * r);

            transform.localScale = newScale;
            
            yield return null;
        }

        transform.localScale = new Vector3(Math.Sign(transform.localScale.x) * 2, 2, 1);
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
