using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Web.Http;
using ZRWebAPiApp.Models;
using Formatting = Newtonsoft.Json.Formatting;
using RouteAttribute = System.Web.Http.RouteAttribute;

namespace ZRWebAPiApp.Controllers
{
    [Route("api/[controller]")]
    //[ApiController]
    public class PostController : ControllerBase
    {
        private readonly string _baseUrl = "http://localhost:1844/api/post/GetPosts";

        private static readonly List<Posts> Posts = new List<Posts>
        {
            new Posts
            {
                id = 1,
                author = "Rylee Paul",
                authorid = 9,
                likes = 960,
                popularity = 0.13,
                reads = 50361,
                tags = new List<string> { "science", "tech", "design", "culture" }
            },
            new Posts
            {
                id = 2,
                author = "Jane Doe",
                authorid = 456,
                likes = 20,
                popularity = 0.85,
                reads = 200,
                tags = new List<string> { "web", "javascript", "react" }
            }
        };

        [Microsoft.AspNetCore.Mvc.HttpGet("/api/[controller]/[action]")]
        public ActionResult<List<Posts>> GetPosts()
        {
            return Posts;
        }

        //  Please note :  hit the request using swagger

        [Microsoft.AspNetCore.Mvc.HttpGet("/api/[controller]/[action]/{tags}")]
        //[Route("getposts")]
        public string GetPosts(string tags, string? sortBy, string? direction)
        {
            // Validate required tags parameter
            if (string.IsNullOrEmpty(tags))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            // Build request URL
            var requestUrl = $"{_baseUrl}";

            // Validate Filter (sortBy,direction)
            if (!string.IsNullOrEmpty(sortBy))
            {
                requestUrl += $"&sortBy={sortBy}";
            }
            if (!string.IsNullOrEmpty(direction))
            {
                requestUrl += $"&direction={direction}";
            }

            // Send HTTP GET request to API
            using (var client = new HttpClient())
            {
                var response = client.GetAsync(requestUrl).Result;

                // Check for HTTP error status
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpResponseException(response.StatusCode);
                }

                var responseJson = response.Content.ReadAsStringAsync().Result;
                var postss = JsonConvert.DeserializeObject<List<Posts>>(responseJson).ToList();

                var newRequestUrl = $"{_baseUrl}/posts?tag={tags}";
                var filterrequestUrl = requestUrl.Replace(requestUrl, newRequestUrl);

                // Remove duplicates
                var posts = postss.Where(post => post.tags.Contains(tags)).Distinct().ToList();

                // Sort posts
                if (!string.IsNullOrEmpty(sortBy))
                {
                    switch (sortBy.ToLower())
                    {
                        case "id":
                            posts = direction.ToLower() == "desc" ? posts.OrderByDescending(p => p.id).ToList() : posts.OrderBy(p => p.id).ToList();
                            break;
                        case "reads":
                            posts = direction.ToLower() == "desc" ? posts.OrderByDescending(p => p.reads).ToList() : posts.OrderBy(p => p.reads).ToList();
                            break;
                        case "likes":
                            posts = direction.ToLower() == "desc" ? posts.OrderByDescending(p => p.likes).ToList() : posts.OrderBy(p => p.likes).ToList();
                            break;
                        case "popularity":
                            posts = direction.ToLower() == "desc" ? posts.OrderByDescending(p => p.popularity).ToList() : posts.OrderBy(p => p.popularity).ToList();
                            break;
                        default:
                            throw new HttpResponseException(HttpStatusCode.BadRequest);
                    }
                }
                return JsonConvert.SerializeObject(posts, Formatting.Indented);
            }
        }
    }

}

