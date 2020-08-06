using Shop.Database;
using Shop.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop.Application.Orders
{
    public class CreateOrder
    {
        private readonly ApplicationDbContext _context;

        public CreateOrder(ApplicationDbContext context)
        {
            _context = context;
        }

        public class Request
        {
            public string StripeReference { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string Address1 { get; set; }
            public string Address2 { get; set; }
            public string City { get; set; }
            public string PostCode { get; set; }
            public List<Stock> Stocks { get; set; }
        }

        public class Stock
        {
            public int StockId { get; set; }
            public int Qty { get; set; }
        }

        public async Task<bool> Do(Request request)
        {
            var stocksToUpdate = _context.Stocks
                .AsEnumerable()
                .Where(x => request.Stocks.Any(y => y.StockId == x.Id))
                .ToList();

            foreach(var stock in stocksToUpdate)
            {
                stock.Qty = stock.Qty - request.Stocks.FirstOrDefault(x => x.StockId == stock.Id).Qty;
            }

            var order = new Order
            {
                OrderRef = CreateOrderReference(),
                StripeReference = request.StripeReference,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PostCode = request.PostCode,
                City = request.City,
                Address1 = request.Address1,
                Address2 = request.Address2,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,

                OrderStocks = request.Stocks.Select(x => new OrderStock
                {
                    StockId = x.StockId,
                    Qty = x.Qty
                   
                }).ToList()
            };

            _context.Orders.Add(order);

            return await _context.SaveChangesAsync() > 0;
        }

        public string CreateOrderReference()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
            var result = new char[12];
            var random = new Random();

            do
            {
                for (int i = 0; i < result.Length; i++)
                    result[i] = chars[random.Next(chars.Length)];
            } while (_context.Orders.Any(x => x.OrderRef == new string(result)));
            
            return new string(result);

        }
    }
}
