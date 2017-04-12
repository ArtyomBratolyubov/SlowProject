using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelsLib;
using ORM;

namespace Service
{
    public static class Mapper
    {
        public static Todo ToTodo(this ToDoItemViewModel obj)
        {
            return new Todo()
            {
                Id = obj.ToDoId,
                CloudId = obj.ToDoId,
                Name = obj.Name,
                IsCompleted = obj.IsCompleted,
                UserId = obj.UserId
            };
        }

        public static ToDoItemViewModel ToMvc(this Todo obj)
        {
            return new ToDoItemViewModel()
            {
                ToDoId = obj.CloudId !=0 ? obj.CloudId.Value : obj.Id,
                Name = obj.Name,
                IsCompleted = obj.IsCompleted,
                UserId = obj.UserId
            };
        }
    }
}
