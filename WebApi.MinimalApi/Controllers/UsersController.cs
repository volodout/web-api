using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
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
    [HttpHead("{userId}")]
    [Produces("application/json", "application/xml")]
    public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
    {
        var userEntity = userRepository.FindById(userId);
        if (userEntity == null)
            return NotFound();
        var userDto = mapper.Map<UserDto>(userEntity);
        Response.ContentType = "application/json; charset=utf-8";
        if (HttpMethods.IsHead(Request.Method))
        {
            return Ok();
        }
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

    [HttpPut("{userId}")]
    [Consumes("application/json")]
    [Produces("application/json", "application/xml")]
    public IActionResult UpdateUser([FromRoute] Guid userId, [FromBody] UserPutDto user)
    {
        if (user == null || userId == Guid.Empty)
            return BadRequest();
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);
        var userEntity = mapper.Map(user, new UserEntity(userId));
        userRepository.UpdateOrInsert(userEntity, out var isInserted);
        if (isInserted)
            return CreatedAtAction(
                actionName: nameof(GetUserById),
                routeValues: new { userId },
                value: userId);

        return NoContent();
    }
    
    [HttpPatch("{userId}", Name = nameof(PatchUser))]
    [Consumes("application/json")]
    [Produces("application/json", "application/xml")]
    public IActionResult PatchUser([FromRoute] Guid userId, [FromBody] JsonPatchDocument<UserPatchDto>? patchDoc)
    {
        if (patchDoc is null)
            return BadRequest();
        if (userId == Guid.Empty)
            return NotFound();
        var userEntity = userRepository.FindById(userId);
        if (userEntity is null)
            return NotFound();
        var userPatch = new UserPatchDto
        {
            Login = userEntity.Login,
            FirstName = userEntity.FirstName,
            LastName = userEntity.LastName
        };
        patchDoc.ApplyTo(userPatch, ModelState);
        if (string.IsNullOrEmpty(userPatch.Login))
            ModelState.AddModelError("login", "login is required");
        else if (!userPatch.Login.All(char.IsLetterOrDigit))
            ModelState.AddModelError("login", "login must be alphanumeric");
        if (userPatch.LastName is null || userPatch.LastName.Length == 0)
            ModelState.AddModelError("lastName", "length of the last name must be greater than 0");
        if (userPatch.FirstName is not null && userPatch.FirstName.Length == 0)
            ModelState.AddModelError("firstName", "length of the first name must be greater than 0");
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);
        if (userPatch.Login != null)
            userEntity.Login = userPatch.Login;
        if (userPatch.FirstName != null)
            userEntity.FirstName = userPatch.FirstName;
        if (userPatch.LastName != null)
            userEntity.LastName = userPatch.LastName;
        userRepository.Update(userEntity);
        return NoContent();
    }

    [HttpDelete("{userId}", Name = nameof(DeleteUser))]
    public IActionResult DeleteUser([FromRoute] Guid userId)
    {
        if (userId == Guid.Empty)
            return NotFound();
        var userEntity = userRepository.FindById(userId);
        if (userEntity is null)
            return NotFound();
        userRepository.Delete(userId);
        return NoContent();
    }
}