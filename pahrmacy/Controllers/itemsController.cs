using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using pahrmacy.Data;
using pahrmacy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pahrmacy.Controllers
{
    public class itemsController : Controller
    {
        private readonly pahrmacyContext _context;

        public itemsController(pahrmacyContext context)
        {
            _context = context;
        }

        // GET: items
        public async Task<IActionResult> Index()
        {
            HttpContext.Session.LoadAsync();
            return View(await _context.items.ToListAsync());
        }

        // GET: items/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var items = await _context.items
                .FirstOrDefaultAsync(m => m.Id == id);
            if (items == null)
            {
                return NotFound();
            }

            return View(items);
        }

        public async Task<IActionResult> Details1(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var items = await _context.items
                .FirstOrDefaultAsync(m => m.Id == id);
            if (items == null)
            {
                return NotFound();
            }

            return View(items);
        }


        public async Task<IActionResult> Category() {

            HttpContext.Session.LoadAsync();
            String role = HttpContext.Session.GetString("Role");
            if (role != "customer")
            {

                return RedirectToAction("Login", "useraccounts");
            }
            else
            { 
            
             return View(await _context.items.OrderBy(c=>c.category).ToListAsync());
            }
          


        }

        public async Task<IActionResult> dash()
        {
            HttpContext.Session.LoadAsync();
            string ro = HttpContext.Session.GetString("Role");
            if (ro != "admin")
            {
                return RedirectToAction("Login", "useraccounts");
            }
            
            {
                string sql = "";

                SqlConnection conn = new SqlConnection("Data Source=.\\SQLEXPRESS;Initial Catalog=master;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
                SqlCommand comm = new SqlCommand(sql, conn);

                
                conn.Open();
                sql = "SELECT COUNT( Id)  FROM items where category =1";
                comm = new SqlCommand(sql, conn);
                ViewData["d1"] = (int)comm.ExecuteScalar();

                sql = "SELECT COUNT( Id)  FROM items where category =2";
                comm = new SqlCommand(sql, conn);
                ViewData["d2"] = (int)comm.ExecuteScalar();

                sql = "SELECT COUNT( Id)  FROM items where category =3";
                comm = new SqlCommand(sql, conn);
                ViewData["d3"] = (int)comm.ExecuteScalar();

                return View();
            }
        }


        // POST: items/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile file,[Bind("Id,name,description,price,discount,category,quantity")] items items)
        {
            if (file != null)
            {
                string filename = file.FileName;
                //  string  ext = Path.GetExtension(file.FileName);
                string path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images"));
                using (var filestream = new FileStream(Path.Combine(path, filename), FileMode.Create))
                { await file.CopyToAsync(filestream); }


                items.imgfile = filename;
               }

            _context.Add(items);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: items/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var items = await _context.items.FindAsync(id);
            if (items == null)
            {
                return NotFound();
            }
            return View(items);
        }

        // POST: items/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IFormFile file, int id, [Bind("Id,name,description,price,discount,category,quantity")] items items)
        {
            if (id != items.Id)
            {
                return NotFound();
            }

            if (file != null)
            {
                string filename = file.FileName;
                //  string  ext = Path.GetExtension(file.FileName);
                string path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images"));
                using (var filestream = new FileStream(Path.Combine(path, filename), FileMode.Create))
                { await file.CopyToAsync(filestream); }

                items.imgfile = filename;
            }
            _context.Update(items);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

            return View(items);

            
        }

        // GET: items/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var items = await _context.items
                .FirstOrDefaultAsync(m => m.Id == id);
            if (items == null)
            {
                return NotFound();
            }

            return View(items);
        }

        // POST: items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var items = await _context.items.FindAsync(id);
            if (items != null)
            {
                _context.items.Remove(items);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool itemsExists(int id)
        {
            return _context.items.Any(e => e.Id == id);
        }
    }
}
