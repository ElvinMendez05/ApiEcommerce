using ApiEcommerce.Data;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.Interface;

namespace ApiEcommerce.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        public UserRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public User? GetUser(int id)
        {
            return _db.Users.FirstOrDefault(u => u.Id == id);
        }

        public ICollection<User> GetUsers()
        {
            return _db.Users.OrderBy(u => u.UserName).ToList();
        }

        public bool IsUniqueUser(string username)
        {
            return !_db.Users.Any(u => u.UserName.ToLower().Trim() == username.ToLower().Trim());
        }

        public Task<UserLoginResponseDto> Login(UserLoginDto loginResponse)
        {
            throw new NotImplementedException();
        }

        public async Task<User> Register(CreateUserDto createUserDto)
        {
            var encriptedPassword = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);
            var user = new User()
            {
                UserName = createUserDto.Username ?? "No username",
                Name = createUserDto.Name ?? "No name",
                Role = createUserDto.Role ?? "No role",
                Password = encriptedPassword
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }
    }
}
