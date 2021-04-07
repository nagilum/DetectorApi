using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DetectorApi.Database.Tables
{
    [Table("Alerts")]
    public class Alert
    {
        #region ORM

        [Key]
        [Column]
        public long Id { get; set; }

        [Column]
        public DateTimeOffset Created { get; set; }

        [Column]
        public DateTimeOffset Updated { get; set; }

        [Column]
        public long ResourceId { get; set; }

        [Column] // Can be null
        public long? ScanResultId { get; set; }

        [Column]
        [MaxLength(16)]
        public string Type { get; set; }

        [Column]
        [MaxLength(1024)]
        public string Url { get; set; }

        [Column]
        [MaxLength(1024)]
        public string Message { get; set; }

        #endregion

        #region Instance functions

        /// <summary>
        /// Create object for API output.
        /// </summary>
        /// <returns>Object.</returns>
        public object CreateApiOutput()
        {
            return new
            {
                created = this.Created,
                updated = this.Updated,
                type = this.Type,
                url = this.Url,
                message = this.Message
            };
        }

        #endregion
    }
}
