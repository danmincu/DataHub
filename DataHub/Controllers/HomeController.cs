using DataHub.Hubs;
using DataHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace DataHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHubContext<QueryHub> _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IHubContext<QueryHub> context, IDataGenerator dataGenerator)
        {
            _context = context;
            _logger = logger;
            dataGenerator.GetStream().Subscribe(async array =>
            {
                await _context.Clients.All.SendAsync("data", 
                    array.LastOrDefault(), 
                    string.Join("\n", array.Take(array.Length - 1))).ConfigureAwait(false);
            });

        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}