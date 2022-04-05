using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This is a quick & dirty State Machine implementation, it just does the job needed for this demo.
/// Nothing fancy here except some events and basic error handling examples
/// </summary>
namespace Robot
{
    /// <summary>
    /// Simple enum with commands to transition from one state to another
    /// </summary>
    public enum RobotCommand
    {
        None,
        MoveToConveyor,
        MoveToStandBy,
        MoveToLift,
        MoveToPalett,
        OpenTool,
        CloseTool,
        DecideDestination
    }

    /// <summary>
    /// Helper class to pass the values needed to set the movement destination
    /// </summary>
    public class MovementEventArgs : EventArgs
    {
        public Vector3 Destination;
        public float RotationSpeed;
    }

    /// <summary>
    /// The States of the StateMachine. Holds the transitions to the next states and the Callback called on entering the state
    /// </summary>
    public class State
    {
        public delegate void DelegateOnStateEnter(EventArgs args);
        public DelegateOnStateEnter OnStateEnter;
        public readonly string StateName;
        private Dictionary<RobotCommand, State> Transitions = new Dictionary<RobotCommand, State>();
        public readonly EventArgs Args;

        //c'tor
        public State(string stateName, EventArgs args)
        {
            StateName = stateName;
            Args = args;
        }

        /// <summary>
        /// Add a new transition
        /// </summary>
        /// <param name="command"></param>
        /// <param name="nextState"></param>
        public void AddTransition(RobotCommand command, State nextState)
        {
            try { Transitions.Add(command, nextState); }
            catch (ArgumentException)
            {
                //Logs an error to the UnityEditor console
                //Debug.LogError("Error in StateMachine::AddTransition: Key '" + command.ToString() + "' already exist");
                throw;
            }
        }

        /// <summary>
        /// Returns the next state or null, if no transition for the command exists
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public State GetNextState(RobotCommand command)
        {
            if (Transitions.ContainsKey(command)) return Transitions[command];
            return null;
        }

    }

    /// <summary>
    /// The state machine
    /// </summary>
    public class StateMachine
    {
        public delegate void DelegateDisplayCurrentState(string stateName);
        public event DelegateDisplayCurrentState DisplayCurrentState;

        private List<State> _states = new List<State>();
        private State _currentState = null;

        /// <summary>
        /// Add a new State. Returns false, if a state with the same name already exists
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool AddState(State state)
        {
            if (_states.Exists(s => s.StateName == state.StateName))
            {
                //Logs an error to the UnityEditor console
                //Debug.LogError("State with name '" + state.StateName + "' already exist");
                return false;
            }

            _states.Add(state);
            return true;
        }

        /// <summary>
        /// Add a new transition. Returns true, if the transition was successfully added, otherwise false
        /// </summary>
        /// <param name="srcStateName"></param>
        /// <param name="dstStateName"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public bool AddTransition(string srcStateName, string dstStateName, RobotCommand command)
        {
            State srcState = GetState(srcStateName);
            State dstState = GetState(dstStateName);

            if (srcState != null && dstState != null) 
            {
                srcState.AddTransition(command, dstState);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Set the initial state
        /// </summary>
        /// <param name="stateName"></param>
        public void SetInitialState(string stateName)
        {
            _currentState = _states.Where(s => s.StateName == stateName).First();
        }

        /// <summary>
        /// Transite to the next state
        /// </summary>
        /// <param name="command"></param>
        public void NextState(RobotCommand command)
        {
            State next = _currentState.GetNextState(command);

            if (next != null)
            {
                DisplayCurrentState(next.StateName);
                _currentState = next;
                _currentState.OnStateEnter(_currentState.Args);
            }
        }

        /// <summary>
        /// Returns the name of the current state
        /// </summary>
        /// <returns></returns>
        public string GetCurrentStateName()
        {
            return _currentState.StateName;
        }

        /// <summary>
        /// Returns the State object for the state name. If no state with the name exists, null is returned 
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        private State GetState(string stateName)
        {
            State ret = null;
            try { ret = _states.Where(s => s.StateName == stateName).First(); }
            catch (InvalidOperationException)
            {
                //Logs an error to the UnityEditor console
                //Debug.LogError("Error in StateMachine: state with name '" + stateName + "' not found");
                return null;
            }

            return ret;
        }
    }

}
