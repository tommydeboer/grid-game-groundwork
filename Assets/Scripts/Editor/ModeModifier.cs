namespace Editor
{
    internal class ModeModifier
    {
        readonly State state;
        readonly Mode mode;
        bool keyWasPressed;
        bool active;

        public ModeModifier(State state, Mode mode)
        {
            this.state = state;
            this.mode = mode;
            keyWasPressed = false;
        }

        public void Check(bool keyIsPressed)
        {
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