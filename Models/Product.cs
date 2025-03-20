using System.ComponentModel.DataAnnotations;

namespace CqrsWithMediatR.Models
{
    /*

        Convention  	    EF Core Behavior
        ========================================================================
        Table Name  	    Uses pluralized class name as table name (Products)
        Primary Key 	    Looks for Id or {ClassName}Id
        Column Names    	Uses property name as column name
        Data Types  	    Infers from C# types 
                                (e.g., decimal → DECIMAL(18,2))
                                (e.g., string  → NVARCHAR(MAX)
        Required Fields	    Non-nullable properties are required (NOT NULL)
        Foreign Keys    	Infers {EntityName}Id as a foreign key

    */

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }

        [Timestamp]   // EF Core uses this for Optimistic Concurrency
        public byte[] RowVersion { get; set; } = default!;
    }
}
