using UnityEngine;

namespace Robot
{
    /// <summary>
    /// MotorController for the Unity-'Motor'-Objects. Implements the IMotorInterface
    /// </summary>
    public class UnityMotorController : MonoBehaviour, IMotorController
    {
        //SerializeField is used to get access to private properties in the UnityEditor
        [SerializeField]

        //max rotations of the motor
        private float _maxRotationSpeed = 2f;

        //Get maxRotationSpeed
        public float MaxRotationSpeed { get => _maxRotationSpeed; }

        /// <summary>
        /// returns the position of the tool in world coordinates
        /// </summary>
        /// <returns></returns>
        public Vector3 GetPosition()
        {
            return transform.position;
        }

        /// <summary>
        /// Returns the rotation of the tool in local euler angles
        /// </summary>
        /// <returns></returns>
        public Vector3 GetRotationAngle()
        {
            return transform.localEulerAngles;
        }

        /// <summary>
        /// Rotate the Motor by the angle with the 0-100% of the max rotation speed
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="rotationSpeedPercentage"></param>
        public void Rotate(float angle, float rotationSpeedPercentage = 1f)
        {
            //some basic error handling
            if (rotationSpeedPercentage > 1) rotationSpeedPercentage = 1;
            if (rotationSpeedPercentage < 0) rotationSpeedPercentage = 0;

            transform.Rotate(Vector3.up * angle * (_maxRotationSpeed * rotationSpeedPercentage));
        }
    }

}
