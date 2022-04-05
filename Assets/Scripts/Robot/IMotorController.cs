using UnityEngine;

namespace Robot
{
    /// <summary>
    /// IMotorController is the interface for every class that controlls a motor
    /// </summary>
    public interface IMotorController
    {
        /// <summary>
        /// Get/Set the max rotations per second for the motor.
        /// </summary>
        /// <value></value>
        public float MaxRotationSpeed { get; }

        /// <summary>
        /// Returns the rotation angle of the motor
        /// </summary>
        /// <returns></returns>
        public Vector3 GetRotationAngle();

        /// <summary>
        /// Rotate motor for an angle
        /// </summary>
        /// <param name="angle">angle for the rotation</param>
        /// <param name="rotationSpeed"> rotation speed from 0 to 1</param>
        public void Rotate(float angle, float rotationSpeed = 0.2f);
    }

}
