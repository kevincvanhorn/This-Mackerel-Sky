﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBehaviour : MonoBehaviour {

    public PInputManager pInputManager;
    public List<PInput> inputFilter; // Inputs that this state accepts - any others should be discarded.
    public PCollisionState collisionState;

    public PState curState;

    // Use this for initialization
    public virtual void Start()
    {
        pInputManager = GetComponent<PInputManager>();
        collisionState = GetComponent<PCollisionState>();
    }

    public virtual void OnFixedUpdate()
    {
        //TODO: pInputManager.FilterInput(inputFilter); // Filter input so that the input array in PInputManager 
        curState.OnFixedUpdate();   
    }

    public virtual void Transition(PState nextState)
    {
        curState.OnStateExit();
        nextState.OnStateEnter();
        curState = nextState;
    }
}
