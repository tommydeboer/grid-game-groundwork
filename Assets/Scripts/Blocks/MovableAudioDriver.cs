using System;
using System.Collections;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GridGame.Blocks
{
    public class MovableAudioDriver : MonoBehaviour
    {
        [SerializeField]
        FMODUnity.EventReference LandedEvent;

        [SerializeField]
        FMODUnity.EventReference SlidingEvent;

        [SerializeField]
        FMODUnity.EventReference ToppledEvent;

        [SerializeField]
        FMODUnity.EventReference ToppleStartEvent;

        Movable movable;
        EventInstance sfxMoving;

        bool isSliding;

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
                case MovableEventType.LANDED_FALL:
                    PlayOneShotIfNotNull(LandedEvent, transform.position);
                    break;
                case MovableEventType.START_SLIDE:
                    StartPlayingSlideSound();
                    break;
                case MovableEventType.STOP_SLIDE:
                    StopPlayingSlideSound();
                    break;
                case MovableEventType.LANDED_TOPPLE:
                    PlayOneShotIfNotNull(ToppledEvent, transform.position);
                    break;
                case MovableEventType.START_TOPPLE:
                    PlayOneShotIfNotNull(ToppleStartEvent, transform.position);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(e), e, null);
            }
        }

        void StartPlayingSlideSound()
        {
            if (isSliding || SlidingEvent.IsNull) return;
            isSliding = true;
            sfxMoving.StartIfNotPlaying();
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(sfxMoving, GetComponent<Transform>());
        }

        void StopPlayingSlideSound()
        {
            if (!isSliding || SlidingEvent.IsNull) return;
            isSliding = false;
            StartCoroutine(StopPlayingSlideSoundConditionally());
        }

        /// The slide sound continues when a player keeps pushing and nothing falls between two moves. To make sure the
        /// player has stopped pushing, we wait for two frames and then stop the sound.
        IEnumerator StopPlayingSlideSoundConditionally()
        {
            yield return WaitFor.EndOfFrame;
            yield return WaitFor.EndOfFrame;

            if (!isSliding)
            {
                sfxMoving.stop(STOP_MODE.ALLOWFADEOUT);
            }
        }

        static void PlayOneShotIfNotNull(FMODUnity.EventReference e, Vector3 position)
        {
            if (!e.IsNull)
            {
                FMODUnity.RuntimeManager.PlayOneShot(e, position);
            }
        }
    }
}