using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard;
using Mycompany.Helloworld.Models;

namespace Mycompany.Helloworld.Services
{
    public interface ITextService:IDependency
    {
        TextRecord GetText();

        TextRecord UpdateText(string content);
    }
}
