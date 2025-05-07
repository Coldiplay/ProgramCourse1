using Bruh.Model.DBs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bruh.Model.Models
{
    [DBContext(typeof(PeriodicitiesOfPaymentDB))]
    public class PeriodicityOfPayment : IModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
}
