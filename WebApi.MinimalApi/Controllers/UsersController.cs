using Microsoft.AspNetCore.Mvc;
using WebApi.MinimalApi.Domain;
using WebApi.MinimalApi.Models;

namespace WebApi.MinimalApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : Controller
{
    private readonly IUserRepository userRepository;

    // Чтобы ASP.NET положил что-то в userRepository требуется конфигурация
    public UsersController(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    [HttpGet("{userId}")]
    public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
    {
        var userEntity = userRepository.FindById(userId);
        if (userEntity == null)
            return NotFound();
        var userDto = ConvertToDto(userEntity);
        return Ok(userDto);
    }

    private UserDto ConvertToDto(UserEntity userEntity)
    {
        return new UserDto
        {
            Id = userEntity.Id,
            Login = userEntity.Login,
            FullName = $"{userEntity.LastName} {userEntity.FirstName}",
            GamesPlayed = userEntity.GamesPlayed,
            CurrentGameId = userEntity.CurrentGameId
        };
    }

    [HttpPost]
    public IActionResult CreateUser([FromBody] object user)
    {
        throw new NotImplementedException();
    }
}