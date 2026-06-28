using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace pahrmacy.Models
{
    public class orders
    {
        public int Id { get; set; }
        public string custname { get; set; }

        [BindProperty,DataType(DataType.Date)]
        public DateTime orderdate { get; set; }
        public int total { get; set; }
    }
}
