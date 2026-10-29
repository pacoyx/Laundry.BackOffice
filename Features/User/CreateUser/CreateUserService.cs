public class CreateUserService : ICreateUserService
{
    private readonly RecepcionDbContext _context;
    private readonly IEncryptService _encryptService;

    public CreateUserService(RecepcionDbContext context, IEncryptService encryptService)
    {
        _context = context;
        _encryptService = encryptService;
    }

    public async Task<User> Create(CreateUserDto userDto)
    {
        var keyPwh = _encryptService.HashPassword(userDto.Password);
        var hashPassword = keyPwh.Split(":")[0];
        var password = keyPwh.Split(":")[1];

        var user = new User()
        {
            Name = userDto.Name,
            UserName = userDto.UserName,
            Password = password,
            Role = userDto.Role,
            Status = "A",
            HashPassword = hashPassword,
            Email = userDto.Email,
            EmployeeId = userDto.EmployeeId
        };

        await _context.Users.AddAsync(user);
        _context.SaveChanges();
        return user;
    }

    public Task<bool> ExistUser(string userName)
    {
        var user = _context.Users.FirstOrDefault(x => x.UserName == userName);
        return Task.FromResult(user != null);
    }
}