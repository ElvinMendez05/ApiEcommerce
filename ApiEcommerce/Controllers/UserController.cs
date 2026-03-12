using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = _userRepository.GetUsers();
            var usersDto = _mapper.Map<List<UserDto>>(users);
            return Ok(usersDto);
        }

        [HttpGet("{userId:int}", Name = "GetUser")]
        public IActionResult GetUser(int userId)
        {
            var user = _userRepository.GetUser(userId);
            if (user == null)
            {
                return NotFound($"The user with {userId} it doesn't exist");
            }
            var userDto = _mapper.Map<UserDto>(user);
            return Ok(userDto);
        }

        [AllowAnonymous]
        [HttpPost("RegisterUser")]
        public async Task<IActionResult> RegisterUser([FromBody] CreateUserDto createUserDto)
        {
            if (createUserDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (string.IsNullOrWhiteSpace(createUserDto.Username))
            {
                return BadRequest("Username is required");
            }

            if (!_userRepository.IsUniqueUser(createUserDto.Username))
            {
                return BadRequest("The user is already in the databse");
            }

            var result = await _userRepository.Register(createUserDto);
            if (result == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al registar el usuario");
            }
            return CreatedAtRoute("GetUser", new { userId = result.Id }, result);
        }

        [AllowAnonymous]
        [HttpPost("LoginUser")]
        public async Task<IActionResult> LoginUser([FromBody] UserLoginDto userLoginDto)
        {
            if (userLoginDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
         
            var user = await _userRepository.Login(userLoginDto);
            if (user == null)
            {
                return Unauthorized();  
            }
            return Ok(user);
        }
    }
}
