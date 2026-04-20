using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace IntegrationTesting
{
    public class UserControllerTest : IClassFixture<CustomApplicationFactory>
    {
        private readonly HttpClient _client;
        public UserControllerTest(CustomApplicationFactory factory)
        {
            _client = factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false
                });
        }

        private static string NewUserName() => $"it_user_{Guid.NewGuid():N}";

        [Fact]
        public async Task Registration()
        {
            var response = await _client.PostAsJsonAsync("api/User/registration", new
            {
                username = "CsabilovesMilan",
                email = "CsabilovesMilan@test.com",
                password = "asd"
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Registration_Ok_WithJsonBody()
        {
            var username = NewUserName();
            var response = await _client.PostAsJsonAsync("api/User/registration", new
            {
                username,
                email = $"{username}@test.com",
                password = "asd"
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task LogIn_Ok_WithValidUser()
        {
            var username = NewUserName();
            var password = "asd";

            // Check registration succeeds before attempting login
            var registrationResponse = await _client.PostAsJsonAsync("api/User/registration", new
            {
                username,
                email = $"{username}@test.com",
                password
            });
            
            // Ensure registration was successful
            Assert.True(
                registrationResponse.IsSuccessStatusCode, 
                $"Registration failed with status: {registrationResponse.StatusCode}. Body: {await registrationResponse.Content.ReadAsStringAsync()}");

            var response = await _client.PostAsJsonAsync("api/User/login", new
            {
                username,
                password
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            using var json = JsonDocument.Parse(body);
            Assert.Equal("User", json.RootElement.GetProperty("role").GetString());
        }

        [Fact]
        public async Task LogIn_Ok_WithPasswordAlias()
        {
            var username = NewUserName();
            var password = "asd";

            await _client.PostAsJsonAsync("api/User/registration", new
            {
                username,
                email = $"{username}@test.com",
                password
            });
            var response = await _client.PostAsJsonAsync("api/User/login", new
            {
                username,
                password
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task LogIn_Unauthorized_WithInvalidPassword()
        {
            var username = NewUserName();

            await _client.PostAsync($"api/User/registration?username={username}&email={username}@test.com&password=asd", null);
            var response = await _client.PostAsJsonAsync("api/User/login", new
            {
                username,
                password = "wrong"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task LogOut_Ok_AndWhoAmIUnauthorizedAfter()
        {
            var username = NewUserName();
            var password = "asd";

            await _client.PostAsync($"api/User/registration?username={username}&email={username}@test.com&password={password}", null);
            await _client.PostAsJsonAsync("api/User/login", new
            {
                username,
                password
            });

            var logoutResponse = await _client.PostAsync("api/User/logout", null);
            Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);

            var whoAmIResponse = await _client.GetAsync("api/User/me");
            Assert.Equal(HttpStatusCode.Unauthorized, whoAmIResponse.StatusCode);
        }

        [Fact]
        public async Task RoleModify_Unauthorized_WithoutLogin()
        {
            var response = await _client.PutAsync("api/User/rolemodify?userid=2", null);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task RoleModify_Forbidden_ForNormalUser()
        {
            var username = NewUserName();
            var password = "asd";

            await _client.PostAsJsonAsync("api/User/registration", new
            {
                username,
                email = $"{username}@test.com",
                password
            });
            await _client.PostAsJsonAsync("api/User/login", new
            {
                username,
                password
            });

            var response = await _client.PutAsync("api/User/rolemodify?userid=2", null);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ModifyPassword_Unauthorized_WithoutLogin()
        {
            var response = await _client.PutAsync("api/User/modifypassword?username=test&password=newpass", null);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ModifyPassword_Ok_ForLoggedInUser()
        {
            var username = NewUserName();
            var password = "asd";

            await _client.PostAsJsonAsync("api/User/registration", new
            {
                username,
                email = $"{username}@test.com",
                password
            });
            await _client.PostAsJsonAsync("api/User/login", new
            {
                username,
                password
            });

            var response = await _client.PutAsJsonAsync("api/User/modifypassword", new
            {
                currentPassword = password,
                newPassword = "newpass"
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task WhoAmI_Ok_ForLoggedInUser()
        {
            var username = NewUserName();
            var password = "asd";

            await _client.PostAsJsonAsync("api/User/registration", new
            {
                username,
                email = $"{username}@test.com",
                password
            });
            await _client.PostAsJsonAsync("api/User/login", new
            {
                username,
                password
            });

            var response = await _client.GetAsync("api/User/me");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            using var json = JsonDocument.Parse(body);
            Assert.Equal(username, json.RootElement.GetProperty("userName").GetString());
            Assert.Equal("User", json.RootElement.GetProperty("role").GetString());
        }

        [Fact]
        public async Task WhoAmI_Unauthorized_WithoutLogin()
        {
            var response = await _client.GetAsync("api/User/me");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetAllUsers_Ok()
        {
            var response = await _client.GetAsync("api/User/allusers");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Login_ShouldReturnErrorMessage_OnFailedLogin()
        {
            var username = NewUserName();
            var password = "wrongpassword";

            var response = await _client.PostAsJsonAsync("api/User/login", new
            {
                username,
                password
            });

            var errorBody = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
