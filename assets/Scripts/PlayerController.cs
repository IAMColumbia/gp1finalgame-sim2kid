using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public SlimeState SlimeState { get; protected set; }
    public Vector2 JumpStrength { get; private set; }

    [Header("Jump Settings")]
    [SerializeField] private Vector2 minJumpStrength = new Vector2(1, 10);
    [SerializeField] private Vector2 maxJumpStrength = new Vector2(5, 20f);
    [SerializeField] private float secondsToCharge = 10;
    [Header("Physics Settings")]
    [SerializeField] private float gravityScale = 1;
    [SerializeField] private float friction = 0.1f;
    [Header("Required Connections")]
    [SerializeField] private ArcManager arcManager;

    private Rigidbody2D rb;
    private bool frictionEnabled;

    // Start is called before the first frame update
    void Start()
    {
        frictionEnabled = false;
        JumpStrength = minJumpStrength;
        SlimeState = SlimeState.Falling;
        try
        {
            rb = gameObject.GetComponent<Rigidbody2D>();
        }
        catch (System.NullReferenceException e)
        {
            Debug.LogError("The PlayerController requires a Rigidbody2D! Generating one instead...\nException: " + e);
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        finally 
        {
            rb.gravityScale = this.gravityScale;
            rb.mass = 1;
            rb.freezeRotation = true;
        }
        if (arcManager == null) 
        {
            try
            {
                arcManager = GameObject.FindObjectOfType<ArcManager>();
            }
            catch (System.NullReferenceException e)
            {
                Debug.LogError("You need to have an ArcManager in the Scene to use a PlayerController!\nException: " + e);
                throw e;
            }
        }
        arcManager.enabled = false;

    }

    // Update is called once per frame
    void Update()
    {
        updateSlimeState();
        updateArcManager();
        updateJumpState(Time.deltaTime);
    }

    void updateJumpState(float time) 
    {
        if (Input.GetKeyDown(KeyCode.Space) && SlimeState == SlimeState.Idle)
        {
            JumpStrength = minJumpStrength;
            SlimeState = SlimeState.Jumping;
        }
        if (SlimeState == SlimeState.Jumping) 
        {
            if (JumpStrength.sqrMagnitude < maxJumpStrength.sqrMagnitude)
                JumpStrength += ((maxJumpStrength - minJumpStrength) / secondsToCharge) * time;
        }

        if (Input.GetKeyUp(KeyCode.Space) && SlimeState == SlimeState.Jumping)
        {
            SlimeState = SlimeState.Falling;
            rb.AddForce(JumpStrength, ForceMode2D.Impulse);
            JumpStrength = minJumpStrength;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        frictionEnabled = true;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(SlimeState == SlimeState.Falling)
            if (rb.velocity.sqrMagnitude > 0.0001)
            {
                if(frictionEnabled)
                    rb.velocity *= 1 - friction;
            }
            else
            {
                rb.velocity = Vector2.zero;
                frictionEnabled = false;
            }
        
    }

    void updateArcManager() 
    {
        switch (SlimeState) 
        {
            case SlimeState.Idle:
                arcManager.enabled = true;
                break;
            case SlimeState.Jumping:
                arcManager.enabled = true;
                break;
            default:
                arcManager.enabled = false;
                break;
        }
    }

    void updateSlimeState()
    {
        // If Bouncing and Moving, State returns to Falling
        if (rb.velocity.sqrMagnitude > 0 && (SlimeState == SlimeState.BouncingDown ||
            SlimeState == SlimeState.BouncingRight || SlimeState == SlimeState.BouncingLeft ||
            SlimeState == SlimeState.BouncingUp))
        {
            SlimeState = SlimeState.Falling;
        }
        // if no movement while alive, goto idle
        if (rb.velocity.sqrMagnitude == 0 && SlimeState == SlimeState.Falling)
        {
            SlimeState = SlimeState.Idle;
        }
    }
}

public enum SlimeState 
{
    Idle,
    Jumping,
    Falling,
    BouncingDown,
    BouncingRight,
    BouncingLeft,
    BouncingUp,
    Dying,
    Dead

}
