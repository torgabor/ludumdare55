using UnityEditor;
using UnityEngine;

namespace Assets.Scripts
{
    public interface IMiniGame
    {
        void StopMiniGame();
        void Enable();
        void Disable();
    }
}