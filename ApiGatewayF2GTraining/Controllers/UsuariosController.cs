using ApiGatewayF2GTraining.Helpers;
using ApiGatewayF2GTraining.Models;
using ApiGatewayF2GTraining.Repositories;
using ModelsF2GTraining;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ApiGatewayF2GTraining.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private IRepositoryF2GTraining repo;
        private HelperOAuthToken helper;

        public UsuariosController(IRepositoryF2GTraining repo, HelperOAuthToken helper)
        {
            this.repo = repo;
            this.helper = helper;
        }

        [HttpPost]
        public async Task InsertUsuario(Usuario user)
        {
            if (!(await this.repo.CheckUsuarioRegistro(user.Nombre)) && !(await this.repo.CheckTelefonoRegistro(user.Telefono)) && !(await this.repo.CheckCorreoRegistro(user.Correo)))
            {
                await this.repo.InsertUsuario(user);
            }
        }

        [HttpPost]
        [Route(("[action]"))]
        public async Task<string> Login(LoginModel model)
        {
            Usuario user = await this.repo.GetUsuarioNamePass(model.username, model.password);
            if (user != null)
            {
                SigningCredentials credentials =
                new SigningCredentials(this.helper.GetKeyToken()
                , SecurityAlgorithms.HmacSha256);

                string jsonUser = JsonConvert.SerializeObject(user);
                Claim[] info = new[]
                {
                    new Claim("UserData", jsonUser)
                };

                JwtSecurityToken token =
                    new JwtSecurityToken(
                        claims: info,
                        issuer: this.helper.Issuer,
                        audience: this.helper.Audience,
                        signingCredentials: credentials,
                        expires: DateTime.UtcNow.AddMinutes(180),
                        notBefore: DateTime.UtcNow
                        );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            return null;

        }

        [Authorize]
        [HttpGet]
        [Route("[action]")]
        public async Task<Usuario> GetUsuarioLogueado()
        {
            Usuario user = HelperContextUser.GetUsuarioByClaim(HttpContext.User.Claims.SingleOrDefault(x => x.Type == "UserData"));
            return user;
        }

        [HttpGet]
        [Route("[action]/{telefono}")]
        public async Task<bool> TelefonoRegistrado(int telefono)
        {
            return await this.repo.CheckTelefonoRegistro(telefono);
        }

        [HttpGet]
        [Route("[action]/{nombre}")]
        public async Task<bool> NombreRegistrado(string nombre)
        {
            return await this.repo.CheckUsuarioRegistro(nombre);
        }

        [HttpGet]
        [Route("[action]/{correo}")]
        public async Task<bool> CorreoRegistrado(string correo)
        {
            return await this.repo.CheckCorreoRegistro(correo);
        }

    }
}
