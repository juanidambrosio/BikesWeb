namespace MyBikes.DomainLogic
{
    using System.Linq;

    public class UserRepository
    {
        public UserRepository()
        {

        }

        public User GetUser(int userId)
        {
            var db = new ApplicationDbContext();
            var theUser = db.Users.FirstOrDefault(user => user.Id == userId);

            return theUser == null ? null : new User
            {
                Id = userId,
                FirstName = theUser.FirstName,
                LastName = theUser.LastName
            };
        }
    }
}