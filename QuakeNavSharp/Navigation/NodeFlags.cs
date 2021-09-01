using System;

namespace QuakeNavSharp.Navigation
{
    public sealed partial class NavigationGraph
    {
        [Flags]
        public enum NodeFlags : ushort
        {
            None = 0,
            Teleporter = 1 << 0,
            Pusher = 1 << 1,
            ElevatorTop = 1 << 2,
            ElevatorBottom = 1 << 3,
            Underwater = 1 << 4,
            Hazard = 1 << 5
        }
    }
}
