namespace AWSUtilities
{
    public enum EnvType
    {
        Dev = 0,
        Prod
    }

    public class StringUtil
    {
        public static string EnvTypeToString(EnvType envType)
        {
            string envTypeString = string.Empty;
            switch (envType)
            {
                case EnvType.Dev:
                    envTypeString = "dev";
                    break;
                case EnvType.Prod:
                    envTypeString = "prod";
                    break;
            }
            return envTypeString;
        }

        /// <summary>
        /// Base64-encodes the specified bytes, and then replaces
        /// <c> +, =, / </c> with <c> -, _, ~ </c> respectively,
        /// thus making the returned encoded string safe to use as
        /// a URL query argument.
        /// </summary>
        public static string ToUrlSafeBase64String(byte[] bytes)
        {
            return System.Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('=', '_')
                .Replace('/', '~');
        }
    }
}
