using System;
using GridGame.SO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GridGame.DevTools
{
    public class LevelPicker : MonoBehaviour
    {
        [SerializeField]
        LoadEventChannelSO loadEventChannel;

        [SerializeField]
        SceneList levelList;

        void Update()
        {
            for (int i = 41; i < 50; i++)
            {
                if (Keyboard.current[(Key)i].wasPressedThisFrame)
                {
                    var level = levelList.GetAt(i - 41);
                    if (level != null)
                    {
                        loadEventChannel.RequestSceneLoad(level, false);
                    }

                    break;
                }
            }
        }
    }
}