﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PBaseMovement_States {
    Null = -1,
    Idle,          // 0
    Running,       // 1
    Airborne,      // 2
    ClimbingSlope, // 3
    OnWall,        // 4
    SteepSlope,    // 5
    TopSlope,      // 6
    Attack_01      // 7
};

/* Contains all of the common state variables for Player Base Movement. */
/* Methods and variables here apply to every state for Base Movement.   */
public class PBaseMovement_State : PState {
    /* Execution Order:
     * FixedUpdate
     * OnCollisionEnter2D
     */


    [HideInInspector]
    public PBaseMovement behaviour;// The parent behaviour of the state - contains all of the shared variables for states in a behaviour.
    public PInputManager input;
    public PCollisionState collisionState;
    public PBaseMovement_CollisionManager collisionManager;

    /* Variables that each PBaseMovement State has a copy of: */

    public PBaseMovement_State(PBaseMovement behaviourIn)
    {
        behaviour = behaviourIn;
        collisionState = behaviour.collisionState;
        input = behaviour.pInputManager;
        collisionManager = new PBaseMovement_CollisionManager(this, collisionState);
    }

    /* Called via Start method of PBaseMovement. */
    public virtual void OnStart()
    {
        //TODO: Merge this into constructor for each sub-state.
    }

    /* When any Base Movement state is entered. */
    public override void OnStateEnter()
    {
        base.OnStateEnter();
        //collisionManager.owner = this;
    }

    /* When any Base Movement state is Updated. */
    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        OnInputBehaviour();
        DoCollisionBehaviour();
    }

    /* For changing order of OnFixedUpdate - ie. skip local OnFixedUpdate. */
    public void OnFixedUpdate_GrandparentCall()
    {
        base.OnFixedUpdate();
    }

    /* Called every FixedUpdate for actions based on current collision overlaps. 
    * Precondition: Persistent and Enter collision overlaps have been checked for this fixed frame. */
    public virtual void DoCollisionBehaviour()
    {
        collisionManager.DoCollisionBehaviour();
        //Debug.Log(this.GetType() + " - DoCOllisionBehaviour"); //@tag: DEBUG
    }


        /* Responds to any Input events - called after collision handling. */
    public virtual void OnInputBehaviour()
    {

    }

    /* When any Base Movement state is Exited. Should not change velocity. */
    public override void OnStateExit()
    {
        base.OnStateExit();
    }
}
