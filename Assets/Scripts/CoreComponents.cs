using UnityEngine;

namespace GridGame
{
    public static class CoreComponents
    {
        static Game game;

        public static Game Game
        {
            get
            {
                if (game == null)
                {
                    game = Object.FindObjectOfType<Game>();
                }

                return game;
            }
        }
    }
}