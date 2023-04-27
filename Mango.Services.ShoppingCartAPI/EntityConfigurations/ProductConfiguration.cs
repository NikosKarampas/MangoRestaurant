using Mango.Services.ShoppingCartAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mango.Services.ShoppingCartAPI.EntityConfigurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.Property(t => t.Name).IsRequired();
            builder.Property(t => t.ProductId).ValueGeneratedNever();
        }
    }
}
