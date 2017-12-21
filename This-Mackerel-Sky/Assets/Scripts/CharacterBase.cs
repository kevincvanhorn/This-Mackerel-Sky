﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MonsterLove.StateMachine; // State-Machine Package.

// FixedUpdate -> OnTrigger -> OnCollision

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterBase : MonoBehaviour {

    Rigidbody2D rigidBody; // Not Kinematic: moves not by transform, but by physics

    /* Collisions Vars */
    public bool isGrounded;

    // note: 3 states- left, right, and still: requires two variables
    public bool isSprinting;
    public bool isTouchingTop;
    public bool isTouchingRight;
    public bool isTouchingLeft;
    public bool isTouchingBot;
    public bool onWall;
    public bool onSlope;

    /* Colliders */
    HashSet<Vector2> contacts = new HashSet<Vector2>();

    /* Movement Variables */
    public float moveSpeed = 10;    // Horizontal speed.
    public float moveSpeedMin = 5;
    public float sprintSpeed = 20;
    public float activeSpeed;
    public Vector3 velocity;

    float wallImpactSpeed;
    float directionFacing = 1;

    /* Jump Variables */
    public float lateralAccelAirborne = 60;
    public float lateralAccelGrounded = 100;

    public float jumpHeightMax = 5;
    public float jumpHeightMin = .9f;
    public float timeToJumpApex = .4f;

    float gravity;
    float jumpVelocityMax;
    float jumpVelocityMin;

    /* Slope Variables */
    public float slopeDir;
    public float slopeAngle = 0;
    public float maxAngle = 80;

    /* Define States */
    public enum States {
        Idle,
        Jumping,
        Falling,
        WallRising,
        WallFalling,
        WallSticking,
        Sprinting,
        Dashing,
        ClimbingSlope
    }

    private enum CollisionType {
        None,
        Top,
        Bot,
        Left,
        Right,
        Slope
    };

    HashSet<CollisionType> enterCollisionTypes = new HashSet<CollisionType>();
    HashSet<CollisionType> exitCollisionTypes = new HashSet<CollisionType>();

    private StateMachine<States> fsm;

    public void Awake() {
        // Initialize State Machine Engine		
        fsm = StateMachine<States>.Initialize(this, States.Falling);
    }

    public void Start() {
        /* Set collision defaults. */
        isTouchingTop = false;
        isTouchingRight = false;
        isTouchingLeft = false;
        isGrounded = false;
        isSprinting = false;
        isTouchingBot = false;
        onSlope = false;

        activeSpeed = moveSpeed;
        wallImpactSpeed = activeSpeed;
        rigidBody = GetComponent<Rigidbody2D>();

        /* Calc constants in terms of Jump time and apex height. */
        gravity = -(2 * jumpHeightMax) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocityMax = Mathf.Abs(gravity * timeToJumpApex);
        jumpVelocityMin = Mathf.Sqrt(2 * Mathf.Abs(gravity) * jumpHeightMin);
    }

    void FixedUpdate() {
        Debug.Log("MAIN - Update");
        rigidBody.velocity = velocity;
    }

    /* Synchronous w/ Custom State fixed updates - for common resetting of attributes. */
    void BaseFixedUpdate() {
        enterCollisionTypes.Clear();
        exitCollisionTypes.Clear();
    }

    /** Called on Player collision with a new object. **/
    void BaseCollisionEnter2D(Collision2D collision) { // ~ Could convert Collision2D to Collider2D
        ContactPoint2D[] contactsIn = new ContactPoint2D[4]; // 2 when side collides (each corner) || 1 when on slope
        collision.GetContacts(contactsIn);

        bool enterSet = false;

        /* Add new contact points to hash. */
        for (int i = 0; i < contactsIn.Length; i++) {
            if (contactsIn[i].normal != Vector2.zero) {
                if (!contacts.Contains(contactsIn[i].normal)) {
                    contacts.Add(contactsIn[i].normal);
                    //print(contactsIn[i].normal);
                }
            }
        }

        /* Call Collider Enter Functions */
        for (int i = 0; !enterSet && i < contactsIn.Length; i++) {
            /* If contact exists (entries are zero in larger alocated ContactPoint2D[])*/
            //print("--------------" + slopeAngle + " " + contactsIn[i].normal);
            if (contactsIn[i].normal != Vector2.zero) {
                /* Vertical Collision */
                slopeAngle = Vector2.Angle(contactsIn[i].normal, Vector2.up);
                if (contactsIn[i].normal.x == 0) { // contactsIn[i].normal.x == 0
                    if (contactsIn[i].normal.y == 1) {
                        enterCollisionTypes.Add(CollisionType.Bot);
                        enterSet = true;
                    }
                    else if (contactsIn[i].normal.y == -1) { // contactsIn[i].normal.y == -1
                        enterCollisionTypes.Add(CollisionType.Top);
                        enterSet = true;
                    }
                }
                /* Horizontal Collision */
                else if (slopeAngle > maxAngle) { // contactsIn[i].normal.y == 0
                    if (contactsIn[i].normal.x > 0) { // contactsIn[i].normal.x == 1
                        enterCollisionTypes.Add(CollisionType.Left);
                        enterSet = true;
                    }
                    else if (contactsIn[i].normal.x < 0) { // contactsIn[i].normal.x == -1
                        enterCollisionTypes.Add(CollisionType.Right);
                        enterSet = true;
                    }
                }
                /* Slope Collision */
                else {
                    slopeDir = (contactsIn[i].normal.x < 0) ? 1 : -1; // 1 = right, -1 = left
                    //slopeAngle = Vector2.Angle(contactsIn[i].normal, Vector2.up);
                    enterCollisionTypes.Add(CollisionType.Slope);
                }
            }
        }
    }

    /** Called on Player collision Exit. **/
    void BaseCollisionExit2D(Collision2D collision) { // ~ Could convert Collision2D to Collider2D
        ContactPoint2D[] contactsRB = new ContactPoint2D[4]; // 2 when side collides (each corner) || 1 when on slope
        rigidBody.GetContacts(contactsRB);

        bool exitSet = false;
        float slopeAngleExit = 0;

        /* Make a hash with the current normals touching the object. */
        HashSet<Vector2> contactNormalsRB = new HashSet<Vector2>();
        foreach (ContactPoint2D c in contactsRB) {
            contactNormalsRB.Add(c.normal);
        }

        HashSet<Vector2> exitContacts = new HashSet<Vector2>();
        exitContacts.UnionWith(contacts);           // exitContacts = contacts

        exitContacts.ExceptWith(contactNormalsRB);  // Set ExitContacts.
        contacts.ExceptWith(exitContacts);          // Remove Exit contacts from Hash.

        /* Call Collider Enter Functions */
        foreach (Vector2 exitContact in exitContacts) {
            //if (!exitSet) {
            /* If contact exists (entries are zero in larger allocated ContactPoint2D[])*/
            if (exitContact != Vector2.zero) {
                slopeAngleExit = Vector2.Angle(exitContact, Vector2.up);
                //print("EXIT --- " + slopeAngleExit);
                /* Vertical Collision */
                if (exitContact.x == 0) {
                    if (exitContact.y == 1) {
                        exitCollisionTypes.Add(CollisionType.Bot);
                        exitSet = true;
                    }
                    else if (exitContact.y == -1) {
                        exitCollisionTypes.Add(CollisionType.Top);
                        exitSet = true;
                    }
                }
                /* Horizontal Collision */
                else if (slopeAngleExit > maxAngle) { //exitContact.y == 0
                    if (exitContact.x > 0) {
                        exitCollisionTypes.Add(CollisionType.Left);
                        exitSet = true;
                    }
                    else if (exitContact.x < 0) {
                        exitCollisionTypes.Add(CollisionType.Right);
                        exitSet = true;
                    }
                }
                /* Slope Collision */
                else {
                    exitCollisionTypes.Add(CollisionType.Slope);
                }
            }
            //}
        }
    }

    /* Collision Methods: Custom ---------------------------------------------*/
    // Enter
    // Exit
    // FixedUpdate
    // Collision Enter/Exit
    // Update
    // LateUpdate
    // Finally

    void Falling_FixedUpdate() {
        BaseFixedUpdate();

        velocity.y += gravity * Time.deltaTime; // Apply Gravity until grounded
    }

    void Falling_OnCollisionEnter2D(Collision2D collision) {
        BaseCollisionEnter2D(collision);
        



        Debug.Log("FALLING - OnCollisionEnter");
        velocity.y = 0;
        fsm.ChangeState(States.Idle, StateTransition.Overwrite);
    }

    void Idle_Enter() {
        velocity.y = 0;
    }

}

// Fianlly: Reset object to desired configuration
// For Overwrite: fsm.ChangeState(States.MyNextState, StateTransition.Overwrite);