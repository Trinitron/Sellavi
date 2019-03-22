using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using SellAvi.Model.DataBaseModels;

namespace SellAvi.Services
{
    /// <summary>
    ///     The Interface defining methods for Create Employee and Read All Employees
    /// </summary>
    public interface IDataAccessService
    {
        DbChangeTracker ChangeTracker { get; }
        ObservableCollection<AvitoUser> GetUsers { get; }

        void DeleteUnsentMessages();
        void SaveContext();
        void RevertContext();
        T GetDetachedEntity<T>(T entityObject) where T : class;
        AvitoUser SaveUser(AvitoUser user);
        void UpdateUserFrom(AvitoUser userFrom, AvitoUser userTo);

        void DeleteUser(AvitoUser user);

        void TruncateAllCustomParams();
        void TruncateMessageHistory();
        void DeleteCustomParam(int id);
        AvitoUser GetLastAvitoUser();
        AvitoUser GetAvitoUser(int id);
    }

    /// <summary>
    ///     Class implementing IDataAccessService interface and implementing
    ///     its methods by making call to the Entities using CompanyEntities object
    /// </summary>
    public class DataAccessService : IDataAccessService
    {
        private readonly AvitoModel context;

        public DataAccessService()
        {
            context = new AvitoModel();
        }

        public DbChangeTracker ChangeTracker => context.ChangeTracker;


        public ObservableCollection<AvitoUser> GetUsers => new ObservableCollection<AvitoUser>(context.AvitoUsers);


        public void DeleteUnsentMessages()
        {
            throw new NotImplementedException();
        }

        public void SaveContext()
        {
            context.SaveChanges();
        }

        public void RevertContext()
        {
            foreach (var entry in ChangeTracker.Entries().Where(x => x.State != EntityState.Unchanged).ToList())
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.CurrentValues.SetValues(entry.OriginalValues);
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Unchanged;
                        break;
                }
        }

        public AvitoUser SaveUser(AvitoUser user)
        {
            context.AvitoUsers.Add(user);
            context.SaveChanges();
            return GetDetachedEntity(user);
        }

        public T GetDetachedEntity<T>(T entityObject) where T : class
        {
            //T entity = context.Set<T>().Create<T>();
            return (T) context.Entry(entityObject).GetDatabaseValues().ToObject();
        }

        public void UpdateUserFrom(AvitoUser userFrom, AvitoUser userTo)
        {
            context.Entry(userTo).CurrentValues.SetValues(userFrom);
            context.SaveChanges();
        }


        public void DeleteUser(AvitoUser user)
        {
            context.AvitoUsers.Remove(user);
            context.SaveChanges();
        }

        public void TruncateAllCustomParams()
        {
            throw new NotImplementedException();
        }

        public void TruncateMessageHistory()
        {
            throw new NotImplementedException();
        }

        public void DeleteCustomParam(int id)
        {
            throw new NotImplementedException();
        }


        public AvitoUser GetLastAvitoUser()
        {
            return context.AvitoUsers.AsNoTracking().OrderByDescending(x => x.Id).FirstOrDefault();
        }


        public AvitoUser GetAvitoUser(int id)
        {
            return context.AvitoUsers.FirstOrDefault(a => a.Id == id);
        }
    }
}