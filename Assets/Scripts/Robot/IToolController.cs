using UnityEngine;

namespace Robot
{
    /// <summary>
    /// The IToolController is the interface for every class, that implements the steering of tool.
    /// In this simulation the tool is a forklift with three movable arms to hold a palett with bricks or wood and a motor to 
    /// horizontaly align the tool.
    /// 
    /// Call OpenTool()/CloseTool() to open/close the movable arms.
    /// The events ToolOpened & ToolClosed a raised after the tool is fully opened/closed
    /// 
    /// Also provides methods to get the current tool position & rotation
    /// </summary>
    public interface IToolController
    {
        //Delegate and events to react, when the tool is fully opened/closed
        public delegate void DelegateOnToolFinished();

        public event DelegateOnToolFinished ToolOpened;
        public event DelegateOnToolFinished ToolClosed;

        //MotorControllers of the tool
        public IMotorController MotorForkLeft { get; set; }
        public IMotorController MotorForkRight { get; set; }
        public IMotorController MotorForkTop { get; set; }
        public IMotorController MotorToolArm { get; set; }

        /// <summary>
        /// Returns the position of the tool in world coordinates
        /// </summary>
        /// <returns></returns>
        public Vector3 GetToolPosition();

        /// <summary>
        /// Returns the rotation of the tool in euler angles
        /// </summary>
        /// <returns></returns>
        public Vector3 GetToolRotation();

        /// <summary>
        /// Call this to set the command to fully open the tool
        /// </summary>
        public void OpenTool();


        /// <summary>
        /// Call this to set the command to fully close the tool
        /// </summary>
        public void CloseTool();

        /// <summary>
        /// Calculate the new rotation angles of the motors to open/close and align the tool horizontal
        /// These methos is called every frame by the DemoController
        /// </summary>
        public void Update();
    }

}
