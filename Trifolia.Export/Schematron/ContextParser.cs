using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trifolia.Export.Schematron.Model;

namespace Trifolia.Export.Schematron
{
    public class ContextParser
    {
        private string _context;

        public ContextParser(string aContext)
        {
            _context = aContext;

            if (_context == null)
                _context = string.Empty;
        }

        private bool IsAttribute(string aName)
        {
            return aName.Contains("@");
        }

        public void Parse(out DocumentTemplateElement aContextElement, out DocumentTemplateElementAttribute aAttribute)
        {
            aContextElement = null; //default
            aAttribute = null; //default

            var parsedContext = _context.Split('/');
            if (parsedContext.Length > 1)   //does the context contain a complex element structure (e.g. code/@code)
            {
                DocumentTemplateElement parentContextElement = null;
                for (int i = 0; i < parsedContext.Length; i++)
                {
                    if (IsAttribute(parsedContext[i]))
                    {
                        aAttribute = new DocumentTemplateElementAttribute(parsedContext[i].Replace("@", ""));
                        aContextElement.AddAttribute(aAttribute);
                    }
                    else
                    {
                        aContextElement = new DocumentTemplateElement(parsedContext[i]);
                        if (parentContextElement != null)
                        {
                            parentContextElement.AddElement(aContextElement);
                        }
                    }
                    parentContextElement = aContextElement;
                }
            }
            else
            {
                if (IsAttribute(_context))
                {
                    aAttribute = new DocumentTemplateElementAttribute(_context.Replace("@", ""));
                }
                else
                {
                    aContextElement = new DocumentTemplateElement(_context);
                }
            }

        }

    }
}
