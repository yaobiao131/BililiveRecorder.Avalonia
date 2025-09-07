namespace BililiveRecorder.Common.Api.Danmaku
{
    public class StatusChangedEventArgs : EventArgs
    {
        public static readonly StatusChangedEventArgs True = new()
        {
            Connected = true
        };
        public static readonly StatusChangedEventArgs False = new()
        {
            Connected = false
        };

        public bool Connected { get; set; }
    }
}
