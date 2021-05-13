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

        /// <summary>
        /// Возвращает подкатегории указанной родительской категории
        /// </summary>
        /// <param name="parentId">Опциональный id родительской категории</param>
        // GET: api/Categories/3
        [Route("{parentId:int?}")]
        [HttpGet]
        public ActionResult<IEnumerable<Category>> Get(int? parentId = null) 
        {
            var categories = _context.Category.Include(cat => cat.ChildCategories)
                                                .Include(cat => cat.Products)
                                                .Where(cat => cat.ParentCategoryId == parentId);

            if (!categories.Any()) 
            {
                return NotFound();
            }

            var result = categories.ToList();

            return result;
        }

        [HttpPost]
        public ActionResult Post(Category _newCategory) 
        {
            if (_newCategory == null || 
                string.IsNullOrEmpty(_newCategory.CategoryName)) 
            {
                return BadRequest();
            }

            _context.Category.Add(new Category { CategoryName = _newCategory.CategoryName, 
                                                    Image = _newCategory.Image, 
                                                    ParentCategoryId = _newCategory.ParentCategoryId });
            _context.SaveChanges();

            return Ok();
        }

        [HttpPut]
        public ActionResult Put(Category _categoryData) 
        {
            if (_categoryData == null ||
                string.IsNullOrEmpty(_categoryData.CategoryName)) 
            {
                return BadRequest();
            }

            var category = _context.Category.Find(_categoryData.CategoryId);

            if (category == null) 
            {
                return NotFound();
            }

            category.CategoryName = _categoryData.CategoryName;
            if (!string.IsNullOrEmpty(_categoryData.Image)) category.Image = _categoryData.Image;

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

        //private void RecursiveLazyLoad(Category parentEntity) 
        //{
        //    _context.Entry(parentEntity).Collection(cat => cat.ChildCategories).Load();

        //    if (!parentEntity.IsEndpoint) 
        //    {
        //        foreach (var childCategory in parentEntity.ChildCategories) 
        //        {
        //            RecursiveLazyLoad(childCategory);
        //        }

        //        _context.Entry(parentEntity).Collection(cat => cat.Products).Load();
        //    }
        //}

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
