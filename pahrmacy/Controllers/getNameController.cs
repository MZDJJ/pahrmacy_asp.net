using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using pahrmacy.Models;


namespace pahrmacy.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class getNameController : ControllerBase
    {
        // getting all book search catagory 
        [HttpGet("{role}")]
        public IEnumerable<allUsers> Get(string role)
        {
            List<allUsers> li = new List<allUsers>();
           //SqlConnection conn1 = new SqlConnection("Data Source=.\\SQLEXPRESS;Initial Catalog=master;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");
            var builder = WebApplication.CreateBuilder();
            string conStr = builder.Configuration.GetConnectionString("pahrmacyContext");
            SqlConnection conn1 = new SqlConnection(conStr);
            string sql;
            sql = "SELECT * FROM useraccounts where role ='" + role + "' ";
             SqlCommand comm = new SqlCommand(sql, conn1);
            conn1.Open();
            SqlDataReader reader = comm.ExecuteReader();

            while (reader.Read())
            {
                li.Add(new allUsers
                {
                    name = (string)reader["name"],
                });

            }

            reader.Close();
            conn1.Close();
            return li;
        }
    }
}