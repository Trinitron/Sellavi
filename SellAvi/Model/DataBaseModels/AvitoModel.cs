using System.Data.Entity;
using System.Diagnostics;
using SellAvi.Views;

namespace SellAvi.Model.DataBaseModels
{
    public class AvitoModel : DbContext
    {
        public AvitoModel()
            : base("name=AvitoModel")
        {
            Database.Log = s => Debug.WriteLine(s);
            //Database.SetInitializer<AvitoModel>(new DropCreateDatabaseAlways<AvitoModel>());
            Database.SetInitializer(new AvitoModelInitializer());
        }


        public virtual DbSet<AvitoUser> AvitoUsers { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }

    public class AvitoModelInitializer : DropCreateDatabaseIfModelChanges<AvitoModel>
    {
        //DropCreateDatabaseAlways
        //CreateDatabaseIfNotExists
        //DropCreateDatabaseIfModelChanges

        public int SeedingOperationProcess { get; set; }

        protected override void Seed(AvitoModel context)
        {
            var s = new SplashScreen();
            s.Show();
            //ReinitializeDbValues(context);
            s.Close();
        }
    }
}