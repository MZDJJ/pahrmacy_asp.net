using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using pahrmacy.Data;
using pahrmacy.Models;

namespace pahrmacy.Controllers
{
    public class ordersController : Controller
    {
        private readonly pahrmacyContext _context;

        public ordersController(pahrmacyContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> orderdetails(string? custname )
        {


            var orItems = await _context.orders.FromSqlRaw("select * from orders where custname = '" +custname + "' ").ToListAsync();
            return View(orItems);
        }


        public async Task<IActionResult> orderline(int? orid)
        {
            var orItems = await _context.orderline.FromSqlRaw("select * from orderline where orderid =  '" + orid +"' ").ToListAsync();
            return View(orItems);


        }

        public async Task<IActionResult> CatalogueBuy()
        {
            await HttpContext.Session.LoadAsync();
            string role = HttpContext.Session.GetString("Role");

            if (role != "customer")
            {
                return RedirectToAction("Login", "useraccounts");
            }

            return View(await _context.items.ToListAsync());
        }


        public async Task<IActionResult> ItemsBuyDetail(int? id)
        {
            await HttpContext.Session.LoadAsync();
            string role = HttpContext.Session.GetString("Role");

            if (role != "customer")
            {
                return RedirectToAction("Login", "useraccounts");
            }

            var items = await _context.items.FindAsync(id);
            return View(items);
        }

        List<buyitems> itm = new List<buyitems>();
        [HttpPost]
        public async Task<IActionResult> cartadd(int Id, int quantity)
        {
            await HttpContext.Session.LoadAsync();

            var sessionString = HttpContext.Session.GetString("Cart");
            if (sessionString is not null)
            {
                itm =JsonSerializer.Deserialize<List<buyitems>>(sessionString);
            }

            var item = await _context.items
                .FromSqlRaw("select * from items where Id = '" + Id + "'")
                .FirstOrDefaultAsync();

            if (item == null)
            {
                ViewData["Error"] = "Item not found.";
                return View("itemsBuyDetail", item);
            }

            if (quantity > item.quantity)
            {
                ViewData["Error"] = "Requested quantity exceeds available stock.";
                return View("itemsBuyDetail", item);
            }

            itm.Add(new buyitems
            {
                name = item.name,
                Price = item.price,
                quant = quantity
            });

            item.quantity -= quantity;
            _context.Update(item);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(itm));

            return RedirectToAction("CartBuy");
        }

        public async Task<IActionResult> CartBuy()
        {
            await HttpContext.Session.LoadAsync();

            var sessionString = HttpContext.Session.GetString("Cart");

            if (sessionString is not null)
            {
                itm = JsonSerializer.Deserialize<List<buyitems>>(sessionString);
            }

            return View(itm);
        }

        public async Task<IActionResult> Buy()
        {
            await HttpContext.Session.LoadAsync();
            var sessionString = HttpContext.Session.GetString("Cart");
            if (sessionString is not null)
            {
                itm = JsonSerializer.Deserialize<List<buyitems>>(sessionString);
            }

            string ctname = HttpContext.Session.GetString("Name");
            orders order = new orders();
            order.total = 0;
            order.custname = ctname;
            order.orderdate = DateTime.Today;
            _context.orders.Add(order);
            await _context.SaveChangesAsync();
            var bord = await _context.orders.FromSqlRaw("select * from orders where custname= '" + ctname + "' ").OrderByDescending(e => e.Id).FirstOrDefaultAsync();
            int ordid = bord.Id;
            decimal tot = 0;
            foreach (var it in itm.ToList())
            {
                orderline oline = new orderline();
                oline.orderid = ordid;
                oline.itemname = it.name;
                oline.itemquant = it.quant;
                oline.itemprice = it.Price;
                _context.orderline.Add(oline);
                await _context.SaveChangesAsync();
                var ite = await _context.items.FromSqlRaw("select * from items  where name= '" + it.name + "' ").FirstOrDefaultAsync();
                ite.quantity = ite.quantity - it.quant;
                _context.Update(ite);
                await _context.SaveChangesAsync();

                tot = tot + (it.quant * it.Price);
            }
            bord.total = Convert.ToInt16(tot);
            _context.Update(bord);
            await _context.SaveChangesAsync();

            ViewData["Message"] = "Thank you See you again";
            itm = new List<buyitems>();
            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(itm));
            return RedirectToAction("MyOrder");
        }
        public async Task<IActionResult> MyOrder()
        {
            string ctname = HttpContext.Session.GetString("Name");
            return View(await _context.orders.FromSqlRaw("select * from orders where custname = '" + ctname + "' ").ToListAsync());
        }




        // GET: orders
        public async Task<IActionResult> Index()
        {
            var orItems = await _context.report.FromSqlRaw("SELECT custname, SUM(total) as total FROM orders GROUP BY custname  ").ToListAsync();
            return View(orItems);
        }

        // GET: orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orders = await _context.orders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orders == null)
            {
                return NotFound();
            }

            return View(orders);
        }

        // GET: orders/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,custname,orderdate,total")] orders orders)
        {
            if (ModelState.IsValid)
            {
                _context.Add(orders);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(orders);
        }

        // GET: orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orders = await _context.orders.FindAsync(id);
            if (orders == null)
            {
                return NotFound();
            }
            return View(orders);
        }

        // POST: orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,custname,orderdate,total")] orders orders)
        {
            if (id != orders.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orders);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ordersExists(orders.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(orders);
        }

        // GET: orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orders = await _context.orders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orders == null)
            {
                return NotFound();
            }

            return View(orders);
        }

        // POST: orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orders = await _context.orders.FindAsync(id);
            if (orders != null)
            {
                _context.orders.Remove(orders);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ordersExists(int id)
        {
            return _context.orders.Any(e => e.Id == id);
        }
    }
}
