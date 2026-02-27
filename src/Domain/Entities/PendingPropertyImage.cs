using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.src.Domain.Entities
{
    public class PendingPropertyImage
    {
        public int Id { get; set; }
        public int PendingPropertyId { get; set; }
        public string ImageUrl { get; set; }

        public PendingProperty PendingProperty { get; set; }

        public string Caption { get; set; } = "Default Caption";
    }
}
