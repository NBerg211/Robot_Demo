using System.Collections.Generic;
using System.Text;
using UnityEngine;

using Simulation;
using Robot;


/// <summary>
/// Simple enumeration for ware types
/// </summary>
public enum WareTypes
{
    Ware_Pickup,
    Ware_Brick,
    Ware_Wood
}

/// <summary>
/// Instantiate & setup Controllers with objects from the Unity Editor
/// Provides methods to display outputs in the UI.
/// 'MonoBehaviour' is the base class for all scripts that interact with Unity objects and/or manipulation them
/// </summary>
public class DemoController : MonoBehaviour
{
    //------------ Properties -----------//

    //The objects like 'Motors' from the simulation. They are set in the Unity Eitor
    public List<GameObject> Wares = new List<GameObject>();
    public List<GameObject> Motors = new List<GameObject>();
    public GameObject MotorToolArm;
    public GameObject ToolPositionObject;
    public GameObject MotorToolForkLeft;
    public GameObject MotorToolForkRight;
    public GameObject MotorToolForkTop;
    public GameObject Target;
    public GameObject ToolObject;
    public GameObject PositionConveyorIn;
    public GameObject PositionConveyorBricks;
    public GameObject PositionConveyorWood;

    //Current Action List in the UI
    public ListView StateList;

    //Behavior scripts for the Simulation/UI 
    public SpawnController SpawnController;
    public CameraController CameraController;


    //These scripts are controlling all 'robot'-behaviour
    private IMovementController _movementController;
    private IToolController _toolController;
    private RobotController _robotController;



    //-------------------- Methods ---------------//

    /// <summary>
    /// Awake is called from the UnityEngine and can be used to initialize variables
    /// </summary>
    void Awake()
    {
        //Get all MotorControllers from the Unity objects
        List<IMotorController> motorControllers = new List<IMotorController>();
        foreach (GameObject obj in Motors)
        {
            motorControllers.Add(obj.GetComponent<UnityMotorController>());
        }

        //Setup a dictionary wich maps the ware type to the position of the related conveyor
        Dictionary<WareTypes, Vector3> conveyorPositions = new Dictionary<WareTypes, Vector3>()
        {
            {WareTypes.Ware_Pickup, PositionConveyorIn.transform.position},
            {WareTypes.Ware_Brick, PositionConveyorBricks.transform.position},
            {WareTypes.Ware_Wood, PositionConveyorWood.transform.position}
        };

        //Initialize an setup the ToolController
        _toolController = new UnityToolController();
        _toolController.MotorToolArm = MotorToolArm.GetComponent<UnityMotorController>();
        _toolController.MotorForkLeft = MotorToolForkLeft.GetComponent<UnityMotorController>();
        _toolController.MotorForkRight = MotorToolForkRight.GetComponent<UnityMotorController>();
        _toolController.MotorForkTop = MotorToolForkTop.GetComponent<UnityMotorController>();

        //The ToolPositionObject is a quick hack for easy getting the position & rotation of the tool 
        (_toolController as UnityToolController).ToolPositionObject = ToolPositionObject;

        //Create a new StateMachine and connect it with the UI
        StateMachine stateMachine = new StateMachine();
        stateMachine.DisplayCurrentState += OnDisplayCurrentState;

        //Create a new MovementController
        _movementController = new InverseKinematicController();
        _movementController.Motors = motorControllers;
        _movementController.Tool = _toolController;

        //Create a new RobotController
        _robotController = new RobotController(_movementController, _toolController, stateMachine, conveyorPositions);

        //Connect the robot events with the simulation
        _robotController.FreeWare += OnFreeWare;
        _robotController.SetWare += OnSetWareToTool;
        _robotController.RobotInStandby += SpawnController.OnSpawnWare;
        _robotController.DisplayCurrentDestination += OnDisplayCurrentDestination;

        //SpawnController spawns and destroy wares
        SpawnController.NewWareArived += OnNewWare;

        //CameraController controlls the rotation of the camera an user inputs
        CameraController.DestinationObject = Target;

        //Start the robot movements
        _robotController.StartRobot();
    }

    /// <summary>
    /// FixedUpdate is called from the UnityEngine
    /// On every update the controllers are updating the steering 
    /// </summary>
    void FixedUpdate()
    {
        _movementController.UpdateMovement();
        _toolController.Update();
    }

    /// <summary>
    ///Callback to quick & dirty fix some problems withe physics
    /// </summary>
    void OnSetWareToTool()
    {
        SpawnController.SetWareParent(ToolObject);
    }

    /// <summary>
    ///Callback to quick & dirty fix some problems withe physics 
    /// </summary>
    void OnFreeWare()
    {
        SpawnController.SetWareParent(gameObject);
    }

    /// <summary>
    ///Callback to simulate that a new ware has arrived on the conveyor belt 
    /// </summary>
    void OnNewWare()
    {
        _robotController.NewWareArived(SpawnController.GetCurrentWareType());
    }

    /// <summary>
    ///Callback to display the current action in the UI
    /// </summary>
    void OnDisplayCurrentState(string stateName)
    {
        StateList.AddItem(AddSpacesToSentence(stateName, false));
    }

    /// <summary>
    ///Callback the set the DestinationObject to a new position
    /// </summary>
    void OnDisplayCurrentDestination(Vector3 destination)
    {
        Target.transform.position = destination;
    }

    /// <summary>
    /// 
    /// /!\ /!\ CAUTION /!\ /!\
    /// 
    /// This method is copy-pasted from:
    /// https://stackoverflow.com/questions/272633/add-spaces-before-capital-letters
    /// 
    /// /!\ /!\ CAUTION /!\ /!\
    /// 
    /// Append a space in front of every character of a string
    /// </summary>
    string AddSpacesToSentence(string text, bool preserveAcronyms)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;
        StringBuilder newText = new StringBuilder(text.Length * 2);
        newText.Append(text[0]);
        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]))
                if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                    (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                     i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                    newText.Append(' ');
            newText.Append(text[i]);
        }
        return newText.ToString();
    }
}
