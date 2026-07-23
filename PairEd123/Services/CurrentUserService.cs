using PairEd123.Models;

namespace PairEd123.Services
{
    public class CurrentUserService
    {
        public User? CurrentUser { get; private set; }

        public void SetUser(User user) => CurrentUser = user;

        public void Clear() => CurrentUser = null;
    }
}