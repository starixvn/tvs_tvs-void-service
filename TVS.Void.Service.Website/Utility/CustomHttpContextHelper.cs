using ONEbook.UM.Helpers;

namespace TVS.Void.Service.Website.Utility
{
    public class CustomHttpContextHelper : IHttpContextHelper
    {
        public string GetSession(string key)
        {
            return string.Empty;
        }

        public string GetValueFromQueryString(string queryKey)
        {
            return string.Empty;
        }

        public void SetSession(string key, string value)
        {
            // TODO
        }
    }
}