using System.Net.Http.Json;
using ConsoleApp.Dto;
using ConsoleApp.Settings;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace ConsoleApp.Services
{
    public class ChatClient(IHttpClientFactory httpClientFactory, IOptions<ChatClientSetting> settings)
        : IChatClient
    {
        private const string JsonPayloadCouldNotBeDeserialized = "The API was successful but the JSON payload could not be deserialized";
        private const string ProblemDetailsCouldNotBeDeserialized = "An unknown error has occured. It's not possible to deserialize the Problem Details from the Server response.";
        private const string LocalException = "An unknown error has occurred. Server Error Details are not available at this time. Local Exception : {0}";

        private readonly ChatClientSetting _settings = settings.Value;

        private HubConnection? _hubConnection;


        public async Task<UserResponseDto> Send(UserRequestDto userRequest)
        {
            var url = $"{_settings.BaseUrl}{_settings.SendUrl}";

            try
            {
                var client = httpClientFactory.CreateClient();

                var response = await client.PostAsJsonAsync(url, userRequest);

                if (response.IsSuccessStatusCode)
                {
                    var userResponse = await response.Content.ReadFromJsonAsync<UserResponseDto>();

                    return userResponse ?? CreateUserResponse(JsonPayloadCouldNotBeDeserialized, userRequest, true);
                }

                var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>();

                return CreateUserResponse(problemDetails == null ? ProblemDetailsCouldNotBeDeserialized : problemDetails.Detail, userRequest, true);
            }
            catch (Exception exception)
            {
                return CreateUserResponse(string.Format(LocalException, exception.Message), userRequest, true);
            }
        }

        public async Task InitializeStreamingConnectionAsync()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_settings.BaseUrl + _settings.HubUrl)
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<UserResponseDto>(_settings.PromptChannel, (message) =>
            {
                Console.Write($"{message.Message}");

                if (message.IsEndOfStream)
                {
                    Console.WriteLine();
                }
            });

            _hubConnection.On<UserResponseDto>("status", (message) =>
            {
                Console.WriteLine($"{message.Message}");
            });

            await _hubConnection.StartAsync();
        }

        private static UserResponseDto CreateUserResponse(string message, UserRequestDto userRequest, bool hasError)
        {
            return new UserResponseDto
                { Message = message, HasError = hasError, SessionId = userRequest.SessionId, TaskId = userRequest.TaskId };
        }
    }

    public interface IChatClient
    {
        Task<UserResponseDto> Send(UserRequestDto userRequest);
        Task InitializeStreamingConnectionAsync();
    }
}
