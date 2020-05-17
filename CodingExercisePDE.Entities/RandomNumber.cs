using System;
using System.Collections.Generic;
using System.Text;

namespace CodingExercisePDE.Entities
{
    public class RandomNumber
    {
        public RandomNumber()
        {
            
        }

        public RandomNumber(int number, DateTime? modifiedAt = null)
        {
            RandomNumberId = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            Number = number;
            ModifiedAt = modifiedAt;
        }

        public Guid RandomNumberId { get; set; }

        public int Number { get; set; }        

        public DateTime CreatedAt { get; set; }

        public DateTime? ModifiedAt { get; set; }
    }
}
