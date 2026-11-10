using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sailabloom.Application.Services;
using Sailabloom.Domain;

namespace Sailabloom.Web.Pages;

public class GuaranteeModel : PageModel
{
    private readonly IOrderService _orders;
    private readonly IWebHostEnvironment _environment;

    public GuaranteeModel(IOrderService orders, IWebHostEnvironment environment)
    {
        _orders = orders;
        _environment = environment;
    }

    public bool Submitted { get; private set; }

    [BindProperty]
    public ReportInput Input { get; set; } = new();

    public class ReportInput
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Enter your order number.")]
        [Display(Name = "Order number")]
        public int OrderId { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Checkout email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "What happened?")]
        public IssueType IssueType { get; set; } = IssueType.ArrivedDead;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        public IFormFile? Photo { get; set; }
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        string? photoFileName = null;
        if (Input.Photo is not null && Input.Photo.Length > 0)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "reports");
            Directory.CreateDirectory(uploadsFolder);

            photoFileName = Guid.NewGuid().ToString("N") + Path.GetExtension(Input.Photo.FileName);
            var filePath = Path.Combine(uploadsFolder, photoFileName);
            await using var stream = new FileStream(filePath, FileMode.Create);
            await Input.Photo.CopyToAsync(stream);
        }

        var result = await _orders.SubmitIssueReportAsync(
            Input.OrderId, Input.Email, Input.IssueType, Input.Description, photoFileName);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "We couldn't submit that report. Please try again.");
            return Page();
        }

        Submitted = true;
        return Page();
    }
}
