using System;
using System.ComponentModel.DataAnnotations;

namespace Senior_Project.Models
{
    public class Anime
    {
        [Required]
        [Display(Name = "English Title")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "# of Episodes")]
        public int Episodes { get; set; }

        [Required]
        [Display(Name = "Airing Status")]
        public string Status { get; set; }

        public int UserId { get; set; }
    }
}