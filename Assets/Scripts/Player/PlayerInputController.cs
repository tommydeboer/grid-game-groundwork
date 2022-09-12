using GridGame.SO;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GridGame.Player
{
    public class PlayerInputController : MonoBehaviour
    {
        [SerializeField]
        GameLoopEventChannelSO gameLoopEventChannelSo;

        public bool HoldingUndo { get; private set; }
        public bool HasDirectionChanged { get; private set; }
        public Vector3Int CurrentMovementDir { get; private set; }
        public bool InputAllowed { get; private set; } = true;

        Vector2 currentInput;

        void OnEnable()
        {
            gameLoopEventChannelSo.OnInputStart += OnInputStart;
            gameLoopEventChannelSo.OnInputEnd += OnInputEnd;
        }

        void OnDisable()
        {
            gameLoopEventChannelSo.OnInputStart -= OnInputStart;
            gameLoopEventChannelSo.OnInputEnd -= OnInputEnd;
        }

        void OnInputStart() => InputAllowed = true;
        void OnInputEnd() => InputAllowed = false;

        [UsedImplicitly]
        public void OnUndo(InputValue value)
        {
            HoldingUndo = value.isPressed;
        }

        [UsedImplicitly]
        public void OnMove(InputValue value)
        {
            var movement = value.Get<Vector2>();

            if (movement.magnitude > 1)
            {
                // two buttons pressed at once, latest counts
                if (currentInput.x == 0 && movement.x != 0)
                {
                    movement = new Vector2(movement.x, 0);
                }
                else
                {
                    movement = new Vector2(0, movement.y);
                }
            }

            currentInput = movement;

            var dir = new Vector3Int((int)movement.x, 0, (int)movement.y);

            HasDirectionChanged = dir != CurrentMovementDir;
            CurrentMovementDir = dir;
        }
    }
}