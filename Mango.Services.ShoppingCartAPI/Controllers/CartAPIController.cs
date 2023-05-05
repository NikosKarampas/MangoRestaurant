using Mango.Services.ShoppingCartAPI.Models.DTO;
using Mango.Services.ShoppingCartAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartAPIController : ControllerBase
    {
        private readonly ICartRepository _cartRepository;
        protected ResponseDto _response;

        public CartAPIController(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
            _response = new ResponseDto();
        }

        [Authorize]
        [HttpGet("GetCart/{userId}")]
        public async Task<ActionResult<object>> GetCart([FromRoute] string userId)
        {
            try
            {
                CartDto cartDto = await _cartRepository.GetCartByUserId(userId);
                _response.Result = cartDto;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [Authorize]
        [HttpPost("AddCart")]
        public async Task<ActionResult<object>> AddCart([FromBody] CartDto cartDto)
        {
            try
            {
                CartDto cartDt = await _cartRepository.CreateUpdateCart(cartDto);
                _response.Result = cartDt;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [Authorize]
        [HttpPost("UpdateCart")]
        public async Task<ActionResult<object>> UpdateCart([FromBody] CartDto cartDto)
        {
            try
            {
                CartDto cartDt = await _cartRepository.CreateUpdateCart(cartDto);
                _response.Result = cartDt;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [Authorize]
        [HttpPost("RemoveCart")]
        public async Task<ActionResult<object>> RemoveCart([FromBody] int cartId)
        {
            try
            {
                bool isSuccess = await _cartRepository.RemoveFromCart(cartId);
                _response.Result = isSuccess;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [Authorize]
        [HttpDelete("ClearCart/{userId}")]
        public async Task<ActionResult<object>> ClearCart([FromRoute] string userId)
        {
            try
            {
                bool isSuccess = await _cartRepository.ClearCart(userId);
                _response.Result = isSuccess;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [Authorize]
        [HttpPost("ApplyCoupon")]
        public async Task<ActionResult<object>> ApplyCoupon([FromBody] CartDto cartDto)
        {
            try
            {
                bool isSuccess = await _cartRepository.ApplyCoupon(cartDto.CartHeader.UserId, 
                    cartDto.CartHeader.CouponCode);
                _response.Result = isSuccess;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }


        [Authorize]
        [HttpPost("RemoveCoupon")]
        public async Task<ActionResult<object>> RemoveCoupon([FromBody] string userId)
        {
            try
            {
                bool isSuccess = await _cartRepository.RemoveCoupon(userId);
                _response.Result = isSuccess;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }
    }
}
