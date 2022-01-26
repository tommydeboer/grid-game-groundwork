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
                    grid = GameObject.FindWithTag("Grid").GetComponent<Grid>();
                }

                return grid;
            }
        }
    }
}