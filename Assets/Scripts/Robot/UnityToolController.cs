using UnityEngine;

namespace Robot
{
    /// <summary>
    /// The Controller for the Unity-'Tool'-object. Opens and closes the arms by rotating the Motors. 
    /// Also tries to permantly align the tool horizontal by rotating the motor of the tool arm 
    /// </summary>
    public class UnityToolController : IToolController
    {
        //Some basic command states
        private enum ToolCommand
        {
            None,
            Open,
            Close
        };


        //------------ Properties -----------// 

        //the current command state of the tool
        private ToolCommand _currentCommand = ToolCommand.None;

        //max angle for an opened tool
        private float _maxForkAngle = 30f;

        //threshold for the precision of the fork angle
        private float _openClosedThreshold = 0.5f;

        //MotorControllers
        private IMotorController _motorForkLeft;
        private IMotorController _motorForkRight;
        private IMotorController _motorForkTop;
        private IMotorController _motorToolArm;
        public IMotorController MotorForkLeft { get => _motorForkLeft; set => _motorForkLeft = value; }
        public IMotorController MotorForkRight { get => _motorForkRight; set => _motorForkRight = value; }
        public IMotorController MotorForkTop { get => _motorForkTop; set => _motorForkTop = value; }
        public IMotorController MotorToolArm { get => _motorToolArm; set => _motorToolArm = value; }

        //Quick & dirty hack to easy get the tool position
        public GameObject ToolPositionObject;

        //Events, that are raised when the tool is fully opened/closed
        public event IToolController.DelegateOnToolFinished ToolOpened;
        public event IToolController.DelegateOnToolFinished ToolClosed;


        //-------------------- Methods ---------------//

        /// <summary>
        /// Set the command to open the tool
        /// </summary>
        public void OpenTool()
        {
            _currentCommand = ToolCommand.Open;
        }

        /// <summary>
        /// Set the command to close the tool
        /// </summary>
        public void CloseTool()
        {
            _currentCommand = ToolCommand.Close;
        }

        /// <summary>
        /// Returns the position of the tool in world coordinates
        /// </summary>
        /// <returns></returns>
        public Vector3 GetToolPosition()
        {
            return ToolPositionObject.transform.position;
        }

        /// <summary>
        /// Returns the rotation of the tool in local euler angles
        /// </summary>
        /// <returns></returns>
        public Vector3 GetToolRotation()
        {
            return ToolPositionObject.transform.rotation.eulerAngles;
        }

        /// <summary>
        /// Here the updates of the steering is calculated.
        /// Update is called every frame by the DemoController 
        /// </summary>
        public void Update()
        {
            //Try to horizantaly align the tool by rotating the motor of the tool arm
            AlignToolHorizontal();

            //If an open/close command is set, rotate the motors of the forks
            switch (_currentCommand)
            {
                case ToolCommand.Open:
                    OnOpenTool();
                    break;
                case ToolCommand.Close:
                    OnCloseTool();
                    break;
                default:
                    break;
            }

        }

        /// <summary>
        /// Rotate the motor the horizantaly align the tool
        /// </summary>
        private void AlignToolHorizontal()
        {
            _motorToolArm.Rotate((GetToolRotation().z - 180) * Time.deltaTime, 1f);
        }

        /// <summary>
        /// Open the tool. If the tool is fully opened, the current command is set to 'None' and the ToolOpened-event is raised
        /// </summary>
        private void OnOpenTool()
        {
            if (ForkMotorsRotateToAngle(-_maxForkAngle))
            {
                _currentCommand = ToolCommand.None;
                ToolOpened();
            }
        }

        /// <summary>
        /// Close the tool. If the tool is fully closed, the current command is set to 'None' and the ToolClosed-event is raised
        /// </summary>
        private void OnCloseTool()
        {
            if (ForkMotorsRotateToAngle(0))
            {
                _currentCommand = ToolCommand.None;
                ToolClosed();
            }
        }

        /// <summary>
        /// Rotate the fork motors to open/close the tool. Returns true, if the tool is opened/closed, otherwise false
        /// Quick & dirty...
        /// </summary>
        /// <param name="targetAngle"></param>
        /// <returns></returns>
        private bool ForkMotorsRotateToAngle(float targetAngle)
        {
            //value to remove the rotation of the Unity-object in the simulation
            float closedRotationAngle = 90;
            targetAngle += closedRotationAngle;

            //check if every fork has reached the open/close angle
            if (TargetAngleReached(_motorForkLeft.GetRotationAngle().x, targetAngle) &&
               TargetAngleReached(_motorForkLeft.GetRotationAngle().x, targetAngle) &&
               TargetAngleReached(_motorForkTop.GetRotationAngle().y, targetAngle - (_currentCommand == ToolCommand.Open ? 30 : closedRotationAngle)))
            {
                return true;
            }

            //calculate the new rotation angles
            float angleLeft = Vector3.Angle(_motorForkLeft.GetRotationAngle(), new Vector3(GetToolRotation().x + targetAngle, 0, 0));
            float angleRight = Vector3.Angle(_motorForkRight.GetRotationAngle(), new Vector3(GetToolRotation().x + targetAngle, 0, 0));
            float angleTop = Vector3.Angle(_motorForkTop.GetRotationAngle(), new Vector3(GetToolRotation().y + targetAngle, 0, 0));

            //check for every motor if the open/close angle is reached and rotate it, if the angle isn't reached
            if (!TargetAngleReached(_motorForkLeft.GetRotationAngle().x, targetAngle))
                MotorForkLeft.Rotate((_currentCommand == ToolCommand.Open ? angleLeft : -angleLeft) * Time.deltaTime);
            if (!TargetAngleReached(_motorForkRight.GetRotationAngle().x, targetAngle))
                MotorForkRight.Rotate((_currentCommand == ToolCommand.Open ? -angleRight : angleRight) * Time.deltaTime);
            if (!TargetAngleReached(_motorForkTop.GetRotationAngle().y, targetAngle - (_currentCommand == ToolCommand.Open ? 30 : closedRotationAngle)))
                MotorForkTop.Rotate((_currentCommand == ToolCommand.Open ? angleTop : -angleTop) * Time.deltaTime);

            return false;
        }

        /// <summary>
        /// Check if the angle is inside the thresholds of the target angle
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="targetAngle"></param>
        /// <returns></returns>
        private bool TargetAngleReached(float angle, float targetAngle)
        {
            return angle > (targetAngle - _openClosedThreshold) && angle < (targetAngle + _openClosedThreshold);
        }
    }
}
