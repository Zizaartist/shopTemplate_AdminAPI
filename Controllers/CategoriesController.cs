using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShopAdminAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopAdminAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ShopContext _context;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ShopContext _context, ILogger<CategoriesController> _logger)
        {
            this._context = _context;
            this._logger = _logger;
        }

        //Получаем все древо категорий сразу
        [HttpGet]
        public ActionResult<IEnumerable<Category>> Get() 
        {
            var categories = _context.Category.Where(cat => cat.ParentCategoryId == null);

            foreach (var category in categories) 
            {
                RecursiveLazyLoad(category);
            }

            return categories.ToList();
        }

        [HttpPost]
        public ActionResult Post(Category _newCategory) 
        {
            _context.Category.Add(_newCategory);
            _context.SaveChanges();

            return Ok();
        }

        [HttpPut]
        public ActionResult Put(Category _categoryData) 
        {
            var category = _context.Category.Find(_categoryData.CategoryId);

            category.CategoryName = _categoryData.CategoryName;
            category.Image = _categoryData.Image;

            _context.SaveChanges();

            return Ok();
        }

        [HttpDelete("{id}")]
        public ActionResult Remove(int id)
        {
            var category = _context.Category.Include(cat => cat.ChildCategories)
                                            .Include(cat => cat.Products)
                                            .FirstOrDefault(cat => cat.CategoryId == id);

            RecursiveRemove(category);

            _context.SaveChanges();

            return Ok();
        }

        private void RecursiveLazyLoad(Category parentEntity) 
        {
            _context.Entry(parentEntity).Collection(cat => cat.ChildCategories).Load();

            if (!parentEntity.IsEndpoint) 
            {
                foreach (var childCategory in parentEntity.ChildCategories) 
                {
                    RecursiveLazyLoad(childCategory);
                }

                _context.Entry(parentEntity).Collection(cat => cat.Products).Load();
            }
        }

        private void RecursiveRemove(Category parentEntity)
        {
            if (parentEntity.ChildCategories != null && parentEntity.ChildCategories.Any())
            {
                var children = _context.Category.Include(cat => cat.ChildCategories)
                                                .Include(cat => cat.Products)
                                                .Where(cat => cat.ParentCategoryId == parentEntity.CategoryId);

                foreach (var childCategory in children)
                {
                    RecursiveRemove(childCategory);
                }
            }

            _context.Category.Remove(parentEntity);
        }
    }
}
