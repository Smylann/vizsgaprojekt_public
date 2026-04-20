using MobileVersion.Dtos;
using MobileVersion.Models;
using MobileVersion.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MobileVersion.Model
{
    public class Filtering
    {
        private readonly consoleClientModel _model;
        private readonly Dictionary<int, PostVM> _postCache = new();
        public Filtering(consoleClientModel model) => _model = model;

        public void ClearCache() => _postCache.Clear();

        public async Task Filter(
            List<DisplayAllPostsDTO> allp,
            List<DisplayAllUserDTO> allus,
            string text,
            CategoryDTO? cat,
            string sort,
            ObservableCollection<PostVM> fposts,
            ObservableCollection<UserVM> fusers,
            int postsLimit,
            int usersLimit) 
        {
            var posts = new List<DisplayAllPostsDTO>(allp);
            var users = new List<DisplayAllUserDTO>(allus);

            bool hasSearch = !string.IsNullOrWhiteSpace(text);
            bool hasCategory = cat != null && !cat.categoryname.Equals("All", StringComparison.OrdinalIgnoreCase);

            // Step 1: Filter posts
            if (hasSearch && hasCategory)
            {
                try
                {
                    var searchandcat = await _model.searchPostswithCat(text, cat!.categoryname);
                    posts = searchandcat.Select(p => new DisplayAllPostsDTO
                    {
                        PostID = p.PostID,
                        UserID = p.UserID,
                        UserName = p.UserName,
                        Title = p.Title,
                        Content = p.Content,
                        Created_at = p.Created_at,
                        Votes = p.Votes
                    }).ToList();
                }
                catch { posts = new(); }
            }
            else if (hasSearch)
            {
                try
                {
                    var searchall = await _model.searchPostswithCat(text, "All");
                    posts = searchall.Select(p => new DisplayAllPostsDTO
                    {
                        PostID = p.PostID,
                        UserID = p.UserID,
                        UserName= p.UserName,
                        Title = p.Title,
                        Content = p.Content,
                        Created_at = p.Created_at,
                        Votes = p.Votes
                    }).ToList();
                }
                catch { posts = new(); }
            }
            else if (hasCategory)
            {
                try
                {
                    var searchcat = await _model.searchPostsbycat(cat!.categoryname);
                    posts = searchcat.Select(p => new DisplayAllPostsDTO
                    {
                        PostID = p.PostID,
                        UserID = p.UserID,
                        UserName = p.UserName,
                        Title = p.Title,
                        Content = p.Content,
                        Created_at = p.Created_at,
                        Votes = p.Votes
                    }).ToList();
                }
                catch { posts = new(); }
            }

            // Step 2: Filter users
            if (hasSearch)
            {
                try
                {
                    var searchuser = await _model.searchUsers(text);
                    users = searchuser.Select(p => new DisplayAllUserDTO
                    {
                        UserID = p.UserID,
                        Username = p.Username
                    }).ToList();
                }
                catch { users = new(); }
            }

            // Step 3: Apply Sorting
            if (!string.IsNullOrEmpty(sort))
            {
                posts = sort switch
                {
                    "Newest" => posts.OrderByDescending(p => p.Created_at).ToList(),
                    "Oldest" => posts.OrderBy(p => p.Created_at).ToList(),
                    "Most Liked" => posts.OrderByDescending(p => p.Votes).ToList(),
                    "Most Disliked" => posts.OrderBy(p => p.Votes).ToList(),
                    _ => posts
                };
            }

            fposts.Clear();
            foreach (var p in posts.Take(postsLimit)) 
            {
                if (!_postCache.TryGetValue(p.PostID, out var vm))
                {
                    vm = new PostVM(_model, p);
                    _postCache[p.PostID] = vm;
                }
                else { vm.Post = p; }
                fposts.Add(vm);
            }

            fusers.Clear();
            foreach (var u in users.Take(usersLimit)) { fusers.Add(new UserVM(u, _model)); } 
        }
    }
}
