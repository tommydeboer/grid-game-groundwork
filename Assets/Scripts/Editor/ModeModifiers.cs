using System;
using System.Collections.Generic;
using UnityEngine;

namespace GridGame.Editor
{
    internal class ModeModifiers
    {
        readonly State state;
        readonly Dictionary<EventModifiers, Mode> modifiers = new();
        EventModifiers currentModifiers = EventModifiers.None;
        bool reset;
        Mode modeBeforeModifiers;

        public ModeModifiers(State state)
        {
            this.state = state;
        }

        public void Register(EventModifiers eventMods, Mode mode)
        {
            modifiers[eventMods] = mode;
        }

        public void Evaluate(EventModifiers eventMods)
        {
            if (eventMods > 0 && (reset || !modifiers.ContainsKey(eventMods)))
            {
                return;
            }

            reset = false;

            switch (eventMods)
            {
                case > 0 when currentModifiers == 0:
                    currentModifiers = eventMods;
                    modeBeforeModifiers = state.Mode;
                    state.Mode = modifiers[eventMods];
                    return;
                case > 0 when eventMods != currentModifiers:
                    currentModifiers = eventMods;
                    state.Mode = modifiers[eventMods];
                    return;
                case 0 when currentModifiers != 0:
                    currentModifiers = 0;
                    state.Mode = modeBeforeModifiers;
                    return;
            }
        }

        public void Reset()
        {
            reset = true;
        }
    }
}