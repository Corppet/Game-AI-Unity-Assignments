using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    public class GInventory
    {
        private List<GameObject> items = new();

        public void AddItem(GameObject item)
        {
            items.Add(item);
        }

        public GameObject FindItemWithTag(string tag)
        {
            foreach (GameObject item in items)
            {
                if (item.CompareTag(tag))
                    return item;
            }

            return null;
        }

        public void RemoveItem(GameObject item)
        {
            items.Remove(item);
        }
    }
}
