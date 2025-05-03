using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RDAExplorer;

namespace AnnoModificationManager4.RDA
{
    public class AMMRDAManager
    {
        public enum RDARequestType
        {
            None,
            SaveChanges,
            ReplaceByBackup
        }

        public event Action<object, RDAReader, RDARequestType> OnCommitted;
        public event Action<object> OnClear;
        public Dictionary<RDAReader, RDARequestType> _RequestTypes = new Dictionary<RDAReader, RDARequestType>();

        public bool Pending
        {
            get
            {
                return _RequestTypes.Count != 0;
            }
        }

        public AMMRDAManager()
        {

        }

        public void CommitChange(RDAReader file, RDARequestType type)
        {
            if (_RequestTypes.ContainsKey(file))
                _RequestTypes[file] = type;
            else
                _RequestTypes.Add(file, type);

            if (OnCommitted != null)
                OnCommitted(this, file, type);
        }

        public void Clear()
        {
            _RequestTypes.Clear();

            if (OnClear != null)
                OnClear(this);
        }
    }
}
