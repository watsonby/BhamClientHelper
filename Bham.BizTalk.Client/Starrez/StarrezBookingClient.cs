using System.Net.Http;
using System.Threading.Tasks;

namespace Bham.BizTalk.Client.Starrez
{
    public class StarrezBookingClient
    {
        private readonly HttpClient _httpClient;
        public StarrezBookingClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetBookingAsync(string baseUrl, int bookingId, string username, string password)
        {
            var url = $"{baseUrl}Booking.xml/?BookingID={bookingId}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var byteArray = System.Text.Encoding.ASCII.GetBytes($"{username}:{password}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", System.Convert.ToBase64String(byteArray));
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
