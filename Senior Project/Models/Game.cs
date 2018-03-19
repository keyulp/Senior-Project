using System.ComponentModel.DataAnnotations;

namespace Senior_Project.Models
{
    public class Game
    {
        [Required]
        [Display(Name = "Game Title")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Game Company")]
        public string Company { get; set; }

        [Required]
        [Display(Name = "Consoles this Game is on")]
        public string Consoles { get; set; }

        public int UserId { get; set; }
    }
}