using UnityEngine;

namespace GridGame
{
    public static class CoreComponents
    {
        static Grid grid;

        public static Grid Grid
        {
            get
            {
                if (grid == null)
                {
                    grid = GameObject.FindWithTag(Tags.GRID).GetComponent<Grid>();
                }

                return grid;
            }
        }

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