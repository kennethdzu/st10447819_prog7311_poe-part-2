using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TechMove.Glms.Web.Data;
using TechMove.Glms.Web.Models;

namespace TechMove.Glms.Web.Controllers
{
    public class ClientController : Controller
    {
        AppDbContext db;

        public ClientController(AppDbContext context)
        {
            db = context;
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Client client)
        {
            if (ModelState.IsValid)
            {
                db.Add(client);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", "Contract");
            }
            return View(client);
        }
    }
}
