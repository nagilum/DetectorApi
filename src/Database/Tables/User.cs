using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DetectorApi.Database.Tables
{
    [Table("Users")]
    public class User
    {
        #region ORM

        [Key]
        [Column]
        [JsonIgnore]
        public long Id { get; set; }

        [Column]
        [JsonIgnore]
        public DateTimeOffset Created { get; set; }

        [Column]
        [JsonIgnore]
        public DateTimeOffset Updated { get; set; }

        [Column]
        [MaxLength(128)]
        public string Email { get; set; }

        [Column]
        [MaxLength(1024)]
        public string Name { get; set; }

        [Column]
        [MaxLength(1024)]
        public string PictureUrl { get; set; }

        #endregion

        #region Instance functions

        /// <summary>
        /// Get the token used for comparison.
        /// </summary>
        /// <returns>Token data.</returns>
        public string CompileTokenContent()
        {
            return $"{this.Id}" +
                   $"{this.Created:yyyy-MM-dd HH:mm:ss}" +
                   $"{this.Email}";
        }

        /// <summary>
        /// Verify the given token vs the saved one.
        /// </summary>
        /// <param name="token">Given token.</param>
        /// <returns>Success.</returns>
        public bool VerifyToken(string token)
        {
            return BCrypt.Net.BCrypt.Verify(
                this.CompileTokenContent(),
                token);
        }

        #endregion
    }
}