public class UpdateUserService : IUpdateUserService
{

    private readonly RecepcionDbContext _context;
    private readonly IEncryptService _encryptService;

    public UpdateUserService(RecepcionDbContext context, IEncryptService encryptService)
    {
        _context = context;
        _encryptService = encryptService;
    }
    public async Task<User?> UpdateUser(int id, UpdateUserRequestDto updateUserRequestDto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return null;
        }

        user.Name = updateUserRequestDto.Name;
        user.UserName = updateUserRequestDto.UserName;
        user.Email = updateUserRequestDto.Email;
        user.Role = updateUserRequestDto.Role;
        user.Status = updateUserRequestDto.Status;
        user.EmployeeId = updateUserRequestDto.EmployeeId;

        if (updateUserRequestDto.Password.Length > 0)
        {
            var keyPwh = _encryptService.HashPassword(updateUserRequestDto.Password);
            var salt = keyPwh.Split(":")[0];
            var password = keyPwh.Split(":")[1];

            user.Password = password;
            user.HashPassword = salt;
        }

        await _context.SaveChangesAsync();
        return user;
    }
}