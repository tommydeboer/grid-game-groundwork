using UnityEngine;

namespace GridGame.Editor
{
    internal class ModeModifier
    {
        readonly State state;
        readonly Mode mode;
        readonly EventModifiers eventMods;
        bool keyWasPressed;
        bool active;

        public ModeModifier(State state, EventModifiers eventMods, Mode mode)
        {
            this.state = state;
            this.mode = mode;
            this.eventMods = eventMods;
            keyWasPressed = false;
        }

        public void Evaluate(EventModifiers eventMods)
        {
            bool keyIsPressed = this.eventMods == eventMods;
            switch (keyIsPressed)
            {
                case true when !keyWasPressed && !active:
                    state.Mode = mode;
                    active = true;
                    keyWasPressed = true;
                    break;
                case false when keyWasPressed && active:
                    state.Mode = state.PreviousMode;
                    active = false;
                    keyWasPressed = false;
                    break;
                case false:
                    keyWasPressed = false;
                    break;
            }
        }

        public void Reset()
        {
            active = false;
        }
    }
}