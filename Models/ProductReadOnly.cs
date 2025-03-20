using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System;

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
    public class ProductReadOnly
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }

        [JsonIgnore] // Prevents direct serialization of byte[]
        public byte[] RowVersion { get; set; } = default!;

        [NotMapped] // Ensures this is NOT stored in the database
        [JsonPropertyName("rowVersion")] // JSON property name in API responses
        public string RowVersionBase64
        {
            get => Convert.ToBase64String(RowVersion);
            set => RowVersion = Convert.FromBase64String(value);
        }
    }
}
