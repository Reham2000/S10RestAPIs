using Core.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/V1.0/[controller]")] // "api/V1.0/products/"  // url
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }


        [HttpGet("GetAll")] // api/V1.0/products/GetAll
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }

        [HttpGet("getById/{id}")] // api/V1.0/products/getById/{id}
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if(product is null)
                    return NotFound(new { statusCode = 404, message = $"Product with id : {id} not found" });
                return Ok(new { statusCode = 200, message= "done!" , data= product });
            }
            catch (Exception ex)
            {
                return BadRequest(new {statusCode = 400 ,message = ex.Message });
            }
        }
        [HttpPost("Add")]  // api/V1.0/products/Add
        public async Task<IActionResult> Add( Product model)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    await _productService.AddAsync(model);
                    return Ok(new { statusCode = 200, message = "Product added successfully!" });
                }
                return BadRequest(new { statusCode = 400, message = "Data is not valid!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { statusCode = 400, message = ex.Message });
            }
        }

        [HttpPut("Edit")]   // api/V1.0/products/Edit
        public async Task<IActionResult> Update(Product model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _productService.UpdateAsync(model);
                    return Ok(new { statusCode = 200, message = "Product updated successfully!" });
                }
                return BadRequest(new { statusCode = 400, message = "Data is not valid!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { statusCode = 400, message = ex.Message });
            }
        }


        [HttpDelete("Delete/{id}")]   // api/V1.0/products/Delete/{id}
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if(_productService.GetByIdAsync(id) is null)
                    return NotFound(new { statusCode = 404, message = $"Product with id : {id} not found" });
                await _productService.DeleteAsync(id);
                return Ok(new { statusCode = 200, message = "Product deleted successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { statusCode = 400, message = ex.Message });
            }
        }




    }
}
