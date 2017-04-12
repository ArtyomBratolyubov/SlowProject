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

        public void Index()
        {
            

        }

        [HttpGet]
        public int InitDataBase(int? userId)
        {
            todoService.InitDataBase(userId.Value);

            return 0;
        }

        [HttpPost]
        public int Create(ToDoItemViewModel model)
        {
            //model.UserId = userService.GetOrCreateUser();
            return todoService.CreateItem(model);

        }

        [HttpPost]
        public void Delete(ToDoItemViewModel model)
        {
            todoService.DeleteItem(model.ToDoId, model.UserId);
        }

        [HttpPost]
        public void SyncObj(ToDoItemViewModel model)
        {
            todoService.SyncObj(model.ToDoId);
        }

        [HttpPost]
        public void Update(ToDoItemViewModel model)
        {
            todoService.UpdateItem(model);
        }

    }
}