using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JetBrains.Annotations;
using Orchard.Data;
using Mycompany.Helloworld.Models;

namespace Mycompany.Helloworld.Services
{
    [UsedImplicitly]
    public class TextService:ITextService
    {
        private readonly IRepository<TextRecord> _textRepository;

        /// <summary>
        /// 通过构造函数传入Repository
        /// </summary>
        /// <param name="textRepository"></param>
        public TextService(IRepository<TextRecord> textRepository)
        {
            _textRepository = textRepository;
        }

        public TextRecord GetText()
        {
            return _textRepository.Table.FirstOrDefault();
        }

        public TextRecord UpdateText(string content)
        {
            var result = GetText();
            if (result == null)
            {
                result = new TextRecord { Content = content };
                _textRepository.Create(result);
            }
            else
            {
                result.Content = content;
                _textRepository.Update(result);
            }
            return result;
        }
    }
}