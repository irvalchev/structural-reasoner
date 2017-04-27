using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.Logging
{
    public class Log
    {
        public const int MaxSize = 1000;
        private int trimmedCount;

        internal Log()
        {
            this.Items = new List<LogItem>();
        }

        public List<LogItem> Items { get; private set; }

        public event EventHandler<NewLogItemEventArgs> AddingNewItem;

        public void AddItem(LogType type, string message)
        {
            LogItem newItem = new LogItem(type, message);

            this.OnAddingNewItem(new NewLogItemEventArgs() { NewItem = newItem });

            if (Items.Count >= MaxSize)
            {
                this.TrimLog(500);
            }

            this.Items.Add(newItem);

#if DEBUG
            Debug.WriteLine(newItem.ToString());
#endif
        }

        /// <summary>
        /// Removes a number of items from the start of the log
        /// </summary>
        /// <param name="itemsToRemove">The number of items to remove</param>
        private void TrimLog(int itemsToRemove)
        {
            this.Items.RemoveRange(0, 500);
            trimmedCount++;
            this.AddItem(LogType.Info, String.Format("Log trimmed (Nr of trims: {0}). The first {1} items were deleted", trimmedCount, itemsToRemove));
        }

        protected virtual void OnAddingNewItem(NewLogItemEventArgs e)
        {
            EventHandler<NewLogItemEventArgs> handler = AddingNewItem;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }

    public class NewLogItemEventArgs : EventArgs
    {
        public LogItem NewItem { get; set; }
    }
}
