using Bruh.Model.DBs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bruh.Model.Models
{
    [DBContext(typeof(BanksDB))]
    public class Bank : IModel
    {
        public int ID { get; set; }
        public string Title { get; set; } = string.Empty;

        public bool AllFieldsAreCorrect => !string.IsNullOrWhiteSpace(Title);
    }
}
