using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Simulation
{
    /// <summary>
    /// PLAIN UNITY PART
    /// SpawnController spawns new wares and destroys the old wares, that are not used anymore
    /// </summary>
    public class SpawnController : MonoBehaviour
    {
        //Callbacks and properties
        public delegate void DelegateNewWare();
        public event DelegateNewWare NewWareArived;
        public List<GameObject> Wares;
        public Vector3 SpawnPosition;
        public GameObject CurrentWare = null;
        private WareTypes _currentWareType;

        /// <summary>
        /// Callback to spawn a new ware and destroy the old one
        /// </summary>
        public void OnSpawnWare()
        {
            if (CurrentWare != null) Destroy(CurrentWare, 0.5f);
            StartCoroutine(SpawnWare());
        }

        /// <summary>
        /// Returns the ware type of the ware, the has to be sorted
        /// </summary>
        /// <returns>Ware type of the new ware</returns>
        public WareTypes GetCurrentWareType()
        {
            return _currentWareType;
        }

        /// <summary>
        /// Quick & dirty fix for some problems with the physics
        /// </summary>
        /// <param name="parent"></param>
        public void SetWareParent(GameObject parent)
        {
            CurrentWare.transform.SetParent(parent.transform);
        }

        /// <summary>
        /// Parallel Job to spawn a new ware after a random time betwenn 0.5-2 seconds after calling
        /// </summary>
        /// <returns></returns>
        IEnumerator SpawnWare()
        {
            yield return new WaitForSeconds(Random.Range(0.5f, 2f));

            //randomly select the Ware type to be spawned
            if (UnityEngine.Random.Range(0f, 1f) >= 0.5)
            {
                CurrentWare = Instantiate(Wares[1], SpawnPosition, Wares[1].transform.rotation);
                _currentWareType = WareTypes.Ware_Brick;
            }
            else
            {
                CurrentWare = Instantiate(Wares[0], SpawnPosition, Wares[0].transform.rotation);
                _currentWareType = WareTypes.Ware_Wood;
            }
            CurrentWare.transform.position = SpawnPosition;

            //Inform the robot, that a new ware has arrived
            NewWareArived();
        }
    }

}
