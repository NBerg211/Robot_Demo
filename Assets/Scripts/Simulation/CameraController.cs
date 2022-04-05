using UnityEngine;

namespace Simulation
{
    /// <summary>
    /// PLAIN UNITY PART
    /// Controlls the user inputs to rotate the camera and show/hide the movement destination
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        public float RotationSpeed;
        public GameObject DestinationObject;
 
        /// <summary>
        /// Update is called from the UnityEngine once per frame
        /// </summary>
        void Update()
        {
            RotateCamera();
            ToggleDestinationVisible();
        }

        /// <summary>
        /// Rotate the camera, if the user presses left or right arrow key
        /// </summary>
        void RotateCamera()
        {
            float horizontalInput = 0;
            if (Input.GetKey(KeyCode.RightArrow)) horizontalInput = -1;
            if (Input.GetKey(KeyCode.LeftArrow)) horizontalInput = 1;

            transform.Rotate(Vector3.up, horizontalInput * RotationSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Show/Hide destination object
        /// </summary>
        void ToggleDestinationVisible()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                DestinationObject.SetActive(!DestinationObject.activeSelf);
        }
    }

}
