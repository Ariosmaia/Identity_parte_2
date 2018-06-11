using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

//Atributos que irão aparecer no formulário
namespace ByteBank.Forum.ViewModels
{
    public class ContaRegistrarViewModel
    {
        [Required] //Obrigatorio
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Nome Completo")]// Informação que mostra para o usuário
        public string NomeCompleto { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Senha { get; set; }
    }
}