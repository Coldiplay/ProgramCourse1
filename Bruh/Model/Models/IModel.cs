using Bruh.Model.DBs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bruh.Model.Models
{
    [DBContext(typeof(ISampleDB))]
    public interface IModel
    {
        public int ID { get; internal set; }
        public bool AllFieldsAreCorrect { get; }
    }
}
