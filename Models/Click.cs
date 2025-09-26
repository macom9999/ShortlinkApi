using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShortlinkApi.Models
{
    public class Click
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();

        [Required] public Guid LinkId { get; set; }
        [ForeignKey(nameof(LinkId))] public Link? Link { get; set; }

        public DateTime Ts { get; set; } = DateTime.UtcNow;
        public string? Country { get; set; }
        public string? Region { get; set; }
        public string? Referer { get; set; }
        public string? Ua { get; set; }
        public string? Device { get; set; }
        public string? IpHash { get; set; }
    }
}
