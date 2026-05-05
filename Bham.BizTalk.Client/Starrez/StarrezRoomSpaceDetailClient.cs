using System.Net.Http;
using System.Threading.Tasks;

namespace Bham.BizTalk.Client.Starrez
{
    public class StarrezRoomSpaceDetailClient
    {
        private readonly HttpClient _httpClient;
        public StarrezRoomSpaceDetailClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetRoomSpaceDetailAsync(string baseUrl, int roomSpaceId, string username, string password)
        {
            var url = $"{baseUrl}RoomSpaceDetail.xml/?RoomSpaceID={roomSpaceId}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var byteArray = System.Text.Encoding.ASCII.GetBytes($"{username}:{password}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", System.Convert.ToBase64String(byteArray));
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
