using Microsoft.AspNetCore.Mvc;
using HospitalApp.Models;

namespace HospitalApp.Controllers
{
    public class PostsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PostsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            List<Post> posts;

            try
            {
                posts = await client.GetFromJsonAsync<List<Post>>("https://jsonplaceholder.typicode.com/posts")
                        ?? new List<Post>();
            }
            catch (HttpRequestException)
            {
                posts = new List<Post>();
                ViewBag.Error = "Could not load posts. Check your internet connection.";
            }

            return View(posts);
        }
    }
}