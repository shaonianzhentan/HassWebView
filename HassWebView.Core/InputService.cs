namespace HassWebView.Core
{
    public static partial class InputService
    {
        public static void InjectText(string text)
        {
            PlatformInjectText(text);
        }

        static partial void PlatformInjectText(string text);
    }
}