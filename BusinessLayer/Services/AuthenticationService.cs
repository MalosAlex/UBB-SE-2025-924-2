using BusinessLayer.Repositories;

namespace BusinessLayer.Services
{
    public class AuthenticationService
    {
        private readonly UsersRepository usersRepository;
        public AuthenticationService(UsersRepository usersRepository)
        {
            this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
        }
    }
}
