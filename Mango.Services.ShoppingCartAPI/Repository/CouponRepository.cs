using Mango.Services.ShoppingCartAPI.Models.DTO;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Mango.Services.ShoppingCartAPI.Repository
{
    public class CouponRepository : ICouponRepository
    {
        private readonly HttpClient _httpClient;

        public CouponRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CouponDto> GetCouponAsync(string couponName, string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync($"/api/coupon/{couponName}");
            
            var apiContent = await response.Content.ReadAsStringAsync();

            var resp = JsonSerializer.Deserialize<ResponseDto>(apiContent);

            if (resp.IsSuccess)
            {
                return JsonSerializer.Deserialize<CouponDto>(Convert.ToString(resp.Result));
            }

            return new CouponDto();
        }        
    }
}
