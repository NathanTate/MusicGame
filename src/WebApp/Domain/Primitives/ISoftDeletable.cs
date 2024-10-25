using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Primitives;
public interface ISoftDeletable
{
    public bool isDeleted { get; set; }
    public DateTime? DeletedOnUtc { get; set; }
}
