namespace BililiveRecorder.Common.Api
{
    public interface ICookieTester
    {
        Task<(bool, string)> TestCookieAsync();
    }
}
