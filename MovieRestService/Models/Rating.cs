using System.ComponentModel.DataAnnotations;

namespace MovieRestService.Models
{
    public class Rating
    {
        [Key]
        public int Id { get; set; }
        public string Source { get; set; }
        public string Value { get; set; }
    }
}
