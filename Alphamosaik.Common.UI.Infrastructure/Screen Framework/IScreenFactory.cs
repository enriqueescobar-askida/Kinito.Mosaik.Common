namespace Alphamosaik.Common.UI.Infrastructure
{
    public interface IScreenFactory
    {
        IScreen CreateScreen(object screenSubject);
    }
}