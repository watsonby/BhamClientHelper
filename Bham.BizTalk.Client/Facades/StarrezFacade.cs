using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using NLog;
using Bham.BizTalk.Client.Starrez;

namespace Bham.BizTalk.Client.Facades
{
    public class StarrezFacade
    {
        private static readonly ILogger Logger = LogManager.GetLogger("StarrezFacade");

        private readonly StarrezEntryClient _entryClient;
        private readonly StarrezBookingClient _bookingClient;
        private readonly StarrezRoomSpaceDetailClient _roomSpaceDetailClient;
        private readonly StarrezUpdateBookingClient _updateBookingClient;

        private readonly string _baseUrl;
        private readonly string _username;
        private readonly string _password;
        public StarrezFacade(HttpClient httpClient, string baseUrl, string username, string password)
        {
            _entryClient = new StarrezEntryClient(httpClient);
            _bookingClient = new StarrezBookingClient(httpClient);
            _roomSpaceDetailClient = new StarrezRoomSpaceDetailClient(httpClient);
            _updateBookingClient = new StarrezUpdateBookingClient(httpClient);
            _baseUrl = baseUrl;
            _username = username;
            _password = password;
        }

        /// <summary>
        /// Logs an orchestration transcript or any string to NLog (Info level).
        /// Call this from BizTalk Expression Shapes.
        /// </summary>
        public static void LogTranscript(string transcript)
        {
            Logger.Info("Orchestration Transcript: {0}", transcript);
        }

        public async Task<string> ProcessStudentEntryAsync(int entryId)
        {
            Logger.Info($"Starting student entry process for EntryID: {entryId}");
            // 1. Call Entry API
            var entryResponse = await _entryClient.GetEntryAsync(_baseUrl, entryId, _username, _password);
            Logger.Debug($"Entry API response: {entryResponse}");
            int bookingId = ExtractBookingId(entryResponse);
            Logger.Info($"Extracted BookingID: {bookingId}");

            // 2. Call Booking API
            var bookingResponse = await _bookingClient.GetBookingAsync(_baseUrl, bookingId, _username, _password);
            Logger.Debug($"Booking API response: {bookingResponse}");
            int roomSpaceId = ExtractRoomSpaceId(bookingResponse);
            Logger.Info($"Extracted RoomSpaceID: {roomSpaceId}");

            // 3. Call RoomSpaceDetail API
            var roomSpaceDetailResponse = await _roomSpaceDetailClient.GetRoomSpaceDetailAsync(_baseUrl, roomSpaceId, _username, _password);
            Logger.Debug($"RoomSpaceDetail API response: {roomSpaceDetailResponse}");

            // 4. Update Booking if needed (XML parsing and logic needed)
            // string xmlBody = ...
            // var updateResponse = await _updateBookingClient.UpdateBookingAsync(_baseUrl, bookingId, xmlBody, _username, _password);

            Logger.Info($"Completed student entry process for EntryID: {entryId}");
            return roomSpaceDetailResponse;
        }

        private int ExtractBookingId(string entryXml)
        {
            if (string.IsNullOrWhiteSpace(entryXml)) return 0;
            try
            {
                var doc = XDocument.Parse(entryXml);
                var bookingIdElement = doc.Descendants("BookingID").FirstOrDefault();
                if (bookingIdElement != null && int.TryParse(bookingIdElement.Value, out int bookingId))
                {
                    Logger.Info($"Parsed BookingID: {bookingId}");
                    return bookingId;
                }
                else
                {
                    Logger.Warn("BookingID element not found or invalid in Entry XML.");
                }
            }
            catch (System.Exception ex)
            {
                Logger.Error(ex, "Error parsing BookingID from Entry XML.");
            }
            return 0;
        }

        private int ExtractRoomSpaceId(string bookingXml)
        {
            if (string.IsNullOrWhiteSpace(bookingXml)) return 0;
            try
            {
                var doc = XDocument.Parse(bookingXml);
                var roomSpaceIdElement = doc.Descendants("RoomSpaceID").FirstOrDefault();
                if (roomSpaceIdElement != null && int.TryParse(roomSpaceIdElement.Value, out int roomSpaceId))
                {
                    Logger.Info($"Parsed RoomSpaceID: {roomSpaceId}");
                    return roomSpaceId;
                }
                else
                {
                    Logger.Warn("RoomSpaceID element not found or invalid in Booking XML.");
                }
            }
            catch (System.Exception ex)
            {
                Logger.Error(ex, "Error parsing RoomSpaceID from Booking XML.");
            }
            return 0;
        }
    }
}
