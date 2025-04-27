using Core.Interfaces;
using Domain.Models;
using Infrastructure.Interfaces;

namespace Core.Services
{
    public class ProductService : IProductService
    {

        private readonly IUnitOfWork _unitOfWork;
        public ProductService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            var products = await _unitOfWork.products.GetAllAsync();
            if(products is null || ! products.Any())
                throw new Exception("No products found");

            return products;
        }


        public async Task<Product> GetByIdAsync(int id)
        {
            var product = await _unitOfWork.products.GetByIdAsync(id);
            if (product is null)
                throw new Exception($"Product with id : {id} not found");
            return product;
        }
        public async Task AddAsync(Product model)
        {
            if(model is null)
                throw new Exception("Product model is null");

            await _unitOfWork.products.AddAsync(model);
            await _unitOfWork.CompleteAsync();
        }
        public async Task UpdateAsync(Product model)
        {
            var product = await _unitOfWork.products.GetByIdAsync(model.Id);
            if (product is null)
                throw new Exception($"Product with id : {model.Id} not found");

            //var newproduct = new Product
            //{
            //    Name = model.Name,
            //    CategoryId = model.CategoryId,
            //    Price = model.Price
            //};
            product.CategoryId = model.CategoryId;
            product.Price = model.Price;
            product.Name = model.Name;

            await _unitOfWork.CompleteAsync();

        }
        public async Task DeleteAsync(int id)
        {
            var product = await _unitOfWork.products.GetByIdAsync(id);
            if (product is null)
                throw new Exception($"Product with id : {id} not found");
            await _unitOfWork.products.DeleteAsync(product);
            await _unitOfWork.CompleteAsync();
        }
    }
}
