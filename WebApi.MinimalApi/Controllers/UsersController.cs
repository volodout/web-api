using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApi.MinimalApi.Domain;
using WebApi.MinimalApi.Models;

namespace WebApi.MinimalApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : Controller
{
    private readonly IUserRepository userRepository;
    private readonly IMapper mapper;

    // Чтобы ASP.NET положил что-то в userRepository требуется конфигурация
    public UsersController(IUserRepository userRepository, IMapper mapper)
    {
        this.userRepository = userRepository;
        this.mapper = mapper;
    }

    [HttpGet("{userId}", Name = nameof(GetUserById))]
    [Produces("application/json", "application/xml")]
    public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
    {
        var userEntity = userRepository.FindById(userId);
        if (userEntity == null)
            return NotFound();
        var userDto = mapper.Map<UserDto>(userEntity);
        return Ok(userDto);
    }

    [HttpPost]
    [Consumes("application/json")]
    [Produces("application/json", "application/xml")]
    public IActionResult CreateUser([FromBody] UserPostDto user)
    {
        if (user == null)
            return BadRequest();

        if (string.IsNullOrEmpty(user.Login))
            ModelState.AddModelError("login", "login is required");
        else if (!user.Login.All(char.IsLetterOrDigit))
            ModelState.AddModelError("login", "login must be alphanumeric");

        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(ModelState);
        }

        var userEntity = mapper.Map<UserEntity>(user);
        userEntity = userRepository.Insert(userEntity);
        return CreatedAtRoute(
            nameof(GetUserById),
            new { userId = userEntity.Id },
            userEntity.Id);
    }
}