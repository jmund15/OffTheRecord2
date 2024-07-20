using Godot;
using System;

namespace TimeRobbers.BaseComponents
{
    public interface IDirectionComponent
    {
        public MovementDirection GetMovementDirection();
        public Vector2 GetDesiredDirection();
        public Vector2 GetDesiredDirectionNormalized();
        //public Vector2 GetCurrentDirection();
        //public Vector2 GetCurrentDirectionNormalized();
        public static MovementDirection GetOppositeDirection(MovementDirection direction)
        {
            switch (direction)
            {
                case MovementDirection.DOWN:
                    return MovementDirection.UP;
                case MovementDirection.UP:
                    return MovementDirection.DOWN;
                case MovementDirection.LEFT:
                    return MovementDirection.RIGHT;
                case MovementDirection.RIGHT:
                    return MovementDirection.LEFT;
                case MovementDirection.DOWNLEFT:
                    return MovementDirection.UPRIGHT;
                case MovementDirection.DOWNRIGHT:
                    return MovementDirection.UPLEFT;
                case MovementDirection.UPLEFT:
                    return MovementDirection.DOWNRIGHT;
                case MovementDirection.UPRIGHT:
                    return MovementDirection.DOWNLEFT;
                default:
                    GD.Print("not any face direction?? facedir = " + direction.ToString());
                    return MovementDirection.RIGHT;
            }
        }
        public static bool AreDirectionsOpposite(MovementDirection dir1, MovementDirection dir2)
        {
            switch (dir1)
            {
                case MovementDirection.DOWN:
                    switch (dir2)
                    {
                        case MovementDirection.UP or MovementDirection.UPRIGHT or MovementDirection.UPLEFT:
                            return true;
                        default:
                            return false;
                    }
                case MovementDirection.UP:
                    switch (dir2)
                    {
                        case MovementDirection.DOWN or MovementDirection.DOWNRIGHT or MovementDirection.DOWNLEFT:
                            return true;
                        default:
                            return false;
                    }
                case MovementDirection.LEFT:
                    switch (dir2)
                    {
                        case MovementDirection.RIGHT or MovementDirection.DOWNRIGHT or MovementDirection.UPRIGHT:
                            return true;
                        default:
                            return false;
                    }
                case MovementDirection.RIGHT:
                    switch (dir2)
                    {
                        case MovementDirection.LEFT or MovementDirection.DOWNLEFT or MovementDirection.UPLEFT:
                            GD.Print("defender is facing " + dir1.ToString() + " and attacker is facing " + dir2.ToString());
                            return true;
                        default:
                            return false;
                    }
                case MovementDirection.DOWNLEFT:
                    switch (dir2)
                    {
                        case MovementDirection.UPRIGHT or MovementDirection.UP or MovementDirection.RIGHT:
                            return true;
                        default:
                            return false;
                    }
                case MovementDirection.DOWNRIGHT:
                    switch (dir2)
                    {
                        case MovementDirection.UPLEFT or MovementDirection.UP or MovementDirection.LEFT:
                            return true;
                        default:
                            return false;
                    }
                case MovementDirection.UPLEFT:
                    switch (dir2)
                    {
                        case MovementDirection.DOWNRIGHT or MovementDirection.DOWN or MovementDirection.RIGHT:
                            return true;
                        default:
                            return false;
                    }
                case MovementDirection.UPRIGHT:
                    switch (dir2)
                    {
                        case MovementDirection.DOWNLEFT or MovementDirection.DOWN or MovementDirection.LEFT:
                            return true;
                        default:
                            return false;
                    }
                default:
                    GD.Print("defender not any face direction?? facedir = " + dir1.ToString());
                    return false;
            }
        }
        public static MovementDirection GetDirectionFromVector(Vector2 direction)
        {
            if (direction.X < 0)
            {
                switch (direction.X / direction.Y + float.Epsilon)
                {
                    case float n when (Math.Abs(n) > 2):
                        return MovementDirection.LEFT;
                    case float n when (Math.Abs(n) > 0.5):
                        if (direction.Y >= 0)
                        {
                            return MovementDirection.DOWNLEFT;
                        }
                        else
                        {
                            return MovementDirection.UPLEFT;
                        }
                    default:
                        if (direction.Y >= 0)
                        {
                            return MovementDirection.DOWN;
                        }
                        else
                        {
                            return MovementDirection.UP;
                        }
                }
            }
            else
            {
                switch (direction.X / direction.Y + float.Epsilon)
                {
                    case float n when (Math.Abs(n) > 2):
                        return MovementDirection.RIGHT;
                    case float n when (Math.Abs(n) > 0.5):
                        if (direction.Y >= 0)
                        {
                            return MovementDirection.DOWNRIGHT;
                        }
                        else
                        {
                            return MovementDirection.UPRIGHT;
                        }
                    default:
                        if (direction.Y >= 0)
                        {
                            return MovementDirection.DOWN;
                        }
                        else
                        {
                            return MovementDirection.UP;
                        }
                }
            }
            //if (direction.X < 0)
            //{
            //    return MovementDirection.LEFT;
            //}
            //else if (direction.X > 0)
            //{
            //    return MovementDirection.RIGHT;
            //}
            //else if (direction.Y < 0)
            //{
            //    return MovementDirection.UP;
            //}
            //else
            //{
            //    return MovementDirection.DOWN;
            //}
        }
        public static string GetFaceDirectionString(MovementDirection direction)
        {
            switch (direction)
            {
                case MovementDirection.DOWN:
                    return "Down";
                case MovementDirection.UP:
                    return "Up";
                case MovementDirection.LEFT:
                    return "Left";
                case MovementDirection.RIGHT:
                    return "Right";
                case MovementDirection.DOWNLEFT:
                    return "DownLeft";
                case MovementDirection.DOWNRIGHT:
                    return "DownRight";
                case MovementDirection.UPLEFT:
                    return "UpLeft";
                case MovementDirection.UPRIGHT:
                    return "UpRight";
                default:
                    GD.PrintErr("not any face direction?? facedir = " + direction.ToString());
                    return "Null";
            }
        }
        public static Vector2 GetVectorFromDirection(MovementDirection dir)
        {
            switch (dir) //RETURN NORMALIZED VECTOR
            {
                case MovementDirection.DOWN:
                    return new Vector2(0, 1);
                case MovementDirection.UP:
                    return new Vector2(0, -1);
                case MovementDirection.LEFT:
                    return new Vector2(-1, 0);
                case MovementDirection.RIGHT:
                    return new Vector2(1, 0);
                case MovementDirection.DOWNLEFT:
                    return new Vector2(-0.707f, 0.707f);
                case MovementDirection.DOWNRIGHT:
                    return new Vector2(0.707f, 0.707f);
                case MovementDirection.UPLEFT:
                    return new Vector2(-0.707f, -0.707f);
                case MovementDirection.UPRIGHT:
                    return new Vector2(0.707f, -0.707f);
                default:
                    GD.Print("not any face direction?? facedir = " + dir.ToString());
                    return Vector2.Zero;
            }
        }
        public static int GetFrameFromDirection(MovementDirection dir)
        {
            switch (dir) //RETURN NORMALIZED VECTOR
            {
                case MovementDirection.DOWN:
                    return 2;
                case MovementDirection.UP:
                    return 6;
                case MovementDirection.LEFT:
                    return 4;
                case MovementDirection.RIGHT:
                    return 0;
                case MovementDirection.DOWNLEFT:
                    return 3;
                case MovementDirection.DOWNRIGHT:
                    return 1;
                case MovementDirection.UPLEFT:
                    return 5;
                case MovementDirection.UPRIGHT:
                    return 7;
                default:
                    GD.Print("not any face direction?? facedir = " + dir.ToString());
                    throw new Exception("DIRECTION ERROR || INVALID MOVMEMENT DIRECTION");
                    //return -1;
            }
        }
    }
}
