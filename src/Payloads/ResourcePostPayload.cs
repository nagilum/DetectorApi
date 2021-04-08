namespace DetectorApi.Payloads
{
    public class ResourcePostPayload
    {
        /// <summary>
        /// Resource name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Resource URL.
        /// </summary>
        public string Url { get; set; }
    }
}