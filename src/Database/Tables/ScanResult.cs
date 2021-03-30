using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DetectorApi.Database.Tables
{
    [Table("ScanResults")]
    public class ScanResult
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

        [Column]
        [MaxLength(1024)]
        public string Url { get; set; }

        [Column] // Can be null
        public int? StatusCode { get; set; }

        [Column] // Can be null
        [MaxLength(32)]
        public string SslErrorCode { get; set; }

        [Column] // Can be null
        [MaxLength(128)]
        public string SslErrorMessage { get; set; }

        [Column] // Can be null
        [MaxLength(128)]
        public string ConnectingIp { get; set; }

        [Column] // Can be null
        public string ExceptionMessage { get; set; }

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
                statusCode = this.StatusCode,
                sslErrorCode = this.SslErrorCode,
                sslErrorMessage = this.SslErrorMessage,
                connectingIp = this.ConnectingIp,
                exceptionMessage = this.ExceptionMessage
            };
        }

        #endregion
    }
}