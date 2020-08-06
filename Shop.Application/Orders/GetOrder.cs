﻿using Microsoft.EntityFrameworkCore;
using Shop.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shop.Application.Orders
{
    public class GetOrder
    {
        private readonly ApplicationDbContext _context;

        public GetOrder(ApplicationDbContext context)
        {
            _context = context;
        }

        public class Response
        {
            public string OrderRef { get; set; }
            public string StripeReference { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string Address1 { get; set; }
            public string Address2 { get; set; }
            public string City { get; set; }
            public string PostCode { get; set; }
            public IEnumerable<Product> Products { get; set; }
            public string TotalValue { get; set; }
        }

        public class Product
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Value { get; set; }
            public int Qty { get; set; }
            public string StockDescription { get; set; }
        }

        public Response Do(string reference) =>
           _context.Orders.Where(x => x.OrderRef == reference)
                .Include(x => x.OrderStocks)
                .ThenInclude(x => x.Stock)
                .ThenInclude(x => x.Product)
                .Select(x => new Response
                {
                    OrderRef = x.OrderRef,
                    StripeReference = x.StripeReference,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    PostCode = x.PostCode,
                    City = x.City,
                    Address1 = x.Address1,
                    Address2 = x.Address2,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    Products = x.OrderStocks.Select(y => new Product
                    {
                        Name = y.Stock.Product.Name,
                        Description = y.Stock.Product.Description,
                        Value = $"$ {y.Stock.Product.Value.ToString("№2")}",
                        Qty = y.Qty,
                        StockDescription = y.Stock.Description
                    }),
                    TotalValue = x.OrderStocks.Sum(y => y.Stock.Product.Value).ToString("№2")
                })
                .FirstOrDefault();
        
    }
}
