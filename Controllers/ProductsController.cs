using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
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

        // PUT api/Products/Add/3/4
        [Route("Add/{_productId}/{_amount}")]
        [HttpPut]
        public ActionResult IncreaseProductCount(int _productId, int _amount) 
        {
            var product = _context.Product.Find(_productId);

            product.InStorage += _amount;

            _context.SaveChanges();

            return Ok();
        }

        [Route("{categoryId}")]
        [HttpGet]
        public ActionResult<IEnumerable<Product>> Get(int categoryId) 
        {
            var products = _context.Product.Where(product => product.CategoryId == categoryId);

            if (!products.Any()) 
            {
                return NotFound();
            }

            var result = products.ToList();

            return result;
        }

        // POST api/Products
        [HttpPost]
        public ActionResult Post(Product _newProduct)
        {
            if (_newProduct == null ||
                string.IsNullOrEmpty(_newProduct.ProductName) ||
                (_newProduct.Discount != null ? _newProduct.Discount <= 0 : false) ||
                _newProduct.Price <= 0) 
            {
                return BadRequest();
            }

            _context.Product.Add(new Product
                                {
                                    CategoryId = _newProduct.CategoryId,
                                    ProductName = _newProduct.ProductName,
                                    Description = _newProduct.Description,
                                    CreatedDate = DateTime.UtcNow,
                                    Discount = _newProduct.Discount,
                                    Price = _newProduct.Price,
                                    Image = _newProduct.Image,
                                    InStorage = _newProduct.InStorage
                                });
            _context.SaveChanges();

            return Ok();
        }

        // PUT api/Products
        [HttpPut]
        public ActionResult Put(Product _productData)
        {
            if (_productData == null ||
                string.IsNullOrEmpty(_productData.ProductName) ||
                (_productData.Discount != null ? _productData.Discount <= 0 : false) ||
                _productData.Price <= 0)
            {
                return BadRequest();
            }

            var product = _context.Product.Find(_productData.ProductId);

            if (product == null) 
            {
                return NotFound();
            }

            product.ProductName = _productData.ProductName;
            product.Description = _productData.Description;
            if(!string.IsNullOrEmpty(_productData.Image)) product.Image = _productData.Image;
            product.Price = _productData.Price;
            product.Discount = _productData.Discount;

            _context.SaveChanges();

            return Ok();
        }

        // DELETE api/Products/5
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
