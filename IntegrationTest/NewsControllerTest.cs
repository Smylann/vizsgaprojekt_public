using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace IntegrationTesting
{
    public class NewsControllerTest : IClassFixture<CustomApplicationFactory>
    {
        private readonly HttpClient _client;

        public NewsControllerTest(CustomApplicationFactory factory)
        {
            _client = factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false
                });
        }

        private static string NewUserName()
        {
            return $"it_news_{Guid.NewGuid():N}";
        }

        private async Task<int> RegisterAndLoginUserAsync()
        {
            var username = NewUserName();
            var password = "asd";

            var registrationResponse = await _client.PostAsJsonAsync("api/User/registration", new
            {
                username,
                email = $"{username}@test.com",
                password
            });
            registrationResponse.EnsureSuccessStatusCode();

            var loginResponse = await _client.PostAsJsonAsync("api/User/login", new
            {
                username,
                password
            });
            loginResponse.EnsureSuccessStatusCode();

            var whoAmI = await _client.GetAsync("api/User/me");
            whoAmI.EnsureSuccessStatusCode();

            var body = await whoAmI.Content.ReadAsStringAsync();
            using var json = JsonDocument.Parse(body);
            return json.RootElement.GetProperty("id").GetInt32();
        }

        [Fact]
        public async Task GetAllUsers_Ok()
        {
            var response = await _client.GetAsync("api/News/getallusers");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetAllPosts_Ok()
        {
            var response = await _client.GetAsync("api/News/getallposts");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetAllCats_Ok()
        {
            var response = await _client.GetAsync("api/News/getallcats");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SearchUser_Ok_WithValidName()
        {
            var response = await _client.GetAsync("api/News/search_user?name=admin");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SearchUser_BadRequest_WithEmptyName()
        {
            var response = await _client.GetAsync("api/News/search_user?name=");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SearchPost_Ok_WithValidTitleAndCat()
        {
            var response = await _client.GetAsync("api/News/search_post?title=Future&cat=All");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SearchPost_BadRequest_WithInvalidTitle()
        {
            var response = await _client.GetAsync("api/News/search_post?title=nincstalalat&cat=All");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SearchPostByCat_Ok_WithValidCategory()
        {
            var response = await _client.GetAsync("api/News/search_post_by_cat?cat=Technology");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task DeleteUsers_Unauthorized_WithoutLogin()
        {
            var response = await _client.DeleteAsync("api/News/delete_users?id=1");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DeleteUsers_Forbidden_ForNormalUser()
        {
            await RegisterAndLoginUserAsync();

            var response = await _client.DeleteAsync("api/News/delete_users?id=1");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task FavouritePosts_Ok_ForLoggedInUser()
        {
            var userId = await RegisterAndLoginUserAsync();

            var response = await _client.PostAsJsonAsync("api/News/favourite_posts", new
            {
                userId,
                postId = 1
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Vote_Ok_ForLoggedInUser()
        {
            var userId = await RegisterAndLoginUserAsync();

            var response = await _client.PostAsJsonAsync("api/News/vote", new
            {
                userId,
                postId = 1,
                isUpvote = true
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Comment_Ok_ForLoggedInUser()
        {
            var userId = await RegisterAndLoginUserAsync();

            var response = await _client.PostAsJsonAsync("api/News/comment", new
            {
                userId,
                postID = 1,
                commentcontent = "integration test comment"
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CreateReport_Ok_WithValidPayload()
        {
            var userId = await RegisterAndLoginUserAsync();

            var response = await _client.PostAsJsonAsync("api/News/create_report", new
            {
                postID = 1,
                userID = userId,
                reportreason = "integration test report"
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
