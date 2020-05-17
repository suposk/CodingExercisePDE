using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodingExercisePDE.Domain
{
    public class NumberDto
    {
        public Guid Id { get; set; }

        public int Number { get; set; }

        public DateTime CreatedAt { get; set; }

        public override string ToString()
        {
            return $"Number {Number}, Id:{Id}";
        }
    }
}
