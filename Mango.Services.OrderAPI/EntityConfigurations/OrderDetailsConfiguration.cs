using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Mango.Services.OrderAPI.Models;

namespace Mango.Services.OrderAPI.EntityConfigurations
{
    public class OrderDetailsConfiguration : IEntityTypeConfiguration<OrderDetails>
    {
        public void Configure(EntityTypeBuilder<OrderDetails> builder)
        {
            builder.HasKey(k => k.OrderDetailsId);

            builder.HasOne(ch => ch.OrderHeader)
                .WithMany(cd => cd.OrderDetails)
                .HasForeignKey(cd => cd.OrderHeaderId)
                .IsRequired();
        }
    }
}
