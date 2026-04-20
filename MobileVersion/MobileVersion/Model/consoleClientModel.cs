using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MobileVersion.Dtos;

namespace MobileVersion.Models;

public enum VoteState { None, Upvoted, Downvoted }

public class consoleClientModel
{
    private HttpClient _httpClient;

    public DisplayAllUserDTO? CurrentUser { get; set; }

    public consoleClientModel(string port)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(port)
        };
    }
   /***************************
    *                         *
    *                         *
    *        DISPLAYS         *
    *                         *
    *                         *
    ***************************/
    public async Task<List<DisplayAllUserDTO>> getallusers()
    {
        return await _httpClient.GetFromJsonAsync<List<DisplayAllUserDTO>>($"api/news/getallusers");
    }
    public async Task<List<DisplayAllPostsDTO>> getallposts()
    {
        return await _httpClient.GetFromJsonAsync<List<DisplayAllPostsDTO>>($"api/news/getallposts");
    }
    public async Task<List<CategoryDTO>> getallcats()
    {
        return await _httpClient.GetFromJsonAsync<List<CategoryDTO>>($"api/news/getallcats");
    }
    public async Task<List<UsersBySearch>> searchUsers(string name)
    {
        return await _httpClient.GetFromJsonAsync<List<UsersBySearch>>($"api/news/search_user?name={name}");
    }

    public async Task<List<PostsBySearch>> searchPostswithCat(string title, string cat)
    {
        return await _httpClient.GetFromJsonAsync<List<PostsBySearch>>($"api/news/search_post?title={title}&cat={cat}");
    }
    public async Task<List<PostsByCat>> searchPostsbycat(string cat)
    {
        return await _httpClient.GetFromJsonAsync<List<PostsByCat>>($"api/news/search_post_by_cat?cat={cat}");
    }
    public async Task<List<GetCommentsFromPost>> fetchcomments(int id)
    {
        return await _httpClient.GetFromJsonAsync<List<GetCommentsFromPost>>($"api/news/getcommentsfrompost?id={id}");
    }
    public async Task<List<OwnPosts>> ownposts(int id)
    {
        return await _httpClient.GetFromJsonAsync<List<OwnPosts>>($"api/news/getownposts?id={id}");
    }
    public async Task<List<OwnComments>> owncomments(int id)
    {
        return await _httpClient.GetFromJsonAsync<List<OwnComments>>($"api/news/getowncomments?id={id}");
    }
    public async Task<List<PostsFromOwnComment>> postsfromowncomment(int id)
    {
        var sutty = await _httpClient.GetFromJsonAsync<List<PostsFromOwnComment>>($"api/news/postsfromowncomment?id={id}");
        return sutty;
    }
    public async Task<List<LikedPosts>> likedposts(int id)
    {
        return await _httpClient.GetFromJsonAsync<List<LikedPosts>>($"api/news/getlikedposts?id={id}");
    }
    public async Task<List<DislikedPosts>> dislikedposts(int id)
    {
        return await _httpClient.GetFromJsonAsync<List<DislikedPosts>>($"api/news/getdislikedposts?id={id}");
    }
    public async Task<List<Favorites>> favorites(int id)
    {
        return await _httpClient.GetFromJsonAsync<List<Favorites>>($"api/news/getfavorites?id={id}");
    }
    public async Task<List<OwnReports>> reports(int id)
    {
        return await _httpClient.GetFromJsonAsync<List<OwnReports>>($"api/news/ownreports?id={id}");
    }

    /***************************
    *                         *
    *                         *
    *     LOGIN / REGISTER    *
    *                         *
    *                         *
    ***************************/

    public async Task LogIn(LoginDto dto)
    {
        var resp = await _httpClient.PostAsJsonAsync($"api/user/login", dto);
        resp.EnsureSuccessStatusCode();
    }

    public async Task<UserDataDTO> me() => await _httpClient.GetFromJsonAsync<UserDataDTO>($"api/user/me");

    public async Task LogOut()
    {
        var resp = await _httpClient.PostAsync($"api/user/logout", null);
        resp.EnsureSuccessStatusCode();
    }

    public async Task Register(RegistrationDto dto)
    {
        var resp = await _httpClient.PostAsJsonAsync($"api/user/registration", dto);
        resp.EnsureSuccessStatusCode();
    }
    public async Task ModifyPassword(ModifyPasswordDTO dto)
    {
        var resp = await _httpClient.PutAsJsonAsync($"api/user/modifypassword", dto);
        resp.EnsureSuccessStatusCode();
    }

    /***************************
    *                         *
    *                         *
    *    USER INTERACTIONS    *
    *                         *
    *                         *
    ***************************/

    public async Task createPost(PostDTO post)
    {
        var resp = await _httpClient.PostAsJsonAsync($"api/news/create_posts", post);
        resp.EnsureSuccessStatusCode();
    }
    public async Task deleteOwnPost(DeleteOwnPostDTO deleteOwnPost)
    {
        var resp = await _httpClient.DeleteAsync($"api/news/delete_own_post?postid={deleteOwnPost.postid}&userid={deleteOwnPost.userId}");
        resp.EnsureSuccessStatusCode();
    }
    public async Task favouritePosts(FavouritePostDTO dto)
    {
        var resp = await _httpClient.PostAsJsonAsync($"api/news/favourite_posts", dto);
        resp.EnsureSuccessStatusCode();
    }
    public async Task votePost(VoteDTO dto)
    {
        var resp = await _httpClient.PostAsJsonAsync($"api/news/vote", dto);
        resp.EnsureSuccessStatusCode();
    }
    public async Task comment(CommentDTO comment)
    {
        var resp = await _httpClient.PostAsJsonAsync($"api/news/comment", comment);
        resp.EnsureSuccessStatusCode();
    }
    public async Task createReport(ReportDTO report)
    {
        var resp = await _httpClient.PostAsJsonAsync($"api/news/create_report", report);
        resp.EnsureSuccessStatusCode();
    }
   
}
