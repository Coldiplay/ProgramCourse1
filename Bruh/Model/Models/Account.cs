﻿using Bruh.Model.DBs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bruh.Model.Models
{
    [DBContext(typeof(AccountsDB))]
    public class Account : IModel
    {
        public int ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public int CurrencyID { get; set; }
        public int? BankID { get; set; }
        public Currency Currency { get; set; }
        public Bank? Bank { get; set; }

        public string GetBalance => $"{Balance} {Currency?.Symbol}";

        public bool AllFieldsAreCorrect => !(string.IsNullOrWhiteSpace(Title) || Currency == null);
    }
}
