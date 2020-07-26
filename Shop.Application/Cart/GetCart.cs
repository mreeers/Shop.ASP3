using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shop.Database;
using Shop.Domain.Models;
using System.Linq;

namespace Shop.Application.Cart
{
    public class GetCart
    {
        private readonly ISession _session;
        private readonly ApplicationDbContext _context;

        public GetCart(ISession session, ApplicationDbContext context)
        {
            _session = session;
            _context = context;
        }

        public class Response
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public int StockId { get; set; }
            public int Qty { get; set; }
        }

        public Response Do()
        {
            var stringObject = _session.GetString("cart");

            var cartProduct = JsonConvert.DeserializeObject<CartProduct>(stringObject);

            var response = _context.Stocks
                .Include(x => x.Product)
                .Where(x => x.Id == cartProduct.StockId)
                .Select(x => new Response
                {
                    Name = x.Product.Name,
                    Value = $" {x.Product.Value.ToString("N2")}",
                    StockId = x.Id,
                    Qty = cartProduct.Qty
                })
                .FirstOrDefault();

            return new Response();
        }
    }
}
