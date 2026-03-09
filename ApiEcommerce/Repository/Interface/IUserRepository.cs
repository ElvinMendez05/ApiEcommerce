using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;

namespace ApiEcommerce.Repository.Interface
{
    public interface 
        IUserRepository
    {
        ICollection<User> GetUsers();
        User? GetUser(int id);
        bool IsUniqueUser(string username);
        Task<UserLoginResponseDto> Login(UserLoginDto loginResponse);
        Task<User> Register(CreateUserDto registerUser);
    }
}
