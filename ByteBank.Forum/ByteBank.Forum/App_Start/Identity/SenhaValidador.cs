using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace ByteBank.Forum.App_Start.Identity
{
    //UserManager usa o PassawordValidator, usa o IdentitiyValidor tbm.
    public class SenhaValidador : IIdentityValidator<string>
    {

        public int TamanhoRequerido { get; set; }
        public bool ObrigatorioCaracteresEspeciais { get; set; }
        public bool ObrigatorioLowerCase { get; set; }
        public bool ObrigatorioUpperCase { get; set; }
        public bool ObrigatorioDigitos { get; set; }

        public async Task<IdentityResult> ValidateAsync(string item)
        {
            var erros = new List<string>();

            //Define os parametros no startup
            if(ObrigatorioCaracteresEspeciais && !VerificaCaracteresEspeciais(item))
                erros.Add("A senha deve conter caracteres especiais");

            if (!VerificaTamanhoRequerido(item))
                erros.Add($"A senha deve conter no mínimo {TamanhoRequerido} caracteres.");

            if (ObrigatorioLowerCase && !VerificaLowercase(item))
                erros.Add($"A senha deve conter no mínimo uma letra minúscula.");

            if (ObrigatorioUpperCase && !VerificaUppercase(item))
                erros.Add($"A senha deve conter no mínimo uma letra maiúscula.");

            if (ObrigatorioDigitos && !VerificaDigito(item))
                erros.Add($"A senha deve conter no mínimo um dígito.");

            //Caso tenha agluns desses erros
            if (erros.Any())
                return IdentityResult.Failed(erros.ToArray());
            else
                return IdentityResult.Success;

        }

        private bool VerificaTamanhoRequerido(string senha) =>
           senha?.Length >= TamanhoRequerido; // Será falso caso seja nula

        private bool VerificaCaracteresEspeciais(string senha) =>
            Regex.IsMatch(senha, @"[~`!@#$%^&*()+=|\\{}':;.,<>/?[\]""_-]");

        private bool VerificaLowercase(string senha) =>
            senha.Any(char.IsLower);

        private bool VerificaUppercase(string senha) =>
            senha.Any(char.IsUpper);

        private bool VerificaDigito(string senha) =>
            senha.Any(char.IsDigit);


    }
}