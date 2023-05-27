namespace gVRC_bHaptics.Classes
{
    internal class OscCommand
    {
        public OscCommandType Type { get; set; }
        public string Path { get; set; }
        public object[] Value { get; set; }
    }
}
