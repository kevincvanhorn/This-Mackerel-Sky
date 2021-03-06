﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PBaseMovement : PBehaviour {

    /* Inherited Variables: */
    // Inherited: PInputManager pInputManager
    // Inherited: PState curState
    // Inherited: PCollisionState collisionState

    /* Declare Components: */
    public Rigidbody2D rigidBody;
    public new Collider2D collider; // @new for gameobject.collider keyword hiding.

    /* Base Movement Variables: */
    public float gravity;
    public float moveSpeed = 10;
    public float activeSpeed;         // Horizontal Speed.
    public float lateralAccelAirborne = 60;
    public float lateralAccelGrounded = 100;
    //public float moveSpeedMin = 5;
    public float sprintSpeed = 20;

    /* Airborne Variables: */
    [HideInInspector]
    public float jumpVelocityMax;
    [HideInInspector]
    public float jumpVelocityMin;

    /* Wall Variables: */
    public Vector2 wallHitSpeed = Vector2.zero;
    //public Vector3 debugWallHitLoc;
    //public Vector2 wallHitNormal;
    public float wallFallSpeed;
    public float wallStickTime = 0;
    public int wallCase;
    public bool isWallSticking;

    /* Slope Variables */
    public float maxAngle = 80;
    public Vector2 climbSlopeHitSpeed;
    public float steepSlopeMinEnterSpeed = 20;

    /* Top Slope Vars: */
    public Vector2 topSlopeSpeedCur;
    public float topSlideFactor = 1;

    /* State-Specific Vars */
    // Steep Slopes
    public float steepSlopeSpeed;
    //public Vector2 steepSlopeHitNormal;
    //public float wallFrictionDown;

    /* Declare States: */
    public PBaseMovement_Airborne SAirborne;
    public PBaseMovement_Idle SIdle;
    public PBaseMovement_Running SRunning;
    public PBaseMovement_State SOnWall, SSteepSlope, STopSlope, SClimbingSlope, SDashing, SAction;

    /* Collision Variables: */
    public HashSet<CollisionType> enterCollisionTypes = new HashSet<CollisionType>(); // Used to simulate onCollisionEnter each FixedUpdate.

    public override void Awake()
    {
        base.Awake();
        /* Get Components. */
        pInputManager = GetComponent<PInputManager>();
        rigidBody = GetComponent<Rigidbody2D>(); // Note: Could be in this.Start Method
        collider = GetComponent<Collider2D>();   // For CameraFollow's Start Method.
    }

    public override void OnStart()
    {
        /* Input Setup. */
        inputFilter = new List<PInput>() { PInput.Vertical, PInput.Horizontal, PInput.Up, PInput.Down, PInput.Left, PInput.Right, PInput.Sprint, PInput.Dash };
        base.OnStart(); // Creates Input Manager.

        /* Calc Movement Variables. */
        gravity = -(2 * PStats.jumpHeightMax) / Mathf.Pow(PStats.timeToJumpApex, 2);
        jumpVelocityMax = Mathf.Abs(gravity * PStats.timeToJumpApex);
        jumpVelocityMin = Mathf.Sqrt(2 * Mathf.Abs(gravity) * PStats.jumpHeightMin);
        activeSpeed = moveSpeed;

        wallHitSpeed.x = activeSpeed;
        //wallFrictionDown = 1;

        /* Create States. */
        SAirborne = new PBaseMovement_Airborne(this);
        SIdle = new PBaseMovement_Idle(this);
        SRunning = new PBaseMovement_Running(this);
        SOnWall = new PBaseMovement_OnWall(this);
        SClimbingSlope = new PBaseMovement_ClimbingSlope(this);
        STopSlope = new PBaseMovement_TopSlope(this);
        SSteepSlope = new PBaseMovement_SteepSlope(this);

        /* Set State. */
        curState = SAirborne;
    }

    public override void OnFixedUpdate()
    {
        /* Receive input from any pInutManager Updates. */

        /* Pre-State Update. */ 
        UpdateLateralInputVars(); // Sets directionMoving, directionFacing, hasLateralInput.

        /* Collision Update. */
        collisionState.OnFixedUpdate();

        /* State Update. */
        base.OnFixedUpdate();     // Via PBehaviour: Runs OnFixedUpdate for the current State.
                                  // If Transition: curState.Exit() -> nextState.Enter().

        rigidBody.velocity = velocity;//((PBaseMovement_State)curState).velocity; // Gets the velocity from the current PBaseMovement_State.
        
    }

    public override void Transition(PState nextState)
    {
        base.Transition(nextState); // Calls exit and enter methods for prev and next state respectively.
    }

    /* ---- Methods for Readability (Called once, solely to slim down overriden methods above.) */
}