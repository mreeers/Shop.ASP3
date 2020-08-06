using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Shop.Application.Cart;
using Shop.Application.Orders;
using Shop.Database;
using Stripe;

namespace Shop.UI.Pages.Checkout
{
    public class PaymentModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PaymentModel(IConfiguration config, ApplicationDbContext context)
        {
            PublicKey = config["Stripe:PublicKey"].ToString();
            _context = context;
        }

        public string PublicKey { get; }

        public IActionResult OnGet()
        {
            var information = new GetCustomerInformation(HttpContext.Session).Do();

            if (information == null)
                return RedirectToPage("/Checkout/CustomerInformation");

            return Page();
        }

        public async Task<IActionResult> OnPost(string stripeEmail, string stripeToken)
        {
            var customers = new CustomerService();
            var charges = new ChargeService();

            var CardOrder = new Application.Cart.GetOrder(HttpContext.Session, _context).Do();

            var customer = customers.Create(new CustomerCreateOptions
            {
                Email = stripeEmail,
                Source = stripeToken
            });

            var charge = charges.Create(new ChargeCreateOptions
            {
                Amount = CardOrder.GetTotalCharge(),
                Description = "Shop Purchase",
                Currency = "gbp",
                Customer = customer.Id
            });

            //create order 
            await new CreateOrder(_context).Do(new CreateOrder.Request
            { 
                StripeReference = charge.Id,
                FirstName = CardOrder.CustomerInformation.FirstName, 
                LastName = CardOrder.CustomerInformation.LastName,
                PostCode = CardOrder.CustomerInformation.PostCode,
                City = CardOrder.CustomerInformation.City,
                Address1 = CardOrder.CustomerInformation.Address1,
                Address2 = CardOrder.CustomerInformation.Address2,
                Email = CardOrder.CustomerInformation.Email, 
                PhoneNumber = CardOrder.CustomerInformation.PhoneNumber,

                Stocks = CardOrder.Products.Select(x => new CreateOrder.Stock
                {
                    StockId = x.StockId,
                    Qty = x.Qty
                }).ToList()
            });

            return RedirectToPage("/Index");
        }
    }
}