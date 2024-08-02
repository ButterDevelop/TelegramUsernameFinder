using TelegramUsernameFinder.Models;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace TelegramUsernameFinder.Repositories
{
    public class UsernameRepository
    {
        private readonly IMongoCollection<UsernameModel> _models;

        public UsernameRepository(string connectionString, string databaseName, string collectionName)
        {
            var client   = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _models      = database.GetCollection<UsernameModel>(collectionName);
        }

        public List<UsernameModel> GetAll()
        {
            return _models.Find(model => true).ToList();
        }

        public List<UsernameModel> GetAllFit(Expression<Func<UsernameModel, bool>> func)
        {
            return _models.Find(func).ToList();
        }

        public UsernameModel Get(string id)
        {
            return _models.Find(model => model.Id == id).FirstOrDefault();
        }

        public void Add(UsernameModel model)
        {
            if (model.Id is null || Get(model.Id) != null)
            {
                throw new Exception("ID is incorrect!");
            }

            _models.InsertOne(model);
        }

        public void Update(UsernameModel model)
        {
            _models.ReplaceOne(existingmodel => existingmodel.Id == model.Id, model);
        }

        public void Delete(string id)
        {
            _models.DeleteOne(model => model.Id == id);
        }
    }
}
