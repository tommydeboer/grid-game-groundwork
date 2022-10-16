using System;
using UnityEngine;

namespace GridGame.Blocks
{
    public class MovableAudioDriver : MonoBehaviour
    {
        [SerializeField]
        FMODUnity.EventReference LandedEvent;

        [SerializeField]
        FMODUnity.EventReference SlidingEvent;

        Movable movable;
        FMOD.Studio.EventInstance sfxMoving;

        bool isSliding;
        Vector3 previousPos;

        bool IsSliding
        {
            set
            {
                if (value == isSliding || SlidingEvent.IsNull) return;
                if (value)
                {
                    sfxMoving.start();
                    FMODUnity.RuntimeManager.AttachInstanceToGameObject(sfxMoving, GetComponent<Transform>());
                }
                else
                {
                    sfxMoving.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                }

                isSliding = value;
            }
        }

        void Awake()
        {
            movable = GetComponent<Movable>();
        }

        void Start()
        {
            if (!SlidingEvent.IsNull)
            {
                sfxMoving = FMODUnity.RuntimeManager.CreateInstance(SlidingEvent);
                FMODUnity.RuntimeManager.AttachInstanceToGameObject(sfxMoving, GetComponent<Transform>());
            }

            previousPos = transform.position;
        }

        void Update()
        {
            var pos = transform.position;
            IsSliding = !movable.IsFalling && pos != previousPos;
            previousPos = pos;
        }

        void OnEnable()
        {
            movable.OnMovableEvent += HandleMoveEvent;
        }

        void OnDisable()
        {
            movable.OnMovableEvent -= HandleMoveEvent;
        }

        void OnDestroy()
        {
            sfxMoving.release();
        }


        void HandleMoveEvent(MovableEventType e)
        {
            switch (e)
            {
                case MovableEventType.NONE:
                    break;
                case MovableEventType.SLIDING:
                    break;
                case MovableEventType.TOPPLING:
                    break;
                case MovableEventType.FALLING:
                    break;
                case MovableEventType.LANDED:
                    PlayLandedSound();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(e), e, null);
            }
        }

        void PlayLandedSound()
        {
            if (!LandedEvent.IsNull)
            {
                FMODUnity.RuntimeManager.PlayOneShot(LandedEvent, transform.position);
            }
        }
    }
}