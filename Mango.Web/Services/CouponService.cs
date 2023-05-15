using Mango.Web.Models;
using Mango.Web.Services.IServices;

namespace Mango.Web.Services
{
    public class CouponService : BaseService, ICouponService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CouponService(IHttpClientFactory htppClientFactory) : base(htppClientFactory)
        {
            _httpClientFactory = htppClientFactory;
        }        

        public async Task<T> GetCouponAsync<T>(string couponCode, string token = null)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = StaticDetails.ApiType.GET,
                Url = StaticDetails.CouponAPIBase + "/api/coupon/" + couponCode,
                AccessToken = token
            });
        }
    }
}
