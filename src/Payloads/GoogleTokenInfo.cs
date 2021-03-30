namespace DetectorApi.Payloads
{
    public class GoogleTokenInfo
    {
        /// <summary>
        /// User e-mail.
        /// </summary>
        public string email { get; set; }

        /// <summary>
        /// User full name.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// User picture URL.
        /// </summary>
        public string picture { get; set; }
    }
}