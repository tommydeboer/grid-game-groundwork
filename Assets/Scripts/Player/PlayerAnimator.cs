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
        Transform model;

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

        Hero hero;

        float moveTime;
        float stopTime;
        float pushAmount;
        bool isPressingMove;

        void Awake()
        {
            hero = GetComponent<Hero>();
        }

        void Update()
        {
            if (!isPressingMove)
            {
                stopTime += Time.deltaTime;
                moveTime = 0f;
                pushAmount = stopTime / timeToReturnFromPush;

                model.localPosition = Vector3.Lerp(model.localPosition, Vector3.zero, pushAmount);
            }
            else if (hero.IsBlocked)
            {
                moveTime += Time.deltaTime;
                stopTime = 0f;
                pushAmount = moveTime / timeToMaxPush;

                model.localPosition = Vector3.forward * maxPushDistance * pushCurve.Evaluate(pushAmount);
            }
        }

        [UsedImplicitly]
        public void OnMove(InputValue value)
        {
            isPressingMove = value.Get<Vector2>() != Vector2.zero;
        }
    }
}