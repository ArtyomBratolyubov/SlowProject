using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ModelsLib;
using Service;

namespace SyncApp.Controllers
{
    public class DataController : Controller
    {
        private readonly ToDoService todoService = new ToDoService();
        private readonly UserService userService = new UserService();

        public void Index()
        {
            int UserId = userService.GetOrCreateUser();

        }

        [HttpGet]
        public int InitDataBase(int? userId)
        {
            todoService.InitDataBase(userId.Value);

            return  0;
        }

        [HttpPost]
        public void Create(ToDoItemViewModel model)
        {
            //model.UserId = userService.GetOrCreateUser();
            todoService.CreateItem(model);
        }

        [HttpPost]
        public void Delete(ToDoItemViewModel model)
        {
            todoService.DeleteItem(model.ToDoId, model.UserId);
        }
    }
}