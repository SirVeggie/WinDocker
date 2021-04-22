using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinUtilities;

namespace Apprentice.Tools {
    public enum Direction {
        None,
        Left,
        Right,
        Up,
        Down
    }

    public static class DirectionExtensions {
        public static Direction Reverse(this Direction dir) {
            switch (dir) {
            case Direction.None:
                return Direction.None;
            case Direction.Left:
                return Direction.Right;
            case Direction.Right:
                return Direction.Left;
            case Direction.Up:
                return Direction.Down;
            case Direction.Down:
                return Direction.Up;
            default:
                return dir;
            }
        }

        public static Direction Clockwise(this Direction dir) {
            switch (dir) {
            case Direction.None:
                return Direction.None;
            case Direction.Left:
                return Direction.Up;
            case Direction.Right:
                return Direction.Down;
            case Direction.Up:
                return Direction.Right;
            case Direction.Down:
                return Direction.Left;
            default:
                return dir;
            }
        }

        public static Direction CounterClockwise(this Direction dir) => dir.Clockwise().Reverse();

        public static bool IsVertical(this Direction dir) => dir == Direction.Up || dir == Direction.Down;

        public static bool IsHorizontal(this Direction dir) => dir == Direction.Left || dir == Direction.Right;

        public static bool IsUpOrLeft(this Direction dir) => dir == Direction.Left || dir == Direction.Up;

        public static Direction AsDirection(this Key key) {
            switch (key) {
            case Key.Left:
                return Direction.Left;
            case Key.Right:
                return Direction.Right;
            case Key.Up:
                return Direction.Up;
            case Key.Down:
                return Direction.Down;
            case Key.A:
                return Direction.Left;
            case Key.D:
                return Direction.Right;
            case Key.W:
                return Direction.Up;
            case Key.S:
                return Direction.Down;
            default:
                return Direction.None;
            }
        }

        public static int AsInt(this Direction dir) => dir == Direction.None ? 0 : dir.IsUpOrLeft() ? -1 : 1;
    }
}
