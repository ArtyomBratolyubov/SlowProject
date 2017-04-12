using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ModelsLib;
using Newtonsoft.Json;
using ORM;

namespace Service
{
    public class ToDoService
    {
        private readonly EntityModel dataBase = new EntityModel();

        /// <summary>
        /// The service URL.
        /// </summary>
        private readonly string serviceApiUrl = ConfigurationManager.AppSettings["ToDoServiceUrl"];

        /// <summary>
        /// The url for getting all todos.
        /// </summary>
        private const string GetAllUrl = "ToDos?userId={0}";

        /// <summary>
        /// The url for updating a todo.
        /// </summary>
        private const string UpdateUrl = "ToDos";

        /// <summary>
        /// The url for a todo's creation.
        /// </summary>
        private const string CreateUrl = "ToDos";

        /// <summary>
        /// The url for a todo's deletion.
        /// </summary>
        private const string DeleteUrl = "ToDos/{0}";

        private readonly HttpClient httpClient;

        private Task synTask;

        private bool loading = false;

        private static int i = 0;

        /// <summary>
        /// Creates the service.
        /// </summary>
        public ToDoService()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        }

        /// <summary>
        /// Gets all todos for the user.
        /// </summary>
        /// <param name="userId">The User Id.</param>
        /// <returns>The list of todos.</returns>
        public void InitDataBase(int userId)
        {

            if (dataBase.Set<Todo>().Any())
            {
                if (dataBase.Set<Todo>().Any(m => m.CloudId == 0) && i == 0)
                {
                    SyncIds(userId);
                }
                i++;

                return;
            }

            var data = GetAllItemsCloud(userId);

            data = data.ToList();
            foreach (var todo in data)
            {
                todo.Name = todo.Name.Replace(" ", string.Empty);
                todo.IsSync = true;
            }
            i++;
            if (i == 1)
                dataBase.Set<Todo>().AddRange(data);

            SaveDataBase();

            //return JsonConvert.DeserializeObject<IList<ToDoItemViewModel>>(dataAsString);
        }

        public IEnumerable<Todo> GetAllItemsCloud(int userId)
        {

            var dataAsString = httpClient.GetStringAsync(string.Format(serviceApiUrl + GetAllUrl, userId)).Result;

            return JsonConvert.DeserializeObject<IList<ToDoItemViewModel>>(dataAsString)
                .ToList().Select(m => m.ToTodo());
        }

        public IEnumerable<Todo> GetAllItemsDataBase(int userId)
        {
            return dataBase.Set<Todo>().ToList();
        }

        /// <summary>
        /// Creates a todo. UserId is taken from the model.
        /// </summary>
        /// <param name="item">The todo to create.</param>
        public int CreateItem(ToDoItemViewModel item)
        {
            Todo obj = item.ToTodo();

            dataBase.Set<Todo>().Add(obj);

            SaveDataBase();

            return obj.Id;
        }

        /// <summary>
        /// Updates a todo.
        /// </summary>
        /// <param name="item">The todo to update.</param>
        public void UpdateItem(ToDoItemViewModel item)
        {
            var obj = dataBase.Set<Todo>().FirstOrDefault(m => m.Id == item.ToDoId || m.CloudId == item.ToDoId);

            if ((obj.CloudId == null || obj.CloudId == 0) && obj.IsSync)
            {
                SyncIds(item.UserId);
            }

            obj = dataBase.Set<Todo>().FirstOrDefault(m => m.Id == item.ToDoId || m.CloudId == item.ToDoId);
            obj.IsCompleted = item.IsCompleted;
            obj.Name = item.Name;
            dataBase.Entry(obj).State = System.Data.Entity.EntityState.Modified;

            SaveDataBase();

            item.ToDoId = obj.CloudId.Value;

            new Task(() =>
            {
                httpClient.PutAsJsonAsync(serviceApiUrl + UpdateUrl, item)
                .Result.EnsureSuccessStatusCode();
            }).Start();
        }

        /// <summary>
        /// Deletes a todo.
        /// </summary>
        /// <param name="id">The todo Id to delete.</param>
        public void DeleteItem(int id, int userId)
        {
            var obj = dataBase.Set<Todo>().FirstOrDefault(m => m.Id == id || m.CloudId == id);
            if (obj == null)
                return;

            if ((obj.CloudId == null || obj.CloudId == 0) && obj.IsSync)
            {
                SyncIds(userId);
            }


            obj = dataBase.Set<Todo>().FirstOrDefault(m => m.Id == id || m.CloudId == id);
            dataBase.Set<Todo>().Remove(obj);

            if (obj.IsSync)
            {
                new Task(() =>
                {
                    httpClient.DeleteAsync(string.Format(serviceApiUrl + DeleteUrl, obj.CloudId))
                        .Result.EnsureSuccessStatusCode();
                }).Start();
            }


            SaveDataBase();
        }


        public void SyncObj(int id)
        {
            var ob = dataBase.Set<Todo>().FirstOrDefault(m => m.Id == id);

            httpClient.PostAsJsonAsync(serviceApiUrl + CreateUrl, ob).Result.EnsureSuccessStatusCode();

            ob = dataBase.Set<Todo>().FirstOrDefault(m => m.Id == id);
            if (ob == null)
                return;

            ob.IsSync = true;

            dataBase.Entry(ob).State = System.Data.Entity.EntityState.Modified;

            SaveDataBase();
        }

        void SyncIds(int userId)
        {
            var dataCloud = GetAllItemsCloud(userId);

            IEnumerable<Todo> dataDb = dataBase.Set<Todo>();

            int synCount = dataDb.Count(m => m.CloudId != 0);

            dataCloud = dataCloud.Skip(synCount).ToList();

            dataDb = dataDb.Skip(synCount);

            //foreach (var todo in dataDb)
            //{
            //    todo.CloudId = todo.Id;
            //    dataBase.Entry(todo).State = System.Data.Entity.EntityState.Modified;
            //}

            for (int i = 0; i < dataCloud.Count(); i++)
            {
                dataDb.ElementAt(i).CloudId = dataCloud.ElementAt(i).Id;

                dataBase.Entry(dataDb.ElementAt(i)).State = System.Data.Entity.EntityState.Modified;
            }


            /*var dataCloud = GetAllItemsCloud(userId);

            IEnumerable<Todo> dataDb = dataBase.Set<Todo>();

            int synCount = dataDb.Count(m => m.IsIdSync);

            dataCloud = dataCloud.Skip(synCount);

            dataDb = dataDb.Skip(synCount);

            for (int i = 0; i < dataCloud.Count(); i++)
            {
                dataDb.ElementAt(i).Id = dataCloud.ElementAt(i).Id;
                dataDb.ElementAt(i).IsIdSync = true;

                dataBase.Entry(dataDb.ElementAt(i)).State = System.Data.Entity.EntityState.Modified;
            }*/

            SaveDataBase();
        }

        private void SaveDataBase()
        {
            try
            {
                dataBase.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Debug.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
        }
    }
}
