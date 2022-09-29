namespace api.Controllers;

public class AccountController : BaseApiController
{
  private readonly UserManager<AppUser> _userManager;
  private readonly SignInManager<AppUser> _signInManager;
  private readonly ITokenService _tokenService;
  private readonly IMapper _mapper;
  public AccountController(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    ITokenService tokenService, IMapper mapper)
  {
    _mapper = mapper;
    _tokenService = tokenService;
    _userManager = userManager;
    _signInManager = signInManager;
  }

  [HttpPost("register")]
  public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
  {
    if (await UserExists(registerDto.Username)) return BadRequest("Username is Taken");

    var user = _mapper.Map<AppUser>(registerDto);

    user.UserName = registerDto.Username.ToLower();

    var result = await _userManager.CreateAsync(user, registerDto.Password);

    if (!result.Succeeded) return BadRequest(result.Errors);

    var roleResult = await _userManager.AddToRoleAsync(user, "Member");

    if (!roleResult.Succeeded) return BadRequest(roleResult.Errors);

    return new UserDto
    {
      Username = user.UserName,
      Token = await _tokenService.CreateToken(user),
      KnownAs = user.KnownAs,
      Gender = user.Gender
    };
  }

  [HttpPost("login")]
  public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
  {

    var user = await _userManager.Users
    .Include(p => p.Photos)
    .SingleOrDefaultAsync(u => u.UserName == loginDto.Username.ToLower());

    if (user == null) return Unauthorized("invalid user/password");

    var result = await _signInManager
    .CheckPasswordSignInAsync(user, loginDto.Password, false);

    if (!result.Succeeded) return Unauthorized("invalid user/password");

    return new UserDto
    {
      Username = user.UserName,
      Token = await _tokenService.CreateToken(user),
      PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
      KnownAs = user.KnownAs,
      Gender = user.Gender
    };
  }

  private async Task<bool> UserExists(string username)
  {
    return await _userManager.Users.AnyAsync(u => u.UserName == username.ToLower());
  }
}
