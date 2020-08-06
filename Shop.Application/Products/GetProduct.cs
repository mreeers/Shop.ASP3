using Microsoft.EntityFrameworkCore;
using Shop.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop.Application.Products
{
    public class GetProduct
    {
        private readonly ApplicationDbContext _context;

        public GetProduct(ApplicationDbContext context)
        {
            _context = context;

        }

        public async Task<ProductViewModel> Do(string name)
        {
            var stockOnHold = _context.StocksOnHolds.Where(x => x.ExpiryDate < DateTime.Now).ToList();

            if(stockOnHold.Count > 0)
            {
                var stockToReturn = _context.Stocks.Where(x => stockOnHold.Any(y => y.StockId == x.Id)).ToList();

                foreach(var stock in stockToReturn)
                {
                    stock.Qty = stock.Qty - stockOnHold.FirstOrDefault(x => x.StockId == stock.Id).Qty;
                }

                _context.StocksOnHolds.RemoveRange(stockOnHold);

                await _context.SaveChangesAsync();
            }

            return _context.Products
                .Include(x => x.Stocks)
                .Where(x => x.Name == name)
                .Select(x => new ProductViewModel
                {
                    Name = x.Name,
                    Description = x.Description,
                    Value = $"$ {x.Value.ToString("N2")}", //1100.50 => 1,100.50

                Stock = x.Stocks.Select(y => new StockViewModel
                    {
                        Id = y.Id,
                        Description = y.Description,
                        InStock = y.Qty > 0
                    })
                })
                .FirstOrDefault();
        }

        public class ProductViewModel
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Value { get; set; }
            public IEnumerable<StockViewModel> Stock { get; set; }
        }

        public class StockViewModel
        {
            public int Id { get; set; }
            public string Description { get; set; }
            public bool InStock { get; set; }
        }
    }
}
