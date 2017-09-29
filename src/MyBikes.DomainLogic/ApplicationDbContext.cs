using System.Collections.Generic;

namespace MyBikes.DomainLogic
{
    internal class ApplicationDbContext
    {
        public List<User> Users { get; set; }
        public ApplicationDbContext()
        {
        }
    }
}