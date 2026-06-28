using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using pahrmacy.Data;
using pahrmacy.Models;
 using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace pahrmacy.Controllers
{
    public class useraccountsController : Controller
    {
        private readonly pahrmacyContext _context;

        public useraccountsController(pahrmacyContext context)
        {
            _context = context;
        }


        //Login Action

        public IActionResult Login()
        {
            if (!HttpContext.Request.Cookies.ContainsKey("Name"))
            {
                return View();
            }
            else
            {
                string na = HttpContext.Request.Cookies["Name"].ToString();
                string ro = HttpContext.Request.Cookies["Role"].ToString();
                HttpContext.Session.SetString("Name", na);
                HttpContext.Session.SetString("Role", ro);
                if (ro == "customer")
                    return RedirectToAction("customer_home", "useraccounts");
                else
                    return RedirectToAction("admin_Home", "useraccounts");
            }
            
        }
        [HttpPost, ActionName("Login")]
        public async Task<IActionResult> Login(string na, string pa, string auto)
        {
            var ur = await _context.useraccounts.FromSqlRaw("select * from useraccounts where name ='" + na +
                "' and pass ='" + pa + "' ").FirstOrDefaultAsync();
            if (ur != null)
            {

                int id = ur.Id;
                string na1 = ur.name;
                string ro = ur.role;
                HttpContext.Session.SetString("userid", Convert.ToString(id));
                HttpContext.Session.SetString("Name", na1);
                HttpContext.Session.SetString("Role", ro);

                if (auto == "auto-login")
                {
                    HttpContext.Response.Cookies.Append("Name", na1);
                    HttpContext.Response.Cookies.Append("Role", ro);
                }

                if (ro == "customer")
                    return RedirectToAction("customer_home", "useraccounts");
                else if (ro == "admin")
                    return RedirectToAction("admin_home", "useraccounts");
                else
                    return View();
            }
            else
            {
                ViewData["Message"] = "wrong username or password";
                return View();
            }

        }

        public async Task<IActionResult> admin_home()
        {
            HttpContext.Session.LoadAsync();
            string ro = HttpContext.Session.GetString("Role");
            if (ro == "admin") {
                ViewData["name"]= HttpContext.Session.GetString("Name");
                return View();
            }
                
            else
                return RedirectToAction("Login", "useraccounts");
        }
        

        public async Task<IActionResult> customer_home() {
            HttpContext.Session.LoadAsync();
            string ro = HttpContext.Session.GetString("Role");
            if (ro == "customer")
            {
                ViewData["name"] = HttpContext.Session.GetString("Name");
                var discount = await _context.items.
                    Where(b => b.discount == "yes").ToListAsync();
                return View(discount);
            }

            else
                return RedirectToAction("Login", "useraccounts");

        }

        public async Task<IActionResult> email() {

            HttpContext.Session.LoadAsync();
            string ro = HttpContext.Session.GetString("Role");
            if (ro != "admin")
            {
                return RedirectToAction("Login", "useraccounts");
            }
            else
                return View();



        }

        [HttpPost, ActionName("email")]
        [ValidateAntiForgeryToken]
        public IActionResult email(string address, string subject, string body) { 

            HttpContext.Session.LoadAsync();
            string ro = HttpContext.Session.GetString("Role");
            if (ro != "admin")
            {
                return RedirectToAction("Login", "useraccounts");
            }
            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
            var mail = new MailMessage();
            mail.From = new MailAddress("qmohmmedq49@gmail.com");
            mail.To.Add(address); // receiver email address 
            mail.Subject = subject;
            mail.IsBodyHtml = true;
            mail.Body = body;
            SmtpServer.Port = 587;
            SmtpServer.UseDefaultCredentials = false;
            SmtpServer.Credentials = new System.Net.NetworkCredential("qmohmmedq49@gmail.com",
"vykgghevmpeihats");
            SmtpServer.EnableSsl = true;
            SmtpServer.Send(mail);
            ViewData["Message"] = "Email sent.";
            return View();



        }

        public IActionResult users_search()
        {
            HttpContext.Session.LoadAsync();
            string ro = HttpContext.Session.GetString("Role");
            if (ro != "admin")
            {
                return RedirectToAction("Login", "useraccounts");
            }
            
                useraccounts users = new useraccounts(); 
            
            return View(users);    


        }

        [HttpPost]

        public async Task<IActionResult> users_search(string name) { 
        
            HttpContext.Session.LoadAsync();
            string ro = HttpContext.Session.GetString("Role");
            if (ro != "admin")
            {
                return RedirectToAction("Login", "useraccounts");
            }
            var users = await _context.useraccounts.
                FromSqlRaw("select * from useraccounts where name = '" + name + "' ").FirstOrDefaultAsync();
            return View(users);

        }
       
        
        public IActionResult Add_Admin()
        {
            HttpContext.Session.LoadAsync();
            string role = HttpContext.Session.GetString("Role");

            if (role != "admin")
            {
                return RedirectToAction("Login", "useraccounts");


            }
            return View();
        }
        [HttpPost, ActionName("Add_Admin")]
        public async Task<IActionResult> Add_Admin([Bind ("name","Pass")]useraccounts acc) {
        
             HttpContext.Session.LoadAsync();
          string role=  HttpContext.Session.GetString("Role");

            
            SqlConnection conn = new SqlConnection("Data Source=.\\SQLEXPRESS;Initial Catalog=master;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");
            MD5 md5 = new MD5CryptoServiceProvider();
            string paa = Encoding.ASCII.GetString(md5.ComputeHash(ASCIIEncoding.Default.GetBytes(acc.Pass)));
            conn.Open();
            string sql;
            
            sql = "select * from useraccounts  where name = '" + acc.name + "'";
            SqlCommand comm = new SqlCommand(sql, conn);
            SqlDataReader reader = comm.ExecuteReader();
            if (reader.Read())
            {
                ViewData["Message"] = "name already exists";
                reader.Close();
            }
            else
            {
                reader.Close();
                acc.role = "admin";
                sql ="insert into useraccounts (name,Pass,role) values ('" + acc.name + "','" + acc.Pass + "','" + acc.role + "')";
                comm = new SqlCommand(sql, conn);
                comm.ExecuteNonQuery();
                ViewData["message"] = "Sucessfully added";
                return RedirectToAction("Index");
            }
            conn.Close();
            return View();



        }


        public IActionResult Logout()
        {
            HttpContext.Response.Cookies.Delete("Name");
            HttpContext.Response.Cookies.Delete("Role");
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "useraccounts");

        }

        public IActionResult Registration()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Registration([Bind("name,email,job,gender,married, location")] customer cust, [Bind("name,Pass,role")] useraccounts acc)
        {

            SqlConnection conn = new SqlConnection("Data Source=.\\SQLEXPRESS;Initial Catalog=master;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
            
            MD5 md5 = new MD5CryptoServiceProvider();
            string paa = Encoding.ASCII.GetString(md5.ComputeHash(ASCIIEncoding.Default.GetBytes(acc.Pass)));

            conn.Open();
            string sql = "select * from useraccounts  where name = '" + acc.name + "' and pass = '" + paa + " ' ";
            SqlCommand comm = new SqlCommand(sql, conn);
            SqlDataReader reader = comm.ExecuteReader();
            if (reader.Read())
            {
                ViewData["message"] = "name and password already exists";
                reader.Close();
            }
            else
            {
                reader.Close();
                sql = "insert into customer (name,email,job,married,gender,location)  values  ('" + cust.name + "','" + cust.email + "','" + cust.job + "','" + cust.married + "' ,'" + cust.gender + "','" + cust.location + "')";
                comm = new SqlCommand(sql, conn);
                comm.ExecuteNonQuery();

                acc.role = "customer";
                sql = "insert into useraccounts (name,pass,role)  values  ('" + acc.name + "','" + paa + "','" + acc.role + "')";
                comm = new SqlCommand(sql, conn);
                comm.ExecuteNonQuery();
                ViewData["message"] = "Sucessfully added";
                int id = acc.Id;
                string na1 = acc.name;
                string ro = acc.role;
                HttpContext.Session.SetString("userid", Convert.ToString(id));
                HttpContext.Session.SetString("Name", na1);
                HttpContext.Session.SetString("Role", ro);
                return RedirectToAction("customer_home", "useraccounts");
            }
            conn.Close();
            return View();
        }




        // GET: useraccounts
        public async Task<IActionResult> Index()
        {
            return View(await _context.useraccounts.ToListAsync());
        }

        // GET: useraccounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var useraccounts = await _context.useraccounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (useraccounts == null)
            {
                return NotFound();
            }

            return View(useraccounts);
        }

        // GET: useraccounts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: useraccounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,name,Pass,role")] useraccounts useraccounts)
        {
            if (ModelState.IsValid)
            {
                _context.Add(useraccounts);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(useraccounts);
        }

        // GET: useraccounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var useraccounts = await _context.useraccounts.FindAsync(id);
            if (useraccounts == null)
            {
                return NotFound();
            }
            return View(useraccounts);
        }

        // POST: useraccounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,name,Pass,role")] useraccounts useraccounts)
        {
            if (id != useraccounts.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(useraccounts);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!useraccountsExists(useraccounts.Id))
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
            return View(useraccounts);
        }

        // GET: useraccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var useraccounts = await _context.useraccounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (useraccounts == null)
            {
                return NotFound();
            }

            return View(useraccounts);
        }

        // POST: useraccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var useraccounts = await _context.useraccounts.FindAsync(id);
            if (useraccounts != null)
            {
                _context.useraccounts.Remove(useraccounts);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool useraccountsExists(int id)
        {
            return _context.useraccounts.Any(e => e.Id == id);
        }
    }
}
