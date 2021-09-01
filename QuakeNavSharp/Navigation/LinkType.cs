namespace QuakeNavSharp.Navigation
{
    public sealed partial class NavigationGraph
    {
        public enum LinkType : ushort
        {
            Walk = 0,
            LongJump = 1,
            Teleport = 2,
            WalkOffLedge = 3,
            Pusher = 4,
            BarrierJump = 5,
            Elevator = 6,
            Train = 7,
            ManualJump = 8,
            Unknown = 9
        }
    }
}
