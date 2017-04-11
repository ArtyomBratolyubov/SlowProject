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
                return;
            }

            var data = GetAllItemsCloud(userId);

            data = data.ToList();
            foreach (var todo in data)
            {
                todo.Name = todo.Name.Replace(" ", string.Empty);
                todo.IsIdSync = true;
                todo.IsSync = true;
            }

            dataBase.Set<Todo>().AddRange(data);
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
        public void CreateItem(ToDoItemViewModel item)
        {
            Todo obj = item.ToTodo();

            dataBase.Set<Todo>().Add(obj);

            dataBase.SaveChanges();

            SyncObj(obj);

            //httpClient.PostAsJsonAsync(serviceApiUrl + CreateUrl, item)
            //    .Result.EnsureSuccessStatusCode();


        }

        /// <summary>
        /// Updates a todo.
        /// </summary>
        /// <param name="item">The todo to update.</param>
        public void UpdateItem(ToDoItemViewModel item)
        {
            httpClient.PutAsJsonAsync(serviceApiUrl + UpdateUrl, item)
                .Result.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Deletes a todo.
        /// </summary>
        /// <param name="id">The todo Id to delete.</param>
        public void DeleteItem(int id, int userId)
        {
            var obj = dataBase.Set<Todo>().FirstOrDefault(m => m.Id == id);
            if (obj == null)
                return;

            //if (!obj.IsSync)
            //{
            //    dataBase.Set<Todo>().Remove(obj);

            //    dataBase.SaveChanges();

            //    return;
            //}

            if (!obj.IsIdSync)
            {
                SyncIds(userId);
            }

            dataBase.Set<Todo>().Remove(obj);

            httpClient.DeleteAsync(string.Format(serviceApiUrl + DeleteUrl, id))
                .Result.EnsureSuccessStatusCode();

            dataBase.SaveChanges();


        }

        void Sync()
        {
            var dataDb = dataBase.Set<Todo>().Where(m => m.IsSync == false);

            for (int i = 0; i < dataDb.Count(); i++)
            {
                var obj = dataDb.ElementAt(i);

                obj.IsSync = true;

                dataBase.Entry(obj).State = System.Data.Entity.EntityState.Modified;

                SyncObj(obj);
            }

            dataBase.SaveChanges();
        }

        void SyncObj(object obj)
        {
            httpClient.PostAsJsonAsync(serviceApiUrl + CreateUrl, obj);
            // .Result.EnsureSuccessStatusCode();
        }

        void SyncIds(int userId)
        {
            var dataCloud = GetAllItemsCloud(userId);

            IEnumerable<Todo> dataDb = dataBase.Set<Todo>();

            int synCount = dataDb.Count(m => m.IsIdSync);

            dataCloud = dataCloud.Skip(synCount);

            dataBase.Set<Todo>().RemoveRange(dataBase.Set<Todo>().Where(m => m.IsIdSync == false));

            dataBase.Set<Todo>().AddRange(dataCloud);

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

            dataBase.SaveChanges();
        }
    }
}
