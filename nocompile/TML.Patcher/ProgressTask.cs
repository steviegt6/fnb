using System.Threading.Tasks;

namespace TML.Patcher
{
    public abstract class ProgressTask
    {
        public ActionableProgress ProgressReporter { get; } = new();

        protected ProgressTask()
        {
            ProgressReporter.Report(new ProgressNotification("Constructing object..."));
        }

        public abstract Task ExecuteAsync();
    }
}