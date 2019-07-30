
using System.IO;

namespace EmployeeWebServerApp {
  /*
   * to test BasicWebServer enter 'http://localhost:8080/' in a browser's address box
   */
  internal class Program {
    public static void Main(string[] args) {
      string currentDir = Directory.GetCurrentDirectory();
      
      string serverBaseFolder = Path.Combine(currentDir, "html");
      string databasePath = Path.Combine(currentDir, "database", "AjaxFruitsDB.db");
      Handlers handlers = new Handlers(serverBaseFolder,databasePath,"Employees");
      handlers.StartServer();
    }
  }
}