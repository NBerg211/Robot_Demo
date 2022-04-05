using System.Collections.Generic;
using UnityEngine;

namespace Robot
{
    /// <summary>
    /// MovementController to steer the robot with the Inverse Kinematic Algorithm
    /// Implements the IMovementController interface
    ///     
    /// /!\ /!\ CAUTION /!\ /!\
    /// 
    /// Inverse kinematic algorithm based on:
    /// https://www.youtube.com/watch?v=VdJGouwViPs&ab_channel=Guidev
    /// 
    /// /!\ /!\ CAUTION /!\ /!\
    /// </summary>
    public class InverseKinematicController : IMovementController
    {
        //------------ Properties -----------//

        private List<IMotorController> _motors;
        private IToolController _tool;
        private Vector3 _targetPosition;
        private float _movementThreshold = 0.1f;
        private float _rotationSpeedPercentage = 1.0f;

        public List<IMotorController> Motors { get => _motors; set => _motors = value; }
        public IToolController Tool { get => _tool; set => _tool = value; }
        public float MovementThreshold { get => _movementThreshold; set => _movementThreshold = value; }

        public event IMovementController.DelegateOnTargetReached OnTargetReached;



        //-------------------- Methods ---------------//

        /// <summary>
        /// Set the new destination and rotation speed of the motors
        /// </summary>
        /// <param name="position"></param>
        /// <param name="maxRotationSpeedPercentage"></param>
        public void SetToolTargetPosition(Vector3 position, float maxRotationSpeedPercentage)
        {
            if (maxRotationSpeedPercentage <= 0) maxRotationSpeedPercentage = 0.01f;
            if (maxRotationSpeedPercentage > 100) maxRotationSpeedPercentage = 1f;

            _rotationSpeedPercentage = maxRotationSpeedPercentage / 100;

            _targetPosition = position;
        }

        /// <summary>
        /// Update the rotation angle of the motors
        /// </summary>
        public void UpdateMovement()
        {
            //If the distance between the tool position and the destination, raise DestinationReached event and return
            if (Vector3.Distance(_tool.GetToolPosition(), _targetPosition) < _movementThreshold)
            {
                OnTargetReached();
                return;
            }

            //get the new rotation angle for every motor and rotate it
            foreach (var motor in _motors)
            {
                float angle = CalculateAngle(motor);
                motor.Rotate(-angle, _rotationSpeedPercentage);
            }
        }

 
        /// <summary>
        /// The actual inverse kinematic algorithm
        /// Calculates an rotation angle with gradient descent
        /// /// Based on:
        /// https://www.youtube.com/watch?v=VdJGouwViPs&ab_channel=Guidev
        /// </summary>
        /// <param name="motor">The MotorController for which the new angle should be calculated</param>
        /// <returns>The new rotation angle</returns>
        float CalculateAngle(IMotorController motor)
        {
            float deltaTheta = 0.01f;

            float distance1 = Vector3.Distance(_tool.GetToolPosition(), _targetPosition);
            motor.Rotate(deltaTheta, motor.MaxRotationSpeed);
            float distance2 = Vector3.Distance(_tool.GetToolPosition(), _targetPosition);
            motor.Rotate(-deltaTheta, motor.MaxRotationSpeed);

            return (distance2 - distance1) / deltaTheta;
        }
    }

}
