using System;
using System.ComponentModel.DataAnnotations;

namespace ShortlinkApi.Models
{
    public class Link
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        [Required] public string Code { get; set; } = default!;
        [Required] public string LongUrl { get; set; } = default!;
        public string? Title { get; set; }
        public string Tags { get; set; } = "[]";  // JSON (MVP)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string UserId { get; set; } = "demo";
    }
}
