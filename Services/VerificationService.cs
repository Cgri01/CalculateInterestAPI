namespace FaizHesaplamaAPI.Services
{
    public interface IVerificationService
    {
        // Define methods for verification service, e.g., sending verification codes, verifying codes, etc.
        string GenerateVerificationCode();
        void StoreVerificationCode(string email, string code);
        bool VerifyCode(string email, string code);
    }
    public class VerificationService : IVerificationService
    {
        private readonly Dictionary<string, (string Code, DateTime expiry)> _verificationCodes = new();
        private readonly TimeSpan _codeExpiryDuration = TimeSpan.FromMinutes(3);

        public string GenerateVerificationCode()
        {
            return new Random().Next(100000 , 999999).ToString(); // Generates a 6-digit verification code

        }
        public void StoreVerificationCode(string email, string code)
        {
            _verificationCodes[email] = (code , DateTime.UtcNow.Add(_codeExpiryDuration));
        }

        public bool VerifyCode(string email , string code)
        {
            if (!_verificationCodes.TryGetValue(email, out var stored))
            {
                return false;
            }
            
            if (DateTime.UtcNow > stored.expiry)
            {
                _verificationCodes.Remove(email); // Remove expired code
                return false;
            }

            return stored.Code == code; // Check if the provided code matches the stored code   
        }

    }
}
