using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace MyCompany.Helloworld.ViewModels
{
    public class EditTextViewModel
    {
        [Required, DisplayName("Hello World Text")]
        public string Content { get; set; }
    }
}