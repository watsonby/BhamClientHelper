using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Bham.BizTalk.Client.Starrez
{
    public class StarrezUpdateBookingClient
    {
        private readonly HttpClient _httpClient;
        public StarrezUpdateBookingClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> UpdateBookingAsync(string baseUrl, int bookingId, string xmlBody, string username, string password)
        {
            var url = $"{baseUrl}Booking/{bookingId}";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            var byteArray = System.Text.Encoding.ASCII.GetBytes($"{username}:{password}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", System.Convert.ToBase64String(byteArray));
            request.Content = new StringContent(xmlBody, Encoding.UTF8, "application/xml");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
