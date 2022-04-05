using System.Collections.Generic;
using UnityEngine;

namespace Robot
{
    /// <summary>
    /// The IMovementController is the interface for every class that controlls the movements of the robot arm.
    /// Specifically these controllers rotate the provided motors to reach the set destination.
    /// 
    /// Call SetToolTargetPosition(...) to set the destination. The Robot will try to get the tool to this position
    /// Use MovementThreshold to increase/decrease precision
    /// OnTargetReached is called, when the destination is reached
    /// </summary>
    public interface IMovementController
    {
        // Delegate & event, which should be raised if the destination is reached
        public delegate void DelegateOnTargetReached();
        public event DelegateOnTargetReached OnTargetReached;

        
        /// <summary>
        /// The precision the controller will try to reach the destination before raising the event.
        /// </summary>
        /// <value></value>
        public float MovementThreshold { get; set; }

        //Get/Set the ToolController
        public IToolController Tool { get; set; }

        /// <summary>
        /// List of MotorControllers of the robot. These motors are used to move the arm
        /// </summary>
        /// <value></value>
        public List<IMotorController> Motors { get; set; }

        /// <summary>
        /// Set the destination the tool should be moved to.
        /// </summary>
        /// <param name="position">Position of the destination</param>
        /// <param name="maxRotationSpeedPercentage">Rotation speed of the motors in percent</param>
        public void SetToolTargetPosition(Vector3 position, float maxRotationSpeedPercentage);

        /// <summary>
        /// UpdateMovement is called from DemoController on every frame. Here the new rotation angles of the motors are calculated
        /// </summary>
        public void UpdateMovement();

    }

}
