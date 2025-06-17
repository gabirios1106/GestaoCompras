using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoCompras.DTO.Access
{
    public class RefreshTokenGetDTO
    {
        public string PasswordHash { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public bool Revalidate { get; set; }

        public RefreshTokenGetDTO() { }

        public RefreshTokenGetDTO(string passwordHash, string token, string refreshToken, bool revalidate = true)
        {
            PasswordHash = passwordHash;
            Token = token;
            RefreshToken = refreshToken;
            Revalidate = revalidate;
        }

        public RefreshTokenGetDTO(string refreshToken)
        {
            PasswordHash = string.Empty;
            RefreshToken = refreshToken;
            Revalidate = false;
        }

    }
}
