using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Trifolia.DB;

namespace Trifolia.Shared
{
    [Serializable]
    public class LookupCodeSystem
    {
        #region Properties

        private int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private string oid;

        public string Oid
        {
            get { return oid; }
            set { oid = value; }
        }

        #endregion

        public static List<LookupCodeSystem> GetCodeSystems()
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                return (from cs in tdb.CodeSystems
                        select new LookupCodeSystem()
                        {
                            Id = cs.Id,
                            Name = cs.Name,
                            Oid = cs.Oid
                        }).ToList();
            }
        }
    }
}
