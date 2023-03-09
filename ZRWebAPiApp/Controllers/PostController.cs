using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Web.Http;
using System.Xml;
using ZRWebAPiApp.Models;
using Formatting = Newtonsoft.Json.Formatting;
using RouteAttribute = System.Web.Http.RouteAttribute;

namespace ZRWebAPiApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly string _baseUrl = "https://localhost:44396/api/post/allpost";

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

        [Route("allpost")]
        public ActionResult<List<Posts>> GetPosts()
        {
            return Posts;
        }

        //  Be noted :  please hit the postman url: https://localhost:44396/api/post/allpost/posts?tag=web&sortby=popularity&direction=desc   or https://localhost:44396/api/post/allpost/posts?tag=web


        [Route("GetPosts")]
        public string GetPosts(string tags, string? sortBy, string? direction)
        {
            // Validate required tags parameter
            if (string.IsNullOrEmpty(tags))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            var postlist = GetPosts();
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
                var posts = JsonConvert.DeserializeObject<List<Posts>>(responseJson).ToList();

                var newRequestUrl = $"{_baseUrl}/posts?tag={tags}";
                var filterrequestUrl = requestUrl.Replace(requestUrl, newRequestUrl);

                // Remove duplicates
                var distinctPosts = posts.Where(post => post.tags.Contains(tags)).Distinct().ToList();

                // Sort posts
                if (!string.IsNullOrEmpty(sortBy))
                {
                    switch (sortBy.ToLower())
                    {
                        case "id":
                            distinctPosts = direction.ToLower() == "desc" ? distinctPosts.OrderByDescending(p => p.id).ToList() : distinctPosts.OrderBy(p => p.id).ToList();
                            break;
                        case "reads":
                            distinctPosts = direction.ToLower() == "desc" ? distinctPosts.OrderByDescending(p => p.reads).ToList() : distinctPosts.OrderBy(p => p.reads).ToList();
                            break;
                        case "likes":
                            distinctPosts = direction.ToLower() == "desc" ? distinctPosts.OrderByDescending(p => p.likes).ToList() : distinctPosts.OrderBy(p => p.likes).ToList();
                            break;
                        case "popularity":
                            distinctPosts = direction.ToLower() == "desc" ? distinctPosts.OrderByDescending(p => p.popularity).ToList() : distinctPosts.OrderBy(p => p.popularity).ToList();
                            break;
                        default:
                            throw new HttpResponseException(HttpStatusCode.BadRequest);
                    }
                }
                return JsonConvert.SerializeObject(distinctPosts, Formatting.Indented);
            }
        }
    }

}

