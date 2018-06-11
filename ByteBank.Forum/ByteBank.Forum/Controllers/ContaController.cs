using ByteBank.Forum.Models;
using ByteBank.Forum.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ByteBank.Forum.App_Start.Identity;

//Gerenciamento da conta do usuário

namespace ByteBank.Forum.Controllers
{
    public class ContaController : Controller
    {

        private UserManager<UsuarioAplicacao> _userManager;
        public UserManager<UsuarioAplicacao> UserManager
        {
            get
            {
                if (_userManager == null)
                {
                    var contextOwin = HttpContext.GetOwinContext(); // Pega o contexto do Owin
                    _userManager = contextOwin.GetUserManager<UserManager<UsuarioAplicacao>>(); // Joga o contexto dentro da variavel
                }
                return _userManager;
            }
            set
            {
                _userManager = value;
            }
        }

        private SignInManager<UsuarioAplicacao, string> _signInManager;
        private SignInManager<UsuarioAplicacao, string> SignInManager
        {
            get
            {
                if (_signInManager == null)
                {
                    var contextOwin = HttpContext.GetOwinContext(); // Pega o contexto do Owin
                    _signInManager = contextOwin.GetUserManager<SignInManager<UsuarioAplicacao, string> > (); // Joga o contexto dentro da variavel
                }
                return _signInManager;
            }
            set
            {
                _signInManager = value;
            }
        }

        /////////////////////////////////////
        // GET: /Account/Login/Acessar a página
        public ActionResult Registrar()
        {
            return View();
        }

        // POST: /Account/Register/Envia as informações
        [HttpPost]
        public async Task<ActionResult> Registrar(ContaRegistrarViewModel modelo)
        {
            //Verifica se o estado do nosso modelo é valido ou não.
            if (ModelState.IsValid)
            {
                // IdentUser já possui Email, UserName, entre outros, dentro das interfaces. Veja no F12.
                // IdentityDbContext usa uma tabela de IdentityUser, somos obrigados a usa-lá. Ele já faz o DbSet.
                var novoUsuario = new UsuarioAplicacao();

                novoUsuario.Email = modelo.Email;
                novoUsuario.UserName = modelo.UserName;
                novoUsuario.NomeCompleto = modelo.NomeCompleto;



                var usuario = await UserManager.FindByEmailAsync(modelo.Email);//Busca um usuário que já existe.
                var usuarioJaExiste = usuario != null;// Diferente de null já foi cadastrado. Não devemos vazar dos dados do usuario

                //Redireciona o usuario para não mostrar os dados
                if (usuarioJaExiste)
                    return View("AguardandoConfirmacao");

                var resultado = await UserManager.CreateAsync(novoUsuario, modelo.Senha);
                // Adiciona e salva no lugar do add e save chanches do Entity.


                //CreatedAsync tem tem o IdentityResult, que possui uma propriedade que tem um bool de Succeed, ou não
                if (resultado.Succeeded)// Verifica se o resultado do Created teve sucesso
                {
                    // Enviar o email de confirmação
                    await EnviarEmailDeConfirmacaoAsync(novoUsuario);
                    return View("AguardandoConfirmacao");
                }
                else
                {
                    // Metodo criado abaixo, para adicinae erros.
                    //Metodo erros
                    AdicionaErros(resultado);
                }
            }

            // Alguma coisa de errado aconteceu!
            return View(modelo);
        }

        private async Task EnviarEmailDeConfirmacaoAsync(UsuarioAplicacao usuario)
        {
            //Cria token
            var token = await UserManager.GenerateEmailConfirmationTokenAsync(usuario.Id);

            var linkDeCallback =
                Url.Action(
                    "ConfirmacaoEmail", //Criei Action abaixo
                    "Conta",
                    new { usuarioId = usuario.Id, token = token },
                    protocol: Request.Url.Scheme);

            await UserManager.SendEmailAsync(
                usuario.Id,
                "Fórum ByteBank - Confirmação de Email",
                $"Bem vindo ao fórum ByteBank, clique aqui {linkDeCallback} para confirmar seu email!");

        }

        // Action de confirmação de email
        public async Task<ActionResult> ConfirmacaoEmail(string usuarioId, string token)
        {
            //Não pode ser null
            if (usuarioId == null || token == null)
                return View("Error");// View que vem como padrão

            var resultado = await UserManager.ConfirmEmailAsync(usuarioId, token); //Resultado

            if (resultado.Succeeded)// Usar resultado e confirmar
                return RedirectToAction("Index", "Home");
            else
                return View("Error");
        }

        public async Task<ActionResult> Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(ContaLoginViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                var usuario = await UserManager.FindByEmailAsync(modelo.Email);

                if (usuario == null)
                    return SenhaOuUsuarioInvalidos();
                

               
                var signInResultado = 
                    await SignInManager.PasswordSignInAsync(
                        usuario.UserName,
                        modelo.Senha,
                        isPersistent: false,
                        shouldLockout: false);

                switch (signInResultado)
                {
                    case SignInStatus.Success:
                        return RedirectToAction("Index", "Home");
                    default:
                        return SenhaOuUsuarioInvalidos();
                            
                }
            }
            
            
            //Algo de errado aconteceu
            return View(modelo);
        }

        private ActionResult SenhaOuUsuarioInvalidos()
        {
            ModelState.AddModelError("", "Credenciais invalidas!");
            return View("Login");
        }


        private void AdicionaErros(IdentityResult resultado)
        {
            //Metodo erros
            // Para cada erro encontrado no resultao
            // Usa a prorpriedade de "Errors"
            foreach (var erro in resultado.Errors)
                ModelState.AddModelError("", erro);
            //Adiciona um erro com modelState.
        }
    }
}