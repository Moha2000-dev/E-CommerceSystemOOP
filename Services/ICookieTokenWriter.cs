namespace E_CommerceSystem.Services
{
    public interface ICookieTokenWriter
    {
        void WriteAccessCookie(HttpResponse res, string jwt, int minutes);
        void WriteRefreshCookie(HttpResponse res, string token, int days);
        void ClearAuthCookies(HttpResponse res);
    }
}
