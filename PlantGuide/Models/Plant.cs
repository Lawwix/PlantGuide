using System.ComponentModel.DataAnnotations;

namespace PlantGuide.Models
{
    public class Plant
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Display(Name = "Научное название")]
        public string ScientificName { get; set; }

        [Display(Name = "Краткое описание")]
        public string Description { get; set; }

        [Display(Name = "Инструкции по уходу")]
        public string CareInstructions { get; set; }

        [Display(Name = "Путь к фото")]
        // Если хотите, чтобы фото было опциональным — замените string на string? и удалите [Required]
        
        public string? PhotoPath { get; set; }
    }
}