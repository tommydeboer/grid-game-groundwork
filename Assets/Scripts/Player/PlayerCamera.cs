using UnityEngine;

namespace GridGame.Player
{
    public class PlayerCamera : MonoBehaviour
    {
        [SerializeField, Range(3, 20)]
        float distance = 8f;

        [SerializeField, Range(0, 90)]
        float viewingAngle = 50f;

        [SerializeField, Range(0, 90)]
        float climbingRotation = 50;

        [SerializeField, Range(0, 1)]
        float movementSmoothTime = 0.3f;

        [SerializeField, Range(0, 1)]
        float rotationSmoothTime = 0.1f;

        Vector3 velocity = Vector3.zero;
        Vector2 currentOrbitAngles;
        Vector2 orbitVelocity = Vector2.zero;
        Transform cameraTransform;
        Transform playerTransform;

        Vector2 OrbitAngles
        {
            get
            {
                if (Player.OnClimbable)
                {
                    return Player.OnClimbable.Orientation switch
                    {
                        Direction.Right => new Vector2(viewingAngle, -climbingRotation),
                        Direction.Left => new Vector2(viewingAngle, climbingRotation),
                        Direction.Back => new Vector2(viewingAngle, 0f),
                        _ => new Vector2(viewingAngle, 180f)
                    };
                }
                else
                {
                    return new Vector2(viewingAngle, 0f);
                }
            }
        }

        Hero player;

        public Hero Player
        {
            get => player;
            set
            {
                player = value;
                playerTransform = player.transform;
                cameraTransform.localPosition = playerTransform.position;
                currentOrbitAngles = OrbitAngles;
            }
        }

        void Awake()
        {
            cameraTransform = transform;
        }

        void LateUpdate()
        {
            if (!Player) return;

            // smoothly rotate to viewing angle (based on player status: walking/climbing)
            currentOrbitAngles =
                Vector2.SmoothDamp(currentOrbitAngles, OrbitAngles, ref orbitVelocity, rotationSmoothTime);

            float moveTime = movementSmoothTime;
            if (currentOrbitAngles != OrbitAngles)
            {
                // if camera is orbiting, translate the camera quickly to prevent jerky movement
                moveTime = 0.02f;
            }

            // calculate new rotation and position
            Quaternion lookRotation = Quaternion.Euler(currentOrbitAngles);
            Vector3 lookDirection = lookRotation * Vector3.forward;
            Vector3 lookPosition = playerTransform.position - lookDirection * distance;

            // apply movement and rotation
            cameraTransform.localPosition =
                Vector3.SmoothDamp(cameraTransform.localPosition, lookPosition, ref velocity, moveTime);
            cameraTransform.rotation = lookRotation;
        }
    }
}