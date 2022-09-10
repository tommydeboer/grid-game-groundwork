using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GridGame.Player
{
    [RequireComponent(typeof(Hero))]
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField]
        Transform pivot;

        [Header("Push Feedback")]
        [SerializeField]
        AnimationCurve pushCurve;

        [SerializeField, Range(0f, 3f)]
        float timeToMaxPush = 1.5f;

        [SerializeField, Range(0f, 2f)]
        float timeToReturnFromPush = 0.25f;

        [SerializeField, Range(0f, .5f)]
        float maxPushDistance = .25f;

        [Header("Tilt feedback")]
        [SerializeField, Range(0f, .5f)]
        float timeToMaxTilt = .1f;

        [SerializeField, Range(0f, 45f)]
        float maxTiltAngle = 10;

        Hero hero;

        readonly ToAndFroTimer moveTime = new();
        readonly ToAndFroTimer pushTime = new();
        Vector3 pivotDefaultPosition;
        float pushAmount;
        float tiltAmount;
        bool isPressingMove;

        class ToAndFroTimer
        {
            public float ToTime { get; private set; }
            public float FroTime { get; private set; }

            public void Evaluate(bool condition)
            {
                if (condition)
                {
                    ToTime += Time.deltaTime;
                    FroTime = 0f;
                }
                else
                {
                    FroTime += Time.deltaTime;
                    ToTime = 0f;
                }
            }
        }

        void Awake()
        {
            hero = GetComponent<Hero>();
            pivotDefaultPosition = pivot.localPosition;
        }

        void Update()
        {
            moveTime.Evaluate(isPressingMove);
            pushTime.Evaluate(hero.IsBlocked);
            
            if (hero.IsBlocked && !hero.OnClimbable)
            {
                Push();
            }
            else
            {
                ReturnFromPush();
            }

            if (isPressingMove && !hero.OnClimbable)
            {
                Tilt();
            }
            else
            {
                ReturnFromTilt();
            }
        }

        void Tilt()
        {
            tiltAmount = Mathf.Clamp(moveTime.ToTime / timeToMaxTilt, 0f, 1f);
            float rotation = Mathf.SmoothStep(0, maxTiltAngle, tiltAmount);
            pivot.localRotation = Quaternion.Euler(rotation, 0, 0);
        }

        void ReturnFromTilt()
        {
            if (tiltAmount == 0) return;
            tiltAmount = 1 - Mathf.Clamp(moveTime.FroTime / timeToMaxTilt, 0f, 1f);
            float rotation = Mathf.SmoothStep(0, maxTiltAngle, tiltAmount);
            pivot.localRotation = Quaternion.Euler(rotation, 0, 0);
        }

        void Push()
        {
            pushAmount = pushTime.ToTime / timeToMaxPush;
            pivot.localPosition = pivotDefaultPosition +
                                  (Vector3.forward * maxPushDistance * pushCurve.Evaluate(pushAmount));
        }

        void ReturnFromPush()
        {
            pushAmount = pushTime.FroTime / timeToReturnFromPush;
            var localPosition = pivot.localPosition;
            pivot.localPosition = Vector3.Lerp(localPosition, pivotDefaultPosition, pushAmount);
        }

        [UsedImplicitly]
        public void OnMove(InputValue value)
        {
            isPressingMove = value.Get<Vector2>() != Vector2.zero;
        }
    }
}