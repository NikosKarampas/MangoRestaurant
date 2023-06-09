﻿using AutoMapper;
using Mango.Services.ShoppingCartAPI.DbContexts;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ShoppingCartAPI.Repository
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public CartRepository(ApplicationDbContext dbContext, IMapper mapper)
        {
            _db = dbContext;
            _mapper = mapper;
        }        

        public async Task<bool> ClearCart(string userId)
        {
            var cartHeaderFromDb = await _db.CartHeaders.FirstOrDefaultAsync(ch => ch.UserId == userId);

            if (cartHeaderFromDb != null)
            {
                _db.CartDetails
                    .RemoveRange(_db.CartDetails.Where(cd => cd.CartHeaderId == cartHeaderFromDb.CartHeaderId));
                _db.CartHeaders.Remove(cartHeaderFromDb);
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<CartDto> CreateUpdateCart(CartDto cartDto)
        {
            try
            {
                Cart cart = _mapper.Map<Cart>(cartDto);

                //check if product exists in database, if not create it!
                var prodInDb = await _db.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.ProductId == cartDto.CartDetails.FirstOrDefault()
                    .ProductId);

                if (prodInDb == null)
                {
                    //Add product in database
                    _db.Products.Add(cart.CartDetails.FirstOrDefault().Product);
                    await _db.SaveChangesAsync();
                }

                //check if header is null
                var cartHeaderFromDb = await _db.CartHeaders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ch => ch.UserId == cart.CartHeader.UserId);

                if (cartHeaderFromDb == null)
                {
                    //create header and details
                    _db.CartHeaders.Add(cart.CartHeader);
                    await _db.SaveChangesAsync();

                    //Create cartdetails with CartHeaderId from cartheader we just created
                    cart.CartDetails.FirstOrDefault().CartHeaderId = cart.CartHeader.CartHeaderId;
                    cart.CartDetails.FirstOrDefault().Product = null;
                    _db.CartDetails.Add(cart.CartDetails.FirstOrDefault());
                    await _db.SaveChangesAsync();
                }
                else
                {
                    //if header is not null
                    //check if details has same product
                    var cartDetailsFromDb = await _db.CartDetails.FirstOrDefaultAsync(
                        cd => cd.ProductId == cart.CartDetails.FirstOrDefault().ProductId &&
                              cd.CartHeaderId == cartHeaderFromDb.CartHeaderId);

                    if (cartDetailsFromDb == null)
                    {
                        //create cart details / add a new product
                        cart.CartDetails.FirstOrDefault().CartHeaderId = cartHeaderFromDb.CartHeaderId;
                        cart.CartDetails.FirstOrDefault().Product = null;
                        _db.CartDetails.Add(cart.CartDetails.FirstOrDefault());
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        //update the count / cart details of the product                        
                        cartDetailsFromDb.Count += cart.CartDetails.FirstOrDefault().Count;
                        _db.CartDetails.Update(cartDetailsFromDb);
                        await _db.SaveChangesAsync();
                    }
                }

                return _mapper.Map<CartDto>(cart);
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating cart", ex);
            }            
        }

        public async Task<CartDto> GetCartByUserId(string userId)
        {
            Cart cart = new()
            {
                CartHeader = await _db.CartHeaders.FirstOrDefaultAsync(ch => ch.UserId == userId)
            };

            if (cart.CartHeader != null)
            {
                cart.CartDetails = _db.CartDetails
                    .Where(cd => cd.CartHeaderId == cart.CartHeader.CartHeaderId).Include(x => x.Product);
            }            

            return _mapper.Map<CartDto>(cart);
        }        

        public async Task<bool> RemoveFromCart(int cartDetailsId)
        {
            try
            {
                CartDetails cartDetailsItem = await _db.CartDetails
                    .FirstOrDefaultAsync(cd => cd.CartDetailsId == cartDetailsId);

                int totalCountOfCartItems = _db.CartDetails
                    .Where(cd => cd.CartHeaderId == cartDetailsItem.CartHeaderId).Count();

                _db.CartDetails.Remove(cartDetailsItem);

                //If it was the last item remove the cart header as well
                if (totalCountOfCartItems == 1)
                {
                    var cartHeaderToRemove = await _db.CartHeaders
                        .FirstOrDefaultAsync(ch => ch.CartHeaderId == cartDetailsItem.CartHeaderId);

                    _db.CartHeaders.Remove(cartHeaderToRemove);
                }

                await _db.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;                
            }
                               
        }

        public async Task<bool> ApplyCoupon(string userId, string couponCode)
        {
            var cartFromDb = await _db.CartHeaders.FirstOrDefaultAsync(ch => ch.UserId == userId);
            cartFromDb.CouponCode = couponCode;
            _db.CartHeaders.Update(cartFromDb);
            await _db.SaveChangesAsync(); 
            return true;
        }

        public async Task<bool> RemoveCoupon(string userId)
        {
            var cartFromDb = await _db.CartHeaders.FirstOrDefaultAsync(ch => ch.UserId == userId);
            cartFromDb.CouponCode = string.Empty;
            _db.CartHeaders.Update(cartFromDb);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
