using Microsoft.EntityFrameworkCore;

namespace pahrmacy
{
    [Keyless]
    public class report
    {
        public string custname { get; set; }
        public int total { get; set; }
    }
}
