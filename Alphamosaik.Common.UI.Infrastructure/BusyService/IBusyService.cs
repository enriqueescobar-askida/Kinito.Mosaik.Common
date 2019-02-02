namespace Alphamosaik.Common.UI.Infrastructure
{
    public interface IBusyService
    {
        void ShowBusy();
        void HideBusy();
        bool IsBusy { get; set; }
    }
}