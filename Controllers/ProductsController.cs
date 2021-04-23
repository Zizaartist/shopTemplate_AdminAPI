using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShopAdminAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ShopAdminAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ShopContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ShopContext _context, ILogger<ProductsController> _logger)
        {
            this._context = _context;
            this._logger = _logger;
        }

        // POST api/<ProductsController>
        [HttpPost]
        public ActionResult Post(Product _newProduct)
        {
            _context.Product.Add(_newProduct);
            _context.SaveChanges();

            return Ok();
        }

        // PUT api/<ProductsController>/5
        [HttpPut("{id}")]
        public ActionResult Put(int id, Product _productData)
        {
            var product = _context.Product.Find(id);

            product.ProductName = _productData.ProductName;
            product.Description = _productData.Description;
            product.Image = _productData.Image;
            product.Price = _productData.Price;
            product.Discount = _productData.Discount;

            _context.SaveChanges();

            return Ok();
        }

        // DELETE api/<ProductsController>/5
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var productToDelete = _context.Product.Find(id);
            _context.Product.Remove(productToDelete);
            _context.SaveChanges();

            return Ok();
        }
    }
}
