using System;
using UnityEngine;

namespace GridGame.DevTools
{
    public class DevTools : MonoBehaviour
    {
        void Awake()
        {
            if (!Debug.isDebugBuild)
            {
                gameObject.SetActive(false);
            }
        }
    }
}