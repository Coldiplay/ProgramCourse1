using Bruh.Model.DBs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bruh.Model.Models
{
    [DBContext(typeof(PeriodicitiesDB))]
    public class Periodicity : IModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
}
