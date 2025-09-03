namespace E_CommerceSystem.Services
{
    public class CookieTokenWriter : ICookieTokenWriter
    {
        public void WriteAccessCookie(HttpResponse res, string jwt, int minutes)
        {
            res.Cookies.Append("access_token", jwt, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddMinutes(minutes),
                Path = "/"
            });
        }

        public void WriteRefreshCookie(HttpResponse res, string token, int days)
        {
            res.Cookies.Append("refresh_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(days),
                Path = "/auth"
            });
        }

        public void ClearAuthCookies(HttpResponse res)
        {
            res.Cookies.Delete("access_token", new CookieOptions { Path = "/", Secure = true, SameSite = SameSiteMode.None });
            res.Cookies.Delete("refresh_token", new CookieOptions { Path = "/auth", Secure = true, SameSite = SameSiteMode.None });
        }
    }
}
