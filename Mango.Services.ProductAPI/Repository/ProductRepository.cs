using AutoMapper;
using Mango.Services.ProductAPI.DbContexts;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ProductAPI.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public ProductRepository(ApplicationDbContext dbContext, IMapper mapper)
        {
            _db = dbContext;
            _mapper = mapper;
        }

        public async Task<ProductDto> CreateUpdateProduct(ProductDto productDto)
        {
            Product product = _mapper.Map<ProductDto, Product>(productDto);

            if (product.ProductId > 0)
                _db.Products.Update(product);
            else
                _db.Products.Add(product);

            await _db.SaveChangesAsync();

            return _mapper.Map<Product, ProductDto>(product);
        }

        public async Task<bool> DeleteProduct(int productId)
        {
            try
            {
                Product product = await _db.Products.FindAsync(productId);

                if (product == null)
                    return false;

                _db.Products.Remove(product);

                await _db.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<ProductDto> GetProductById(int productId)
        {
            Product product = await _db.Products.FindAsync(productId);
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<IEnumerable<ProductDto>> GetProducts()
        {
            IEnumerable<Product> productsList = await _db.Products.ToListAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(productsList);
        }
    }
}
