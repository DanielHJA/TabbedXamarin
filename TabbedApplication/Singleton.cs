using System;
namespace TabbedApplication
{
    public class Singleton
    {

        public static Singleton Shared = new Singleton();

        private Singleton()
        {
        }

        static internal Singleton Instance() {
            return Shared;
        }

        public string DoSomeDatabaseInteraction() {
            return "Database interction completed";
        }
    }
}
