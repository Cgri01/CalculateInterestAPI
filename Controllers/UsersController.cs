using Microsoft.AspNetCore.Mvc;
using FaizHesaplamaAPI.Data;
using FaizHesaplamaAPI.Models;
using FaizHesaplamaAPI.Dtos;
using FaizHesaplamaAPI.Services;
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
        private readonly IEmailService _emailService;
        private readonly IVerificationService _verificationService; 

        public UsersController(AppDbContext context, IConfiguration config , IEmailService emailService , IVerificationService verificationService)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
            _verificationService = verificationService;
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

        //POST USER REGISTER WITH EMAIL VERIFICATION CODE



        [HttpPost("request-verification")]
        public async Task<IActionResult> RequestVerificationCode([FromBody] VerificationRequestDto request)
        {
            if (!IsValidEmail(request.email)) {
                return BadRequest(new {message = "Invalid Email format" });
            }

            if (await _context.Users.AnyAsync(u => u.email == request.email))
            {
                return BadRequest(new { message = "Bu email zaten kullanımda" });
            }

            var verificationCode = _verificationService.GenerateVerificationCode();
            _verificationService.StoreVerificationCode(request.email, verificationCode);

            await _emailService.SendVerificationCodeAsync(request.email, verificationCode);
            return Ok(new { message = "Doğrulama Kodu email adresinize gönderildi" });

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto register)
        {
            if (!_verificationService.VerifyCode(register.email, register.VerificationCode)) // Use the injected _verificationService instance
            {
                return BadRequest(new { message = "Geçersiz veya süresi dolmuş doğrulama kodu" });
            }
            if (await _context.Users.AnyAsync(u => u.email == register.email))
            {
                return BadRequest(new { message = "Bu email zaten kullanımda" });
            }
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(register.password);
            var user = new Users
            {
                name = register.name,
                surname = register.surname,
                email = register.email,
                passwordHash = hashedPassword,
                role = "User", // Default 
                emailVerified = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Kayıt başarılı" });
        }
        
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public class VerificationRequestDto
        {
            public string email { get; set; }
        }



        ////POST USER REGISTER IF USER EXIST OR NOT
        //[HttpPost("register")]
        //public async Task<ActionResult> Register(RegisterDto register)
        //{
        //    if (!IsValidEmail(register.email))
        //    {
        //        return BadRequest(new { message = "Invalid Email format" });

        //    }

        //    // Check if email already exist
        //    if (await _context.Users.AnyAsync(u => u.email == register.email))
        //    {
        //        return BadRequest(new { message = "Email already in use!" });

        //    }

        //    string hashedPasword = BCrypt.Net.BCrypt.HashPassword(register.password);
        //    var user = new Users
        //    {
        //        name = register.name,
        //        surname = register.surname,
        //        email = register.email,
        //        passwordHash = hashedPasword,
        //        role = ""
        //    };
        //    _context.Users.Add(user);
        //    await _context.SaveChangesAsync();
        //    return Ok(new { message = "Register Successfull" });

        //}

        //private bool IsValidEmail(string email)
        //{
        //    try
        //    {
        //        var addr = new System.Net.Mail.MailAddress(email);
        //        return addr.Address == email;

        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}




        // POST USER (Register):

        //[HttpPost("register")]
        //public async Task<ActionResult> Register(RegisterDto register)
        //{
        //    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(register.password);
        //    var user = new Users
        //    {
        //        name = register.name,
        //        surname = register.surname,
        //        email = register.email,
        //        passwordHash = hashedPassword, //sifrelenmis sifreyi passwordHash'e atamak
        //        role = "User"
        //    };
        //    _context.Users.Add(user);
        //    await _context.SaveChangesAsync();
        //    return Ok(new { message = "Register successfull" });
        //}

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
                expires: DateTime.Now.AddHours(3), //Token 3 saat geçerli olacak
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
        //PUT UPDATE PASSWORD BY EMAIL:
        [HttpPut("updatePassword")]
        public async Task<ActionResult> UpdatePassword(UpdatePasswordDto update)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.email == update.email);
            var userId = user?.id;
            if (userId == null)
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

        //PUT UPDATE PASSWORD: 
        //[HttpPut("updatePassword")]
        //public async Task<ActionResult> UpdatePassword(UpdatePasswordDto update)
        //{
        //    var user = await _context.Users.FindAsync(update.id);
        //    if (user == null)
        //    {
        //        return NotFound("Kullanici bulunamadi!");
        //    }
        //    if (!BCrypt.Net.BCrypt.Verify(update.oldPassword, user.passwordHash))
        //    {
        //        return BadRequest("Eski sifre yanlis!");
        //    }
        //    user.passwordHash = BCrypt.Net.BCrypt.HashPassword(update.newPassword);
        //    await _context.SaveChangesAsync();
        //    return Ok(new { message = "Sifre guncellendi!" });
        //}

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
