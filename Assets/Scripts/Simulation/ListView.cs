using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Simulation
{
    /// <summary>
    /// PLAIN UNITY PART
    /// Quick & dirty ListView to show a list of strings in the UI
    /// 
    /// /!\ /!\ CAUTION /!\ /!\
    /// 
    /// Based on:
    /// https://stackoverflow.com/questions/40928336/how-to-create-a-basic-listview-in-unity
    /// 
    /// /!\ /!\ CAUTION /!\ /!\
    /// </summary>
    public class ListView : MonoBehaviour
    {
        public UnityEngine.UI.VerticalLayoutGroup verticalLayoutGroup;
        public GameObject ItemPrefab;
        private int _listSize = 10;
        private List<GameObject> _listItems = new List<GameObject>();

        /// <summary>
        /// Start is called from the UnityEngine
        /// </summary>
        void Start()
        {
            RectTransform parent = verticalLayoutGroup.GetComponent<RectTransform>();
            for (int i = 0; i < _listSize; ++i)
            {
                GameObject go = Instantiate(ItemPrefab);
                _listItems.Add(go);
                go.GetComponent<TextMeshProUGUI>().text = "";
                go.transform.SetParent(parent);
            }
        }

        /// <summary>
        /// Add a new item to the list
        /// </summary>
        /// <param name="itemText"></param>
        public void AddItem(string itemText)
        {
            for (int i = _listItems.Count - 1; i > 0; --i)
            {
                _listItems[i].GetComponent<TextMeshProUGUI>().text = _listItems[i - 1].GetComponent<TextMeshProUGUI>().text;
            }
            if (_listItems.Count > 0) _listItems.First().GetComponent<TextMeshProUGUI>().text = itemText;
        }

    }

}
