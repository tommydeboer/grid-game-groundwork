using DG.Tweening;
using GridGame.Blocks;
using UnityEngine;

namespace GridGame.Player
{
    public class PlayerInput : Mover
    {
        [SerializeField]
        bool debugLadders;

        public Ladder OnLadder { get; private set; }
        const float LadderOffset = 0.35f;
        public override BlockType Type => BlockType.Player;

        void Start()
        {
            if (Camera.main != null)
            {
                var playerCamera = Camera.main.GetComponent<PlayerCamera>();
                playerCamera.Player = this;
            }
            else
            {
                Debug.LogWarning("No main camera. Is the persistent scene loaded?");
            }
        }

        void Update()
        {
            if (CanInput())
            {
                CheckInput();
            }
        }

        static bool CanInput()
        {
            return !Game.isMoving && !Game.instance.holdingUndo;
        }

        void CheckInput()
        {
            float hor = Input.GetAxisRaw("Horizontal");
            float ver = Input.GetAxisRaw("Vertical");

            var direction = Vector3Int.zero;

            if (hor == 0 && ver == 0)
            {
                return;
            }

            if (hor != 0 && ver != 0)
            {
                if (direction == Vector3.right || direction == Vector3.left)
                {
                    hor = 0;
                }
                else
                {
                    ver = 0;
                }
            }

            if (hor == 1)
            {
                direction = Vector3Int.right;
            }
            else if (hor == -1)
            {
                direction = Vector3Int.left;
            }
            else if (ver == -1)
            {
                direction = Vector3Int.back;
            }
            else if (ver == 1)
            {
                direction = Vector3Int.forward;
            }

            TryPlayerMove(direction);
            Game.instance.DoScheduledMoves();
        }

        void LookAt(Vector3 dir)
        {
            var q = Quaternion.LookRotation(dir);
            transform.DORotate(q.eulerAngles, 0.1f);
        }


        void TryPlayerMove(Vector3Int dir)
        {
            var playerPos = Tile.gridPos;
            Vector3Int targetPos = playerPos + dir;
            var belowPlayer = playerPos + Vector3Int.down;

            if (OnLadder)
            {
                TryClimb(dir, playerPos, belowPlayer);
                return;
            }
            else if (Grid.HasOriented<Ladder>(belowPlayer, dir) && Grid.IsEmpty(targetPos) &&
                     Grid.IsEmpty(targetPos + Vector3Int.down))
            {
                LogLadderDebug("Mounting ladder from above");

                // mount ladder from above
                ScheduleMove(Vector3Int.down + (Vector3)dir * (1 - LadderOffset));
                OnLadder = Grid.Get<Ladder>(belowPlayer);
                LookAt(-dir);
            }
            else if (Grid.HasOriented<Ladder>(targetPos, -dir))
            {
                LogLadderDebug("Mounting ladder");

                ScheduleMove((Vector3)dir * LadderOffset);
                OnLadder = Grid.Get<Ladder>(targetPos);
                LookAt(dir);
            }
            else if (Grid.Has<Mover>(targetPos))
            {
                if (TryMove(dir))
                {
                    ScheduleMove(dir);
                    OnLadder = null;
                }
            }
            else if (Grid.IsEmpty(targetPos))
            {
                ScheduleMove(dir);
                OnLadder = null;
            }

            if (!OnLadder)
            {
                LookAt(dir);
            }
        }

        void TryClimb(Vector3Int dir, Vector3Int playerPos, Vector3Int belowPlayer)
        {
            // correct input direction based on ladder's orientation
            dir = Vector3Int.RoundToInt(Quaternion.Euler(OnLadder.Tile.rot) * dir);
            Vector3Int targetPos = playerPos + dir;

            var abovePlayer = playerPos + Vector3Int.up;
            var ladderPos = OnLadder.Tile.gridPos;
            if (targetPos == ladderPos)
            {
                if (Grid.Has<Block>(abovePlayer)) return;

                var aboveLadder = targetPos + Vector3Int.up;
                if (Grid.HasOriented<Ladder>(aboveLadder, -dir))
                {
                    LogLadderDebug("Climbing up ladder");

                    ScheduleMove(Vector3Int.up);
                    OnLadder = Grid.Get<Ladder>(aboveLadder);
                    LookAt(dir);
                }
                else if (Grid.IsEmpty(aboveLadder))
                {
                    LogLadderDebug("Climbing up ladder over edge");

                    ScheduleMove(Vector3Int.up + ((Vector3)dir * (1 - LadderOffset)));
                    OnLadder = null;
                }
            }
            else if (Grid.HasOriented<Ladder>(ladderPos + dir, OnLadder.Orientation))
            {
                if (TryMove(dir))
                {
                    LogLadderDebug("Climbing to neighbouring ladder");

                    ScheduleMove(dir);
                    OnLadder = Grid.Get<Ladder>(ladderPos + dir);
                }
            }
            else if (OnLadder == Grid.Get<Ladder>(playerPos - dir))
            {
                var belowLadder = ladderPos + Vector3Int.down;

                if (Grid.HasOriented<Ladder>(belowLadder, OnLadder.Orientation) && Grid.IsEmpty(belowPlayer))
                {
                    LogLadderDebug("Climbing down ladder");

                    ScheduleMove(Vector3Int.down);
                    OnLadder = Grid.Get<Ladder>(belowLadder);
                }
                else if (Grid.IsEmpty(belowLadder) && Grid.IsEmpty(belowPlayer))
                {
                    LogLadderDebug("Falling down ladder");

                    ScheduleMove(Vector3Int.down + ((Vector3)dir * LadderOffset));
                    OnLadder = null;
                }
                else
                {
                    LogLadderDebug("Stepping off ladder");

                    ScheduleMove((Vector3)dir * LadderOffset);
                    OnLadder = null;
                }
            }
            else if (Grid.HasOriented<Ladder>(playerPos + dir, -dir))
            {
                LogLadderDebug("Climbing to other ladder in corner");

                Vector3 directionToLadder = ((Vector3)playerPos - ladderPos).normalized;
                ScheduleMove((directionToLadder * LadderOffset) + ((Vector3)dir * LadderOffset));
                OnLadder = Grid.Get<Ladder>(playerPos + dir);
                LookAt(dir);
            }
            else
            {
                Vector3 directionToLadder = ((Vector3)playerPos - ladderPos).normalized;
                if (TryMove(dir))
                {
                    LogLadderDebug("Stepping off ladder sideways");

                    ScheduleMove(dir + (directionToLadder * LadderOffset));
                    OnLadder = null;
                }
            }

            if (!OnLadder)
            {
                LookAt(dir);
            }
        }


        protected override bool ShouldFall()
        {
            return !OnLadder && base.ShouldFall();
        }

        public void OnDrawGizmos()
        {
            if (debugLadders && OnLadder)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(OnLadder.Tile.pos, Vector3.one);
            }
        }

        void LogLadderDebug(string log)
        {
            if (debugLadders)
            {
                Debug.Log(log);
            }
        }
    }
}