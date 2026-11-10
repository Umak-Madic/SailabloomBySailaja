using System.ComponentModel.DataAnnotations;
using Sailabloom.Domain;

namespace Sailabloom.Web.Pages.Admin.Plants;

/// <summary>Shared form model for the admin Create/Edit plant pages.</summary>
public class PlantInput
{
    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(60)]
    public string Category { get; set; } = string.Empty;

    [Range(0, 100000)]
    public decimal Price { get; set; }

    [Range(0, 100000)]
    public int Stock { get; set; }

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Image URL")]
    public string ImageUrl { get; set; } = string.Empty;

    public Plant ToPlant(int id = 0) => new()
    {
        Id = id,
        Name = Name.Trim(),
        Category = Category.Trim(),
        Price = Price,
        Stock = Stock,
        Description = Description.Trim(),
        ImageUrl = ImageUrl.Trim()
    };

    public static PlantInput FromPlant(Plant plant) => new()
    {
        Name = plant.Name,
        Category = plant.Category,
        Price = plant.Price,
        Stock = plant.Stock,
        Description = plant.Description,
        ImageUrl = plant.ImageUrl
    };
}
