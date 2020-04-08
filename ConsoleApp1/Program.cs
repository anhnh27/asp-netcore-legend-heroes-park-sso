using IdentityServer4.Models;
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Guid g = Guid.NewGuid();

            var c = GrantTypes.Code;

            var x = new Secret("3d73bc46-d313-4155-8074-8cb1c13ada03".Sha256()).Value;
            Console.WriteLine((new Secret("1cb5160c-93b2-4aad-ab55-28130c96208f".Sha256())).Value);
        }
    }
}
