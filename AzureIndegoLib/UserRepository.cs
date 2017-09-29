using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureIndegoLib
{
    public class UserRepository
    {
        public User GetUser(int userId)
        {
            // hit database, get user
            return new User
            {
                Id = userId,
                FirstName = "Abel",
                LastName = "Wang"
            };
        }
    }
}
