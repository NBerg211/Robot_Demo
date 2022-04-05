using System;
using System.Collections.Generic;
using UnityEngine;

namespace Robot
{
    /// <summary>
    /// The RobotController controlls the abstract behaviour of the robot with a state machine
    /// </summary>
    public class RobotController
    {
        //------------ Properties -----------//

        //delegates and events to connect the controller with the simulation and UI
        public delegate void DelegateFixPhysicsProblem();
        public delegate void DelegateRobotInStandby();
        public delegate void DelegateDisplayCurrentDestination(Vector3 destination);

        public event DelegateFixPhysicsProblem SetWare;
        public event DelegateFixPhysicsProblem FreeWare;
        public event DelegateRobotInStandby RobotInStandby;
        public event DelegateDisplayCurrentDestination DisplayCurrentDestination;

        //Dictionary to map the ware type to the coresponding conveyor positions
        private Dictionary<WareTypes, Vector3> _conveyorPositions = new Dictionary<WareTypes, Vector3>();

        private IMovementController _movementController;
        private IToolController _toolController;
        private StateMachine _stateMachine;
        private RobotCommand _nextCommand = RobotCommand.None;
        private WareTypes _currentWareType = WareTypes.Ware_Pickup;


        //-------------------- Methods ---------------//
        //c'tor
        public RobotController(IMovementController movementController, IToolController tool, StateMachine stateMachine, Dictionary<WareTypes, Vector3> conveyorPositions)

        {
            //set the properties
            _movementController = movementController;
            _toolController = tool;
            _conveyorPositions = conveyorPositions;
            _stateMachine = stateMachine;

            //connect the ToolController
            _toolController.ToolOpened += OnToolOpened;
            _toolController.ToolClosed += OnToolClosed;

            //connect the MovementController
            _movementController.OnTargetReached += OnMoveDestinationReached;

            //Setup the statemachine, which controlls the behaviour
            BuildStateMachine();
        }

        /// <summary>
        /// Start the StateMachine
        /// </summary>
        public void StartRobot()
        {
            _stateMachine.NextState(RobotCommand.MoveToStandBy);
        }

        /// <summary>
        /// Returns the current ware type, which is handled
        /// </summary>
        /// <returns></returns>
        public WareTypes GetCurrentWareType()
        {
            return _currentWareType;
        }

        /// <summary>
        /// Set the current ware type to be handled
        /// </summary>
        /// <param name="wareType"></param>
        public void SetCurrentWareType(WareTypes wareType)
        {
            _currentWareType = wareType;
        }

        /// <summary>
        /// Callback to react on the arrival of a new ware
        /// </summary>
        /// <param name="wareType"></param>
        public void NewWareArived(WareTypes wareType)
        {
            _currentWareType = wareType;
            _stateMachine.NextState(RobotCommand.MoveToConveyor);
        }


        //--------- Callback methods for the State Machine states-----------------//

        /// <summary>
        /// Set the destination the tool should be moved to
        /// </summary>
        /// <param name="args"></param>
        private void OnMoveTo(EventArgs args)
        {
            MovementEventArgs moveArgs = args as MovementEventArgs;
            SetMovementTarget(moveArgs.Destination, moveArgs.RotationSpeed);
        }

        /// <summary>
        /// Set the movement destinations relativ to the position of the conveyor
        /// </summary>
        /// <param name="args"></param>
        private void OnMoveToWare(EventArgs args)
        {
            MovementEventArgs moveArgs = new MovementEventArgs()
            {
                Destination = GetConveyorPosition(_currentWareType) + (args as MovementEventArgs).Destination,
                RotationSpeed = (args as MovementEventArgs).RotationSpeed
            };
            OnMoveTo(moveArgs);
        }

        /// <summary>
        /// Callback to set the command to close the tool
        /// </summary>
        /// <param name="args">args is not used</param>
        private void OnCloseTool(EventArgs args)
        {
            _toolController.CloseTool();
        }

        /// <summary>
        /// Callback to set the command to open the tool
        /// </summary>
        /// <param name="args">args is not used</param>
        private void OnOpenTool(EventArgs args)
        {
            _toolController.OpenTool();
        }

        /// <summary>
        /// OnToolOpened is called by the ToolController, when the tool is fully opened
        /// </summary>
        private void OnToolOpened()
        {
            FreeWare();
            _stateMachine.NextState(RobotCommand.MoveToPalett);
        }

        /// <summary>
        /// OnToolClosed is called by the ToolController, when the tool is fully closed
        /// </summary>
        private void OnToolClosed()
        {
            SetWare();
            _stateMachine.NextState(RobotCommand.MoveToConveyor);
        }

        /// <summary>
        /// OnMoveDestinationReached is called by the MovementController, when the tool has reached the movement destination
        /// </summary>
        private void OnMoveDestinationReached()
        {
            _stateMachine.NextState(_nextCommand);
        }

        /// <summary>
        /// Set the next command the robot should execute after the movement is finished
        /// </summary>
        /// <param name="command"></param>
        private void SetNextCommand(RobotCommand command)
        {
            _nextCommand = command;
        }

        /// <summary>
        /// Calls the MovementController and set a new movement destination
        /// </summary>
        /// <param name="target"></param>
        /// <param name="rotationSpeed"></param>
        private void SetMovementTarget(Vector3 target, float rotationSpeed)
        {
            DisplayCurrentDestination(target);
            _movementController.SetToolTargetPosition(target, rotationSpeed);
        }


        /// <summary>
        /// Returns the position of the conveyor for the ware type, where the ware should be picked up or dropped
        /// </summary>
        /// <param name="wareType"></param>
        /// <returns></returns>
        private Vector3 GetConveyorPosition(WareTypes wareType)
        {
            return _conveyorPositions[wareType];
        }

        /// <summary>
        /// Returns the normalized direction of a conveyor relativ to the robot 
        /// </summary>
        /// <param name="wareType"></param>
        /// <returns></returns>
        private Vector3 GetConveyorDirection(WareTypes wareType)
        {
            //Get relative x & z direction between droppoint and robot
            Vector3 position = GetConveyorPosition(wareType);
            return new Vector3(position.x, 0, position.z).normalized;
        }

        /// <summary>
        /// Helper function to build the states & transitions of the state machine
        /// </summary>
        private void BuildStateMachine()
        {
            //----------------Set states----------------//

            _stateMachine.AddState(new State("Idle", EventArgs.Empty));

            _stateMachine.AddState(new State("MoveToStandby", new MovementEventArgs() { Destination = new Vector3(-2.5f, 6f, 0), RotationSpeed = 100 })
            {
                OnStateEnter = (a) =>
                {
                    SetNextCommand(RobotCommand.None);
                    OnMoveTo(a);
                }
            });

            _stateMachine.AddState(new State("Standby", new MovementEventArgs() { Destination = new Vector3(-2.5f, 6f, 0), RotationSpeed = 100 })
            {
                OnStateEnter = (a) =>
                {
                    SetCurrentWareType(WareTypes.Ware_Pickup);
                    RobotInStandby();
                }
            });

            _stateMachine.AddState(new State("MoveToConveyorPickup", new MovementEventArgs() { Destination = GetConveyorPosition(WareTypes.Ware_Pickup) - (GetConveyorDirection(WareTypes.Ware_Pickup) * 3), RotationSpeed = 100 })
            {
                OnStateEnter = (a) =>
                {
                    SetNextCommand(RobotCommand.MoveToPalett);
                    OnMoveTo(a);
                }
            });

            _stateMachine.AddState(new State("MoveToPalett", new MovementEventArgs() { Destination = GetConveyorPosition(WareTypes.Ware_Pickup), RotationSpeed = 20 })
            {
                OnStateEnter = (a) =>
                {
                    SetNextCommand(RobotCommand.MoveToLift);
                    OnMoveTo(a);
                }
            });

            _stateMachine.AddState(new State("LiftPalett", new MovementEventArgs() { Destination = GetConveyorPosition(WareTypes.Ware_Pickup) + (Vector3.up * 0.5f), RotationSpeed = 2 })
            {
                OnStateEnter = (a) =>
                {
                    SetNextCommand(RobotCommand.CloseTool);
                    OnMoveTo(a);
                }
            });

            _stateMachine.AddState(new State("LiftWareFromConveyor", new MovementEventArgs() { Destination = GetConveyorPosition(WareTypes.Ware_Pickup) - (GetConveyorDirection(WareTypes.Ware_Pickup) * 4f) + (Vector3.up * 0.25f), RotationSpeed = 60 })
            {
                OnStateEnter = (a) =>
                {
                    SetNextCommand(RobotCommand.MoveToConveyor);
                    OnMoveTo(a);
                }
            });


            _stateMachine.AddState(new State("MoveToConveyorWare", new MovementEventArgs() { Destination = Vector3.up * 0.5f, RotationSpeed = 40 })
            {
                OnStateEnter = (a) =>
                {
                    SetNextCommand(RobotCommand.MoveToLift);
                    OnMoveToWare(new MovementEventArgs()
                    {
                        Destination = (GetConveyorDirection(GetCurrentWareType()) * -3.5f) + (a as MovementEventArgs).Destination,
                        RotationSpeed = (a as MovementEventArgs).RotationSpeed
                    });
                }
            });

            _stateMachine.AddState(new State("MoveToDrop", new MovementEventArgs() { Destination = Vector3.up * 0.5f, RotationSpeed = 50 })
            {
                OnStateEnter = (a) =>
                {
                    SetNextCommand(RobotCommand.OpenTool);
                    OnMoveToWare(a);
                }
            });

            _stateMachine.AddState(new State("DropPalett", new MovementEventArgs() { Destination = Vector3.zero, RotationSpeed = 1 })
            {
                OnStateEnter = (a) =>
                {
                    SetNextCommand(RobotCommand.MoveToConveyor);
                    OnMoveToWare(a);
                }
            });

            _stateMachine.AddState(new State("DepartFromPalett", new MovementEventArgs() { Destination = Vector3.zero, RotationSpeed = 20 })
            {
                OnStateEnter = (a) =>
                {
                    SetNextCommand(RobotCommand.MoveToStandBy);
                    OnMoveToWare(new MovementEventArgs()
                    {
                        Destination = (GetConveyorDirection(GetCurrentWareType()) * -3.5f),
                        RotationSpeed = (a as MovementEventArgs).RotationSpeed
                    });
                }
            });

            _stateMachine.AddState(new State("OpenTool", EventArgs.Empty)
            {
                OnStateEnter = (a) =>
                {
                    SetNextCommand(RobotCommand.MoveToConveyor);
                    OnOpenTool(a);
                }
            });
            _stateMachine.AddState(new State("CloseTool", EventArgs.Empty)
            {
                OnStateEnter = (a) =>
                {
                    SetNextCommand(RobotCommand.None);
                    OnCloseTool(a);
                }
            });



            //----------------Set transitions----------------//

            _stateMachine.AddTransition("Idle", "MoveToStandby", RobotCommand.MoveToStandBy);
            _stateMachine.AddTransition("MoveToStandby", "Standby", RobotCommand.None);
            _stateMachine.AddTransition("Standby", "MoveToConveyorPickup", RobotCommand.MoveToConveyor);

            _stateMachine.AddTransition("MoveToConveyorPickup", "MoveToPalett", RobotCommand.MoveToPalett);
            _stateMachine.AddTransition("MoveToPalett", "LiftPalett", RobotCommand.MoveToLift);
            _stateMachine.AddTransition("LiftPalett", "CloseTool", RobotCommand.CloseTool);
            _stateMachine.AddTransition("CloseTool", "LiftWareFromConveyor", RobotCommand.MoveToConveyor);

            _stateMachine.AddTransition("LiftWareFromConveyor", "MoveToConveyorWare", RobotCommand.MoveToConveyor);

            _stateMachine.AddTransition("MoveToConveyorWare", "MoveToDrop", RobotCommand.MoveToLift);
            _stateMachine.AddTransition("MoveToDrop", "OpenTool", RobotCommand.OpenTool);
            _stateMachine.AddTransition("OpenTool", "DropPalett", RobotCommand.MoveToPalett);
            _stateMachine.AddTransition("DropPalett", "DepartFromPalett", RobotCommand.MoveToConveyor);

            _stateMachine.AddTransition("DepartFromPalett", "MoveToStandby", RobotCommand.MoveToStandBy);
            _stateMachine.AddTransition("DepartFromPalett", "MoveToConveyorPickup", RobotCommand.MoveToConveyor);


            //----------------Set initial state----------------//
            _stateMachine.SetInitialState("Idle");
        }
    }
}

