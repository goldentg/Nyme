using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure
{
   public class Servers
    {
        private readonly brainKillerContext _context;


        public Servers(brainKillerContext context)
        {
            _context = context;
        }
    }
}
