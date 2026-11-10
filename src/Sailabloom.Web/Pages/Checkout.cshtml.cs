using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sailabloom.Application;
using Sailabloom.Application.Services;
using Sailabloom.Domain;
using Sailabloom.Web.Services;

namespace Sailabloom.Web.Pages;

public class CheckoutModel : PageModel
{
    private readonly IOrderService _orders;
    private readonly CartService _cart;

    public CheckoutModel(IOrderService orders, CartService cart)
    {
        _orders = orders;
        _cart = cart;
    }

    public string ZelleNumber => OrderService.ZelleNumber;

    public List<CartItem> Items { get; private set; } = new();

    public decimal Subtotal => Items.Sum(i => i.Price * i.Quantity);

    [BindProperty]
    public CheckoutInput Input { get; set; } = new();

    public class CheckoutInput
    {
        [Required]
        [StringLength(120)]
        [Display(Name = "Full name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Address { get; set; }

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Card;

        public FulfillmentMethod Fulfillment { get; set; } = FulfillmentMethod.StandardShipping;

        /// <summary>
        /// Demo card number, used only in memory to derive brand + last 4.
        /// It is never saved anywhere.
        /// </summary>
        [Display(Name = "Card number (demo)")]
        public string? CardNumber { get; set; }
    }

    public IActionResult OnGet()
    {
        Items = _cart.GetItems();
        if (Items.Count == 0)
        {
            return RedirectToPage("/Cart");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Items = _cart.GetItems();
        if (Items.Count == 0)
        {
            return RedirectToPage("/Cart");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        string? cardBrand = null;
        string? cardLast4 = null;
        if (Input.PaymentMethod == PaymentMethod.Card)
        {
            var digits = new string((Input.CardNumber ?? string.Empty).Where(char.IsDigit).ToArray());
            if (digits.Length < 4)
            {
                ModelState.AddModelError("Input.CardNumber", "Please enter the demo card number.");
                return Page();
            }

            cardLast4 = digits[^4..];
            cardBrand = digits.StartsWith('4')
                ? "Visa"
                : digits.StartsWith('5')
                    ? "Mastercard"
                    : digits.StartsWith('3')
                        ? "American Express"
                        : "Card";
        }

        var request = new PlaceOrderRequest
        {
            CustomerName = Input.CustomerName,
            Email = Input.Email,
            Address = Input.Address,
            PaymentMethod = Input.PaymentMethod,
            Fulfillment = Input.Fulfillment,
            CardBrand = cardBrand,
            CardLast4 = cardLast4,
            Lines = Items.Select(i => new CartLineDto { PlantId = i.PlantId, Quantity = i.Quantity }).ToList()
        };

        var result = await _orders.PlaceOrderAsync(request);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Checkout failed. Please try again.");
            return Page();
        }

        _cart.Clear();
        return RedirectToPage("/OrderConfirmation", new { id = result.OrderId });
    }
}
