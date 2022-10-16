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

        Movable movable;
        FMOD.Studio.EventInstance sfxMoving;

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
                case MovableEventType.TOPPLED:
                    break;
                case MovableEventType.LANDED:
                    PlayLandedSound();
                    break;
                case MovableEventType.START_SLIDE:
                    StartPlayingSlideSound();
                    break;
                case MovableEventType.STOP_SLIDE:
                    StopPlayingSlideSound();
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
    }
}