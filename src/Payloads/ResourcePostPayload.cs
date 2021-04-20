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

        /// <summary>
        /// Set the resource active/disabled.
        /// </summary>
        public bool? Active { get; set; }
    }
}