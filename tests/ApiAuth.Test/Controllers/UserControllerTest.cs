using System.Net;
using System.Net.Http.Headers;
using System.Text;
using ApiAuth.Models;
using ApiAuth.Requests;
using ApiAuth.Responses;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;

namespace ApiAuth.Test.Controllers
{
    public class UserControllerTest
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public UserControllerTest()
        {
            _factory = new WebApplicationFactory<Program>();
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task AdminGetAllUsers()
        {
            // Arrange
            await CreateUser("admin", "admin", "admin@email.com", "admin");
            await Login("admin", "admin");

            // Act
            var response = await _client.GetAsync("/api/user");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var users = JsonConvert.DeserializeObject<IEnumerable<User>>(responseContent);

            Assert.NotNull(users);
            Assert.NotEmpty(users);

            var user = users.FirstOrDefault(x => x.Username == "admin");

            Assert.NotNull(user);
            Assert.Equal("admin", user.Username);
            Assert.Equal("admin", user.Role);
        }


        [Fact]
        public async Task UserGetCurrentUser()
        {
            // Arrange
            await CreateUser("user", "user", "user@gmail.com", "user");
            await Login("user", "user");

            // Act
            var response = await _client.GetAsync("/api/user/current");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<User>(responseContent);

            Assert.NotNull(user);
            Assert.Equal("user", user.Username);
            Assert.Equal("user", user.Role);
        }


        [Fact]
        public async Task UserGetUserByName()
        {
            // Arrange
            var username = "user";
            await CreateUser(username, "user", "user@gmail.com", "user");
            await Login("user", "user");

            // Act
            var response = await _client.GetAsync($"/api/user/{username}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<User>(responseContent);

            Assert.NotNull(user);
            Assert.Equal(username, user.Username);
            Assert.Equal("user", user.Role);
        }


        [Fact]
        public async Task UserCannotGetAllUsers()
        {
            // Arrange
            await CreateUser("user", "user", "user@gmail.com", "user");
            await Login("user", "user");

            // Act
            var response = await _client.GetAsync("/api/user");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }


        private async Task CreateUser(string username, string password, string email, string role)
        {
            var createUserRequest = new CreateUserRequest
            {
                Username = username,
                Password = password,
                Email = email,
                Role = role
            };

            var content = new StringContent(JsonConvert.SerializeObject(createUserRequest), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/user", content);
            response.EnsureSuccessStatusCode();
        }


        private async Task Login(string username, string password)
        {
            var loginRequest = new LoginRequest
            {
                Username = username,
                Password = password
            };

            var content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/auth/login", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        }
    }
}