using Microsoft.AspNetCore.Mvc;
using FaizHesaplamaAPI.Data;
using FaizHesaplamaAPI.Models;
using FaizHesaplamaAPI.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;


namespace FaizHesaplamaAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public UsersController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        //GET ALL USERS:

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Users>>> Get()
        {
            var usersInfo = await _context.Users.ToListAsync();
            return usersInfo;
        }

        //GET USER BY ID:

        [HttpGet("{id}")]
        public async Task<ActionResult<Users>> GetUserById(int id)
        {
            var user = await _context.Users.FindAsync(id); //return user yaptıgımda passwordHash'da return ediliyor onlemek için:
            var showInfo = new
            {
                id = user.id,
                name = user.name,
                surname = user.surname,
                email = user.email,
                role = user.role
            };
            if (showInfo == null)
            {
                return NotFound();
            }
            return Ok(showInfo);
        }

        // POST USER (Register):

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDto register)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(register.password);
            var user = new Users
            {
                name = register.name,
                surname = register.surname,
                email = register.email,
                passwordHash = hashedPassword, //sifrelenmis sifreyi passwordHash'e atamak
                role = "User"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Register successfull" });
        }

        //LOGIN

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDto login)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.email == login.email);
            if (user == null) return BadRequest("Kullanıcı bulunamadı!");

            if (!BCrypt.Net.BCrypt.Verify(login.password, user.passwordHash))
                return BadRequest("Hatali sifre!!!");

            string token = CreateToken(user);
            return Ok(new { token = token });

        }

        //JWT TOKEN:
        private string CreateToken(Users user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                //new Claim(ClaimTypes.Name, user.name),
                //new Claim(ClaimTypes.Surname, user.surname),
                new Claim(ClaimTypes.Email, user.email),
                new Claim(ClaimTypes.Role, user.role)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _config.GetSection("AppSettings:Token").Value!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1), //Token 1 gün geçerli olacak
                signingCredentials: creds
                );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;

        }


        //PUT UPDATE USER:
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUser(int id, UpdateUserDto updateDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            user.name = updateDto.name;
            user.surname = updateDto.surname;
            user.email = updateDto.email;
            user.role = updateDto.role;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Kullanici bilgileri guncellendi!" });
        }

        //PUT UPDATE PASSWORD:
        [HttpPut("updatePassword")]
        public async Task<ActionResult> UpdatePassword(UpdatePasswordDto update)
        {
            var user = await _context.Users.FindAsync(update.id);
            if (user == null)
            {
                return NotFound("Kullanici bulunamadi!");
            }
            if (!BCrypt.Net.BCrypt.Verify(update.oldPassword, user.passwordHash))
            {
                return BadRequest("Eski sifre yanlis!");
            }
            user.passwordHash = BCrypt.Net.BCrypt.HashPassword(update.newPassword);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Sifre guncellendi!" });
        }

        //DELETE USER:
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("Kullanici bulunamadi!");
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Kullanici silindi!" });
        }

    }
}
