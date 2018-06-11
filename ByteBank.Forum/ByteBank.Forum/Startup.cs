using ByteBank.Forum.App_Start.Identity;
using ByteBank.Forum.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

//Owin é um protocolo de comunicação entre aplicação e o servidor
//É executado sempre que tem uma requisição

[assembly: OwinStartup(typeof(ByteBank.Forum.Startup))]
//Atributo que define qual a classe de inicialização do Owin

namespace ByteBank.Forum
{
    public class Startup
    {
        public void Configuration(IAppBuilder builder)
        {
            //1)Definindo o DbContext => IdentityDbContext do Usuário Aplicação + Nome da Connetion String
            builder.CreatePerOwinContext<DbContext>(() =>
                new IdentityDbContext<UsuarioAplicacao>("DefaultConnection"));// Busca a connection string no webconfig


            //2)
            //UseStore gera a interface entre o Identity e o banco de dados, usa a IUserStore para não depender do EF.
            //Manipula os usuários dentro IdentityFramework
            //IUserStore separa as camadas
            builder.CreatePerOwinContext<IUserStore<UsuarioAplicacao>>(
                (opcoes, contextoOwin) =>
                {
                    var dbContext = contextoOwin.Get<DbContext>(); // Pega o DbContext que criei lá em cima
                    return new UserStore<UsuarioAplicacao>(dbContext); // Retorna o UserStore da Aplicação + DbContext
                });


            //3)
            //UserManager gerencia o Identity, o UserStore faz o link para o Entity. Relacionado ao cadsatro de usuario.
            builder.CreatePerOwinContext<UserManager<UsuarioAplicacao>>(
                (opcoes, contextoOwin) =>
                {
                    var userStore = contextoOwin.Get<IUserStore<UsuarioAplicacao>>(); //Pega a UserStore criada
                    var userManager = new UserManager<UsuarioAplicacao>(userStore); // Cria nosso UserManager


                    //Criando validação de usuários
                    // UserValidor + UsuuarioAPlicação + UsurManager
                    var userValidator = new UserValidator<UsuarioAplicacao>(userManager);
                    userValidator.RequireUniqueEmail = true;// Retorna os emails unicos na base de dados


                    // Atribui o UserValidator no UserManeger
                    userManager.UserValidator = userValidator;


                    //SenhaValidador
                    //Definindo os valores da classe
                    userManager.PasswordValidator = new SenhaValidador()
                    {
                        TamanhoRequerido = 6,
                        ObrigatorioCaracteresEspeciais = true,
                        ObrigatorioDigitos = true,
                        ObrigatorioLowerCase = true,
                        ObrigatorioUpperCase = true
                    };


                    // Serviço de e-mail
                    userManager.EmailService = new EmailServico();

                    // Provedor de obejtos que oferece proteção para os dados
                    var dataProtectionProvider = opcoes.DataProtectionProvider;
                    // Implmenta um obejto Idataprotector. String com o nome da aplicação
                    var dataProtectionProviderCreated = dataProtectionProvider.Create("ByteBank.Forum");
                    
                    //Criando atavés das opções do Owin, tada  vez que precisa de um token ele será gerado
                    userManager.UserTokenProvider = new DataProtectorTokenProvider<UsuarioAplicacao>(dataProtectionProviderCreated);

                    return userManager;
                });

            builder.CreatePerOwinContext<SignInManager<UsuarioAplicacao, string>>(
                (opcoes, contextoOwin) =>
                {
                    var userManager = contextoOwin.Get<UserManager<UsuarioAplicacao>>();

                    var signInManager =
                        new SignInManager<UsuarioAplicacao, string>(
                            userManager,
                            contextoOwin.Authentication);

                    return signInManager;
                });

            builder.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie
            });
                
         }
    }
}