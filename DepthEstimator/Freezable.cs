using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scanning
{
    // Subclasses of Freezable can be frozen to assert immutability. Not thread-safe.
    abstract class Freezable
    {
        private bool isFrozen;

        // Returns true if this is frozen, false otherwise.
        public bool IsFrozen { get { return isFrozen; } }

        protected Freezable()
        {
            isFrozen = false;
        }

        // Freeze this.
        public void Freeze()
        {
            isFrozen = true;
        }

        // Subclasses must call this whenever they attempt to modify themselves. Subclasses can
        // override this method for different behavior. If not overriden, the default behavior is
        // to throw an InvalidOperationException when trying to modify a frozen object.
        protected void Modify()
        {
            if (isFrozen)
                throw new InvalidOperationException("Attempted to modify frozen object.");
        }
    }
}
