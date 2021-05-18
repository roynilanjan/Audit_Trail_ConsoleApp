using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Z.EntityFramework.Plus;
namespace Audit_trail_ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
			GenerateData();

			// Audit : Easily tracks changes, exclude/include entity or property and auto save audit entries in the database.
			// See also Context
			using (var context = new EntityContext())
			{
				var listToRemove = context.Customers.Where(x => x.IsActive == false).ToList();
				var listToModify = context.Customers.Where(x => x.IsActive == true).ToList();
				var listToAdd = new List<Customer>() { new Customer() { Name = "Customer_C", Description = "Description", IsActive = false } };

				context.Customers.AddRange(listToAdd); // add
				context.Customers.RemoveRange(listToRemove); // remove
				listToModify.First().Description = "Updated_A"; // modify



				var audit = new Audit();
				audit.CreatedBy = "ZZZ Projects"; // Optional
				context.SaveChanges(audit);
			}



			//Retrieve AuditEntries for specific item : You can filter the AuditEntries DbSet using Where method and providing either the item or the key.
			using (var context = new EntityContext())
			{
				var item = context.Customers.Where(x => x.Name == "Customer_C").ToList().First();
				
				context.AuditEntries.Where(item).ToList();
				/*FiddleHelper.WriteTable("All Entry", );

				FiddleHelper.WriteTable("All Entry", context.AuditEntries.Where<Customer>(item.CustomerID).ToList());
*/
				Console.WriteLine("All Entry ========" + context.AuditEntries.Where<Customer>(item.CustomerID).ToList());
				int id = item.CustomerID;

				//FiddleHelper.WriteTable("All Entry", context.AuditEntries.Where<Customer>(id).ToList());

				foreach (var entry in context.AuditEntries.Where<Customer>(id).ToList())
				{
					Console.WriteLine("Properties for Entry ID: " + entry.AuditEntryID, context.AuditEntryProperties.Where(x => x.AuditEntryID == entry.AuditEntryID).ToList());
				}
				Console.Read();
			}
		}

		public static void GenerateData()
		{

			using (var context = new EntityContext())
			{
				context.Customers.Add(new Customer() { Name = "Customer_A", Description = "Description", IsActive = true });
				context.Customers.Add(new Customer() { Name = "Customer_B", Description = "Description", IsActive = false });
				//Console.WriteLine("Customers = " + Customer)
				context.SaveChanges();
			}
		}

		public class EntityContext : DbContext
		{
			/*public EntityContext() : base(FiddleHelper.GetConnectionStringSqlServer())
			{

			}*/

			public DbSet<AuditEntry> AuditEntries { get; set; }
			public DbSet<AuditEntryProperty> AuditEntryProperties { get; set; }

			static  EntityContext() 
			{
				AuditManager.DefaultConfiguration.AutoSavePreAction = (context, audit) =>
				   // ADD "Where(x => x.AuditEntryID == 0)" to allow multiple SaveChanges with same Audit
				   (context as EntityContext).AuditEntries.AddRange(audit.Entries);
			}

			public DbSet<Customer> Customers { get; set; }
		}

		public class Customer
		{
			public int CustomerID { get; set; }
			public string Name { get; set; }
			public string Description { get; set; }
			public Boolean IsActive { get; set; }
		}
	}
    }

