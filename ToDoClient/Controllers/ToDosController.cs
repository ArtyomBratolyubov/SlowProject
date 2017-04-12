using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using ModelsLib;
using Service;


namespace ToDoClient.Controllers
{
    /// <summary>
    /// Processes todo requests.
    /// </summary>
    public class ToDosController : ApiController
    {
        private readonly ToDoService todoService = new ToDoService();
        private readonly UserService userService = new UserService();

        /// <summary>
        /// Returns all todo-items for the current user.
        /// </summary>
        /// <returns>The list of todo-items.</returns>
        public IEnumerable<ToDoItemViewModel> Get()
        {
            int UserId = userService.GetOrCreateUser();

            WebRequest webRequest = WebRequest.Create("http://localhost:50433/Data/InitDataBase/?userId=" +
                userService.GetOrCreateUser());
            WebResponse webResp = webRequest.GetResponse();

            return todoService.GetAllItemsDataBase(UserId).Select(m => m.ToMvc());
        }

    }
}
