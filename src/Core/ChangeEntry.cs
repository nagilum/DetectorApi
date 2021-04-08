namespace DetectorApi.Core
{
    public class ChangeEntry
    {
        /// <summary>
        /// Which property has changed.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// From which value did it change.
        /// </summary>
        public string OldValue { get; set; }

        /// <summary>
        /// To which value did it change.
        /// </summary>
        public string NewValue { get; set; }

        #region Overrides

        public override string ToString()
        {
            return $"'{this.PropertyName}' changed from '{this.OldValue}' to '{this.NewValue}'";
        }

        #endregion
    }
}