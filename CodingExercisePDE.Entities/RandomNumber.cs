using System;
using System.Collections.Generic;
using System.Text;

namespace CodingExercisePDE.Entities
{
    public class RandomNumber
    {
        public Guid RandomNumberId { get; set; }

        public int Number { get; set; }        

        public DateTime CreatedAt { get; set; }

        public DateTime? ModifiedAt { get; set; }
    }
}
