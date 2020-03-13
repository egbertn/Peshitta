using System;

namespace Peshitta.Infrastructure.Sqlite.Model
{
    public class Publication
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public bool? Searchable { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}