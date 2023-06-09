﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Mango.Services.ShoppingCartAPI.Models;

namespace Mango.Services.ShoppingCartAPI.EntityConfigurations
{
    public class CartDetailsConfiguration : IEntityTypeConfiguration<CartDetails>
    {
        public void Configure(EntityTypeBuilder<CartDetails> builder)
        {
            builder.HasKey(k => k.CartDetailsId);

            builder.HasOne(ch => ch.CartHeader)
                .WithOne(cd => cd.CartDetails)
                .HasForeignKey<CartDetails>(cd => cd.CartHeaderId);

            builder.HasOne(ch => ch.Product)
                .WithOne(cd => cd.CartDetails)
                .HasForeignKey<CartDetails>(cd => cd.ProductId);

            builder.HasIndex(cd => cd.CartHeaderId).IsUnique(false);
            builder.HasIndex(cd => cd.ProductId).IsUnique(false);
        }
    }
}
